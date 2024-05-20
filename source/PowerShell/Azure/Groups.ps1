Param (
    [Parameter(Mandatory=$true)][string]$ScanID
)

Import-Module "$($PSScriptRoot)\..\AzureFunctions.psm1" -Force
Import-Module "$($PSScriptRoot)\..\WriteToNeo.psm1" -Force
$scriptFileName = $MyInvocation.MyCommand.Name


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


Invoke-CleanupNodes -Label "AZ_Object" -Message "Cleaning up AZ $($scriptFileName.Replace('.ps1',''))" -ScanId $ScanID -ScannerID $scriptFileName
Invoke-CleanupRelationships -Label "AZ_MEMBER_OF" -Message "Cleaning up AZ_MEMBER_OF relationships" -ScanId $ScanID -ScannerID $scriptFileName
