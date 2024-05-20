Param (
[Parameter(Mandatory=$true)][string]$ScanID
)

Import-Module "$($PSScriptRoot)\..\AzureFunctions.psm1" -Force
Import-Module "$($PSScriptRoot)\..\WriteToNeo.psm1" -Force
$scriptFileName = $MyInvocation.MyCommand.Name

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


Invoke-CleanupNodes -Label "AZ_Object" -Message "Cleaning up AZ $($scriptFileName.Replace('.ps1',''))" -ScanId $ScanID -ScannerID $scriptFileName
Invoke-CleanupRelationships -Label "AZ_OWNS_DEVICE" -Message "Cleaning up AZ_OWNS_DEVICE relationships" -ScanId $ScanID -ScannerID $scriptFileName
