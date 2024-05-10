Param (
    [Parameter(Mandatory=$true)][string]$ScanID
)

$scriptFileName = $MyInvocation.MyCommand.Name


#region Get Named / Trusted Locations
$locationResult = Get-GraphRequest -All -URI "https://graph.microsoft.com/beta/identity/conditionalAccess/namedLocations"
$namedLocations = @()
foreach ($loc in $locationResult) {
    if ($loc.ipRanges) {
        $ipRanges = $loc.ipRanges | ForEach-Object { $_.cidrAddress }
    }
    else {
        $ipRanges = [string]::Empty
    }

    if ($loc.countriesAndRegions) {
        $countriesAndRegions = $loc.countriesAndRegions -join ', '
    }
    else {
        $countriesAndRegions = [string]::Empty
    }

    $namedLocations += @{
        name = $loc.displayName
        id = $loc.id
        isTrusted = $loc.isTrusted
        ipRanges = $ipRanges
        countriesAndRegions = $countriesAndRegions
        includeUnknownCountriesAndRegions = $loc.includeUnknownCountriesAndRegions
        countryLookupMethod = $loc.countryLookupMethod
    }
}
$namedLocationsQuery = @'
UNWIND $props AS prop 
MERGE (n:AZ_Named_Location {id:prop.id}) 
SET n:AZ_Object 
SET n.name = prop.name  
SET n.isTrusted = prop.isTrusted 
SET n.ipRanges = prop.ipRanges 
SET n.countriesAndRegions = prop.countriesAndRegions 
SET n.includeUnknown = prop.includeUnknownCountriesAndRegions 
SET n.countryLookupMethod = prop.countryLookupMethod 
SET n.lastscan=$ScanID 
SET n.scannerid=$ScannerID 
RETURN n.name
'@
$op = @{
    message = "Writing $($namedLocations.Length) named locations"
    params = @{
        props = $namedLocations
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = $namedLocationsQuery
}
Write-NeoOperations @op
#endregion

#region Get directory roles
$roles = Get-GraphRequest -URI "https://graph.microsoft.com/beta/directoryRoles"
$rolesQuery = @'
UNWIND $props AS prop 
MERGE (n:AZ_Object {id:prop.id}) 
SET n:AZ_Role 
SET n.name = prop.displayName 
SET n.roleTemplateId = prop.roleTemplateId 
SET n.description = prop.description  
SET n.lastscan=$ScanID 
SET n.scannerid=$ScannerID 
RETURN n.name
'@
$op = @{
    message = "Writing $($roles.Length) roles"
    params = @{
        props = $roles
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = $rolesQuery
}
Write-NeoOperations @op
#endregion

#region Service Principals
$servicePrincipals = Get-GraphRequest -All -URI "https://graph.microsoft.com/beta/servicePrincipals?`$select=id,displayName,appDescription,appId,servicePrincipalType,signInAudience"
$servicePrincipalsQuery = @'
UNWIND $props AS prop 
MERGE (n:AZ_Object {id:prop.id}) 
SET n:AZ_ServicePrincipal 
SET n.name = prop.displayName 
SET n.appId = prop.appId 
SET n.description = prop.appDescription 
SET n.servicePrincipalType = prop.servicePrincipalType 
SET n.signInAudience = prop.signInAudience 
SET n.lastscan=$ScanID 
SET n.scannerid=$ScannerID 
RETURN n.name
'@
$op = @{
    message = "Writing $($servicePrincipals.Length) service principals"
    params = @{
        props = $servicePrincipals
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = $servicePrincipalsQuery
}
Write-NeoOperations @op
#endregion

#region Applications
$applications = Get-GraphRequest -All -URI "https://graph.microsoft.com/beta/applications?`$select=id,displayName,description,appId,signInAudience"
$applicationsQuery = @'
UNWIND $props AS prop 
MERGE (n:AZ_Object {id:prop.id}) 
SET n:AZ_Registered_Application 
SET n.name = prop.displayName 
SET n.description = prop.description 
SET n.appId = prop.appId 
SET n.signInAudience = prop.signInAudience 
SET n.lastscan=$ScanID 
SET n.scannerid=$ScannerID 
RETURN n.name
'@
$op = @{
    message = "Writing $($applications.Length) applications"
    params = @{
        props = $applications
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = $applicationsQuery
}
#endregion

#region Applications->Service Principal link
$appPrincipalLinkQuery = @'
MATCH (s:AZ_ServicePrincipal) 
MATCH (a:AZ_Registered_Application {appId:s.appId}) 
MERGE (s)-[r:AZ_ServicePrincipalFor]->(a) 
SET r.lastscan=$ScanID 
SET r.scannerid=$ScannerID 
RETURN count(r) 
'@
$op = @{
    message = "Connecting applications to service principals"
    params = @{
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = $appPrincipalLinkQuery
}
Write-NeoOperations @op
#endregion


#region Get Conditional Access Policies
$caPolicies = Get-GraphRequest -All -URI "https://graph.microsoft.com/v1.0/identity/conditionalAccess/policies"
$caPoliciesQuery = @'
UNWIND $props AS prop 
MERGE (n:AZ_Object {id:prop.id}) 
SET n:AZ_CA_Policy 
SET n.name = prop.displayName 
SET n.templateId = prop.templateId 
SET n.state = prop.state 
SET n.lastscan=$ScanID 
SET n.scannerid=$ScannerID 
RETURN n.name
'@
$op = @{
    message = "Writing $($caPolicies.Length) CA policies"
    params = @{
        props = $caPolicies
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = $caPoliciesQuery
}
Write-NeoOperations @op
#endregion

#region Process CA policy includes/excludes
$caPolicyIncludes = @()
$caPolicyExcludes = @()

foreach ($pol in $caPolicies) {
    $includes = @(
        $pol.conditions.applications.includeApplications,
        $pol.conditions.users.includeUsers,
        $pol.conditions.users.includeGroups,
        $pol.conditions.users.includeRoles
    )
    $includes | ForEach-Object {
        $_ | ForEach-Object {
            $caPolicyIncludes += @{
                PolicyID = $pol.id
                ObjectID = $_
            }
        }
    }

    $excludes = @(
        $pol.conditions.applications.excludeApplications,
        $pol.conditions.users.excludeUsers,
        $pol.conditions.users.excludeGroups,
        $pol.conditions.users.excludeRoles
    )

    $excludes | ForEach-Object {
        $_ | ForEach-Object {
            $caPolicyExcludes += @{
                PolicyID = $pol.id
                ObjectID = $_
            }
        }
    }    
}
$caIncludesQuery = @'
UNWIND $props AS prop 
MATCH (p:AZ_CA_Policy {id:prop.PolicyID}) 
MATCH (o:AZ_Object {id:prop.ObjectID}) 
MERGE (o)-[r:AZ_INCLUDED_IN_POLICY]->(p) 
SET r.lastscan=$ScanID 
SET r.scannerid=$ScannerID 
RETURN count(r)
'@
$op = @{
    message = "Writing $($caPolicyIncludes.Length) CA policy includes"
    params = @{
        props = $caPolicyIncludes
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = $caIncludesQuery
}
Write-NeoOperations @op

$caExcludesQuery = @'
UNWIND $props AS prop 
MATCH (p:AZ_CA_Policy {id:prop.PolicyID}) 
MATCH (o:AZ_Object {id:prop.ObjectID}) 
MERGE (o)-[r:AZ_EXCLUDED_IN_POLICY]->(p) 
SET r.lastscan=$ScanID 
SET r.scannerid=$ScannerID 
RETURN count(r)
'@
$op = @{
    message = "Writing $($caPolicyExcludes.Length) CA policy excludes"
    params = @{
        props = $caPolicyExcludes
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = $caExcludesQuery
}
Write-NeoOperations @op
#endregion


#region cleanup
$op = @{
    message = "Cleaning up CA scanner objects"
    params = @{
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = @'
        MATCH (n:AZ_Object {scannerid:$ScannerID}) 
        WHERE n.lastscan <> $ScanID 
        DETACH DELETE n 
        RETURN count(n)
'@
}
Write-NeoOperations @op

$op = @{
    message = "Cleaning up CA scanner links"
    params = @{
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = @'
        MATCH (:AZ_Object)-[r]->(:AZ_Object) 
        WHERE r.scannerid = $ScannerID AND r.lastscan <> $ScanID 
        DELETE r 
        RETURN count(r)
'@
}
Write-NeoOperations @op

#endregion
