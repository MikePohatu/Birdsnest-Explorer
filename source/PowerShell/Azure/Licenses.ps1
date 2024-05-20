Param (
    [Parameter(Mandatory=$true)][string]$ScanID
)

Import-Module "$($PSScriptRoot)\..\AzureFunctions.psm1" -Force
Import-Module "$($PSScriptRoot)\..\WriteToNeo.psm1" -Force
$scriptFileName = $MyInvocation.MyCommand.Name

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


Invoke-CleanupNodes -Label "AZ_Object" -Message "Cleaning up AZ $($scriptFileName.Replace('.ps1',''))" -ScanId $ScanID -ScannerID $scriptFileName
Invoke-CleanupRelationships -Label "AZ_ASSIGNED_LICENSE" -Message "Cleaning up AZ_ASSIGNED_LICENSE relationships" -ScanId $ScanID -ScannerID $scriptFileName
