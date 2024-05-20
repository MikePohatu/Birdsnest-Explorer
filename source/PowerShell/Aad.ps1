Param (
    [Parameter(Mandatory=$true)][string]$ScanID
)

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



#region
    #devices
    #id,displayName,accountEnabled,approximateLastSignInDateTime,createdDateTime,deviceId,operatingSystem,operatingSystemVersion,trustType
 
write-host "Getting AZ devices"
$path = "https://graph.microsoft.com/beta/devices?`$select=id,displayName,accountEnabled,approximateLastSignInDateTime,createdDateTime,deviceId,operatingSystem,operatingSystemVersion,trustType"
$aadDevices = Get-GraphRequest -URI $path -All
$expandedDevices = @()
foreach ($aadDevice in $aadDevices) {
    if ($aadDevice.approximateLastSignInDateTime) { $lastsignin = $aadDevice.approximateLastSignInDateTime.toString() }
    else { $lastsignin = "" }

    if ($aadDevice.createdDateTime) { $created = $aadDevice.createdDateTime.toString() }
    else { $created = "" }

    $expandedDevices += @{
        id = $aadDevice.id
        displayName = $aadDevice.displayName
        accountEnabled = $aadDevice.accountEnabled
        approximateLastSignInDateTime = $lastsignin
        createdDateTime = $created
        deviceId = $aadDevice.deviceId
        operatingSystem = $aadDevice.operatingSystem
        operatingSystemVersion = $aadDevice.operatingSystemVersion
        trustType = $aadDevice.trustType
    }
}

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
    message = "Writing $($expandedDevices.Length) devices"
    params = @{
        props = $expandedDevices
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = $aadDevicesQuery
}
Write-NeoOperations @op
#endregion


#region
#device ownerships

write-host "Getting AZ device owners"
$path = "https://graph.microsoft.com/v1.0/users?`$filter=userType eq 'Member'&`$count=true"
$aadMembers = Get-GraphRequest -URI $path -All -ConsistencyEventual

$ownedDevices = @()
$activity = "Processing devices for $($aadMembers.count) users"

for ($i = 0; $i -lt $aadMembers.count; $i++) {
    $member = $aadMembers[$i]
    $path = "https://graph.microsoft.com/v1.0/users/$($member.id)/ownedDevices?`$select=id,deviceId,displayName"
    $memberDevices = @(Get-GraphRequest -URI $path)
    $memberDevices | ForEach-Object { 
        $ownedDevices += @{
            deviceID = $_.id
            userID = $member.id
        } 
    }
    Write-Progress -Activity $activity -PercentComplete (($i / $aadMembers.count) * 100)
}
Write-Progress -Activity $activity -Completed

$aadDeviceOwnersQuery = @'
UNWIND $props AS prop 
MATCH (dev:AZ_Object {id:prop.deviceID}) 
MATCH (user:AZ_Object {id:prop.userID}) 
MERGE (user)-[r:AZ_OWNS_DEVICE]->(dev) 
SET r.lastscan=$ScanID 
SET r.scannerid=$ScannerID 
RETURN count(r)
'@
    
$op = @{
    message = "Writing $($ownedDevices.Length) device ownerships"
    params = @{
        props = $ownedDevices
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = $aadDeviceOwnersQuery
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
MATCH(g:AZ_Object { id: prop.GroupId}) 
MATCH(m:AZ_Object{ id: prop.MemberId}) 
MERGE p = (m)-[r:AZ_MEMBER_OF]->(g) 
SET r.lastscan = $ScanID 
SET r.scannerid = $ScannerID 
SET r.layout='mesh' 
RETURN count(r)
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
        SET n:AZ_Deleted 
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


Invoke-CleanupRelationships -Label "AZ_ASSIGNED_LICENSE" -Message "Cleaning up AZ license assignments" -ScanId $ScanID -ScannerID $scriptFileName

