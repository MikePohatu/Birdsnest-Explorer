Param (
    [Parameter(Mandatory=$true)][string]$ScanID
)

Import-Module "$($PSScriptRoot)\..\AzureFunctions.psm1" -Force
Import-Module "$($PSScriptRoot)\..\WriteToNeo.psm1" -Force
$scriptFileName = $MyInvocation.MyCommand.Name

write-host "Getting AZ users"
#region
$path = "https://graph.microsoft.com/beta/users?`$select=id,displayName,givenName,surname,jobTitle,onPremisesSamAccountName,onPremisesSecurityIdentifier,mail,userPrincipalName,userType,accountEnabled,signInActivity,licenseAssignmentStates,createdDateTime"
$aadUsers = Get-GraphRequest -URI $path -All

$groupLicenseAssignments = @{}
$userLicenseAssignments = @()

$aadUsers | ForEach-Object {
    $user = $_
    if ($user.signInActivity.lastSignInDateTime) {
        $user.Add('lastSignIn', $user.signInActivity.lastSignInDateTime.toString())
    }
    else {
        $user.Add('lastSignIn', '')
    }

    if ($user.createdDateTime) {
        $user.Add('created', $user.createdDateTime.toString())
    }
    else {
        $user.Add('created', '')
    }

    foreach ($lic in $user.licenseAssignmentStates) {
        #ignore inactive licenses
        if (-not $lic.state -eq 'Active') { contine }

        if ($lic.assignedByGroup) {
            $licenseKey = "$($lic.skuId)_$($lic.assignedByGroup)"

            $licDeets = @{
                skuId=$lic.skuId
                target=$lic.assignedByGroup
                lastUpdatedDateTime = $lic.lastUpdatedDateTime
            }
            if (-not $groupLicenseAssignments.ContainsKey($licenseKey)) {
                $groupLicenseAssignments.Add($licenseKey, $licDeets)
            }
        }
        else {
            $userLicenseAssignments += @{
                skuId=$lic.skuId
                target=$user.id
                lastUpdatedDateTime = $lic.lastUpdatedDateTime
            }
        }
    }
}
$aadUsersQuery = @'
UNWIND $props AS prop 
MERGE (n:AZ_Object {id:prop.id}) 
SET n:AZ_User 
SET n.name = prop.displayName 
SET n.userprincipalname = prop.userPrincipalName 
SET n.onpremisessid = prop.onPremisesSecurityIdentifier 
SET n.onpremisessamaccountname = prop.onPremisesSamAccountName 
SET n.givenname = prop.givenName 
SET n.surname = prop.surname  
SET n.mail = prop.mail 
SET n.jobtitle = prop.jobTitle 
SET n.userType = prop.userType 
SET n.accountEnabled = prop.accountEnabled 
SET n.created = prop.created  
SET n.createdDateTime = null 
SET n.lastSignIn = prop.lastSignIn 
SET n.lastscan=$ScanID 
SET n.scannerid=$ScannerID 
RETURN n.name
'@

$op = @{
    message = "Writing $($aadUsers.Length) users"
    params = @{
        props = $aadUsers
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = $aadUsersQuery
}
Write-NeoOperations @op
#endregion

#region
$aadUsersSyncQuery = @'
MATCH (n:AZ_User) 
WITH collect(DISTINCT n) as aadusers 
UNWIND aadusers as aaduser 
MATCH(aduser:AD_User { id:aaduser.onpremisessid}) 
MERGE p = (aduser)-[r:AZ_SYNC]->(aaduser) 
SET r.lastscan = $ScanID 
SET r.scannerid = $ScannerID 
RETURN p
'@
$op = @{
    message = "Writing user sync relationships"
    params = @{
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = $aadUsersSyncQuery
}
Write-NeoOperations @op
#endregion

#region
<#
Licenses
example object:
@{
    skuId=$lic.skuId
    target=$lic.assignedByGroup
    lastUpdatedDateTime = $lic.lastUpdatedDateTime
}
#>
$licQuery = @'
UNWIND $props AS prop 
MERGE (n:AZ_Object {id:prop.target}) 
MERGE (sku:AZ_Object {id:prop.skuId}) 
MERGE (n)-[r:AZ_ASSIGNED_LICENSE]->(sku) 
SET r.lastUpdatedDateTime = prop.lastUpdatedDateTime 
SET r.lastscan=$ScanID 
SET r.scannerid=$ScannerID 
RETURN count(r)
'@
$op = @{
    message = "Writing $($groupLicenseAssignments.Values.Count) group license assignments"
    params = @{
        props = $groupLicenseAssignments.Values
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = $licQuery
}
Write-NeoOperations @op

$op = @{
    message = "Writing $($userLicenseAssignments.Length) user license assignments"
    params = @{
        props = $userLicenseAssignments
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = $licQuery
}
Write-NeoOperations @op
#endregion



Invoke-CleanupNodes -Label "AZ_Object" -Message "Cleaning up AZ $($scriptFileName.Replace('.ps1',''))" -ScanId $ScanID -ScannerID $scriptFileName
Invoke-CleanupRelationships -Label "AZ_ASSIGNED_LICENSE" -Message "Cleaning up AZ_ASSIGNED_LICENSE relationships" -ScanId $ScanID -ScannerID $scriptFileName
