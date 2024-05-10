Param (
    [Parameter(Mandatory=$true)][string]$ScanID
)

$scriptFileName = $MyInvocation.MyCommand.Name

#region
write-host "Getting AZ users"
$path = "https://graph.microsoft.com/beta/users?`$select=id,displayName,givenName,surname,jobTitle,onPremisesSamAccountName,onPremisesSecurityIdentifier,mail,userPrincipalName,userType,accountEnabled,signInActivity,licenseAssignmentStates,createdDateTime"
$aadUsers = Get-GraphRequest -URI $path -All

$licenseAssignments = @{}

$aadUsers | ForEach-Object {
    $user = $_
    if ($user.signInActivity) {
        $user.Add('lastSignIn', $user.signInActivity.lastSignInDateTime)
    }
    else {
        $user.Add('lastSignIn', '')
    }

    foreach ($lic in $user.licenseAssignmentStates) {
        
        if ($lic.assignedByGroup) {
            $objectId = $lic.assignedByGroup
        }
        else {
            $objectId = $user.id
        }
        
        $key = "$($lic.skuId)_$($objectId)"

        if (-not $licenseAssignments.ContainsKey($key)) {
            $licenseAssignments.Add($key, @{
                objectId = $objectId
                skuId = $lic.skuId
                state = $lic.state
                lastUpdatedDateTime = $lic.lastUpdatedDateTime
            })
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
SET n.createdDateTime = prop.createdDateTime 
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
write-host "Getting AZ licenses"

<#
$licenseAssignments = @()
for ($i=0; $i -lt $aadUsers.Count; $i++) {
    $user = $aadUsers[$i]
    $path = "https://graph.microsoft.com/beta/users/$($user.id)/licenseDetails"
    $userLicenses = Get-GraphRequest -URI $path
    Write-Progress -Activity "Getting license details" -PercentComplete (($i/$aadUsers.Count)*100) -CurrentOperation "$($user.displayName)"

    foreach ($lic in $userLicenses) {
        $licenseAssignments += @{
            userId = $user.id
            skuId = $lic.skuId
            id = $lic.id 
            skuPartNumber = $lic.skuPartNumber
        }
    }
}
Write-Progress -Activity "Getting license details" -Completed

$licensesQuery = @'
UNWIND $props AS prop 
MATCH (user:AZ_Object {id:prop.userId}) 
MATCH (sku:AZ_Object {id:prop.skuId}) 
MERGE (user)-[r:AZ_ASSIGNED_LICENSE]->(sku) 
SET r.id = prop.id 
SET r.lastscan=$ScanID 
SET r.scannerid=$ScannerID 
RETURN count(r)
'@
$op = @{
    message = "Writing $($licenseAssignments.Length) license assignments"
    params = @{
        props = $licenseAssignments
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = $licensesQuery
}
Write-NeoOperations @op
#endregion
#>


#region 
write-host "Getting AZ Subscribed SKUs"

$path = "https://graph.microsoft.com/beta/subscribedSkus"
$subs = Get-GraphRequest -URI $path


$subsQuery = @'
UNWIND $props AS prop 
MERGE (account:AZ_Object {id:prop.accountId}) 
WITH prop, account 
MATCH (sub:AZ_Object {id:prop.skuId}) 
SET account:AZ_Account 
SET account.name = prop.accountName 
SET sub.skuPartNumber = prop.skuPartNumber 
SET account.lastscan=$ScanID 
SET account.scannerid=$ScannerID 
MERGE (sub)-[r:AZ_SUBSCRIBED_TO]->(account) 
SET r.id = prop.id 
SET r.lastscan=$ScanID 
SET r.scannerid=$ScannerID 
RETURN count(r)
'@
$op = @{
    message = "Writing $($subs.Length) SKU subscriptions"
    params = @{
        props = $subs
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = $subsQuery
}
Write-NeoOperations @op
#endregion


$licenseAssignmentsQuery = @'
UNWIND $props AS prop 
MATCH (o:AZ_Object {id:prop.objectId}) 
MATCH (sku:AZ_Object {id:prop.skuId}) 
MERGE (o)-[r:AZ_ASSIGNED_LICENSE]->(sku) 
SET r.id = prop.id 
SET r.state = prop.state 
SET r.lastUpdatedDateTime = prop.lastUpdatedDateTime 
SET r.lastscan=$ScanID 
SET r.scannerid=$ScannerID 
RETURN count(r)
'@
$op = @{
    message = "Writing $($licenseAssignments.Values.Count) license assignments"
    params = @{
        props = $licenseAssignments.Values
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = $licenseAssignmentsQuery
}
Write-NeoOperations @op


#region
    #devices
    #id,displayName,accountEnabled,approximateLastSignInDateTime,createdDateTime,deviceId,operatingSystem,operatingSystemVersion,trustType
 
write-host "Getting AZ devices"
$path = "https://graph.microsoft.com/beta/devices?`$select=id,displayName,accountEnabled,approximateLastSignInDateTime,createdDateTime,deviceId,operatingSystem,operatingSystemVersion,trustType"
$aadDevices = Get-GraphRequest -URI $path -All
$aadDevicesQuery = @'
UNWIND $props AS prop 
MERGE (n:AZ_Object {id:prop.id}) 
SET n:AZ_Device 
SET n.name = prop.displayName 
SET n.enabled = prop.accountEnabled 
SET n.approxLastSignIn = prop.approximateLastSignInDateTime 
SET n.createdDateTime = prop.createdDateTime  
SET n.deviceId = prop.deviceId 
SET n.operatingSystem = prop.operatingSystem  
SET n.operatingSystemVersion = prop.operatingSystemVersion 
SET n.trustType = prop.trustType 
SET n.lastscan=$ScanID 
SET n.scannerid=$ScannerID 
RETURN count(n)
'@

$op = @{
    message = "Writing $($aadDevices.Length) devices"
    params = @{
        props = $aadDevices
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = $aadDevicesQuery
}
Write-NeoOperations @op
#endregion


#region
write-host "Getting AZ groups"
$path = "https://graph.microsoft.com/beta/groups?`$select=id,displayName,description,onPremisesSamAccountName,onPremisesSecurityIdentifier,mail,mailEnabled,isAssignedToRole,securityEnabled,groupTypes"
$aadGroups = Get-GraphRequest -URI $path -All

#work out the group type
$aadGroups | ForEach-Object {
    $group = $_
    $group.Add('dynamic', $false)
    if ($group.securityEnabled) {
        $group.Add('grouptype', 'Security')
    }
    else {
        $group.Add('grouptype', 'Distribution')
    }

    if (-not $group.onPremisesSecurityIdentifier) {
        $group.onPremisesSecurityIdentifier = [string]::Empty
    }
    
    $group.groupTypes | ForEach-Object {
        if ($_ -eq "DynamicMembership") {
            $group.dynamic = $true
        }
        elseif ($_ -eq "Unified") {
            $group.grouptype = 'Microsoft365'
        }
    }
}
$aadGroupsQuery = @'
UNWIND $props AS prop 
MERGE (n:AZ_Object {id:prop.id}) 
SET n:AZ_Group 
SET n.name = prop.displayName 
SET n.description = prop.description 
SET n.dynamic = prop.dynamic 
SET n.mail = prop.mail 
SET n.mailenabled = prop.mailEnabled 
SET n.type = prop.grouptype 
SET n.onpremisessamaccountname = prop.onPremisesSamAccountName 
SET n.onpremisessid = prop.onPremisesSecurityIdentifier 
SET n.lastscan=$ScanID 
SET n.scannerid=$ScannerID 
WITH n 
MATCH (adgroup:AD_Group {id: n.onpremisessid}) 
MERGE (adgroup)-[r:AZ_SYNC]->(n)  
SET r.lastscan=$ScanID 
SET r.scannerid=$ScannerID 
SET r.layout='mesh' 
RETURN n.name 
'@
$op = @{
    message = "Writing $($aadGroups.Length) groups"
    params = @{
        props = $aadGroups
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = $aadGroupsQuery
}
Write-NeoOperations @op
#endregion

#region MEMBERSHIPS
$memberships = @()
$count = 0
$activity = 'Getting group memberships'

Write-Progress -Activity $activity -PercentComplete 0

if ($aadGroups) {
    $aadGroups | ForEach-Object {
        $count++
        Write-Progress -Activity $activity -PercentComplete (($count/$aadGroups.Count)*100) -CurrentOperation $_.displayName
        $groupId = $_.id
        $path = "https://graph.microsoft.com/beta/groups/$groupId/members"
        $members = Get-GraphRequest -URI $path -All  #| ForEach-Object { New-Object -TypeName PSObject -Property $_ }

        $members | ForEach-object {
            $memberships += [PSCustomObject]@{
                GroupId = $groupId
                MemberId = $_.id
            }
        }
    }
}
else {
    throw "AZ Groups couldn't be gathered"
}
Write-Progress -Activity $activity -Completed

$membershipsQuery = @'
UNWIND $props AS prop 
MATCH(g:AZ_Group { id: prop.GroupId}) 
MATCH(m:AZ_Object{ id: prop.MemberId}) 
MERGE p = (m)-[r:AZ_MEMBER_OF]->(g) 
SET r.lastscan = $ScanID 
SET r.scannerid = $ScannerID 
SET r.layout='mesh' 
RETURN p
'@

$op = @{
    message = "Writing $($memberships.Length) memberships"
    params = @{
        props = $memberships
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = $membershipsQuery
}
Write-NeoOperations @op
#endregion

$op = @{
    message ="Cleaning up AZ objects"
    params = @{
            ScanID = $scanID
            ScannerID = $scriptFileName
        }
    query = @'
        MATCH(n:AZ_Object { scannerid:$ScannerID}) 
        WHERE n.lastscan <> $ScanID 
        DETACH DELETE n 
        RETURN count(n)
'@
}
Write-NeoOperations @op

$op = @{
    message = "Cleaning up AZ group memberships"
    params = @{
        ScanID = $scanID
        ScannerID = $scriptFileName
    } 
    query = @'
        MATCH ()-[r:AZ_MEMBER_OF {scannerid:$ScannerID}]->() 
        WHERE r.lastscan <> $ScanID 
        DELETE r 
        RETURN count(r)
'@
}
Write-NeoOperations @op


$op = @{
    message ="Writing group membership counts"
    params = @{
            ScanID = $scanID
            ScannerID = $scriptFileName
        }
    query = @'
            MATCH (n:AZ_Group {scannerid:$ScannerID}) 
            SET n.member_count = 0 
            WITH n 
            MATCH (o:AZ_Object)-[r:AZ_MEMBER_OF]->(n) 
            WHERE o.scannerid = $ScannerID 
            WITH n,count(r) AS i 
            SET n.member_count = i 
            RETURN i
'@
}
Write-NeoOperations @op

$op = @{
    message = "Writing group membership counts"
    params = @{
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = @'
        MATCH (o)-[:AZ_MEMBER_OF *]->(g:AZ_Group) 
        WHERE o:AZ_User OR o:AZ_Device 
        WITH collect(DISTINCT o) as nodes, g 
        SET g.scope = size(nodes) 
        RETURN g
'@
}
Write-NeoOperations @op

$op = @{
    message = "Cleaning up AZ license assignments" 
    params = @{
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = @'
        MATCH ()-[r:AZ_ASSIGNED_LICENSE {scannerid:$ScannerID}]->() 
        WHERE r.lastscan <> $ScanID 
        DELETE r 
        RETURN count(r)
'@
}
Write-NeoOperations @op

$op = @{
    message = "Writing license assignment counts"
    params = @{
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = @'
        MATCH (o:AZ_User)-[*]->(s:AZ_SKU) 
        WITH collect(DISTINCT o) as nodes, s 
        SET s.scope = size(nodes) 
        RETURN s
'@
}
Write-NeoOperations @op

