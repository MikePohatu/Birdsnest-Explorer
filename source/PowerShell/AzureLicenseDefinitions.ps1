Import-Module "$($PSScriptRoot)\WriteToNeo.psm1" -Force
Import-Module "$($PSScriptRoot)\AzureFunctions.psm1" -Force
Set-NeoConnection -neoURL "http://localhost:7474/db/data/transaction/commit" -neoconf "C:\birdsnest\Scanners\config\neoconfig.json"

$scanID = Get-ShortGuid 
$scriptFileName = $MyInvocation.MyCommand.Name

#region
$scriptPath = $PSScriptRoot
$licenseDetailsPath = "$($scriptPath)\Product_names_and_service_plan_identifiers_for_licensing.csv"
$licenseDetails = Get-Content "$licenseDetailsPath" -Raw | ConvertFrom-Csv

$products = @{}
$servicePlans = @{}
$servicePlanAssignments = @()

for ($i=0;$i -lt $licenseDetails.Count; $i++) {
    $detail = $licenseDetails[$i] 
    Write-Progress -Activity "Importing license details" -PercentComplete (($i/$licenseDetails.Count)*100) -CurrentOperation "$($detail.Product_Display_Name) | $($detail.Service_Plan_Name)"

    if (-not $products[$detail.GUID]) {
        $products.Add($detail.GUID, @{
            DisplayName = $detail.Product_Display_Name
            Id = $detail.String_Id
            Guid = $detail.GUID
        })
    }

    if (-not $servicePlans[$detail.Service_Plan_Id]) {
        $servicePlans.Add($detail.Service_Plan_Id, @{
            DisplayName = ($detail.Service_Plans_Included_Friendly_Names -creplace '\P{IsBasicLatin}').Trim()
            Name = $detail.Service_Plan_Name 
            Id = $detail.Service_Plan_Id
        })
    }
    $servicePlanAssignments += @{
        ProductId = $detail.GUID
        ServicePlanId = $detail.Service_Plan_Id
    }
}
Write-Progress -Activity "Importing license details" -Completed

$productsQuery = @'
UNWIND $props AS prop 
MERGE (n:AZ_Object {id:prop.Guid}) 
SET n:AZ_SKU 
SET n.name = prop.DisplayName 
SET n.subscriptionId = n.Id 
SET n.lastscan=$ScanID 
SET n.scannerid=$ScannerID 
RETURN count(n)
'@
$op = @{
    message = "Writing $($products.Values.Count) products"
    params = @{
        props = $products.Values
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = $productsQuery
}
Write-NeoOperations @op

$servicePlansQuery = @'
UNWIND $props AS prop 
MERGE (n:AZ_Object {id:prop.Id}) 
SET n:AZ_ServicePlan 
SET n.name = prop.DisplayName 
SET n.planname = prop.Name 
SET n.lastscan=$ScanID 
SET n.scannerid=$ScannerID 
RETURN count(n)
'@
$op = @{
    message = "Writing $($servicePlans.Values.Count) service plans"
    params = @{
        props = $servicePlans.Values
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = $servicePlansQuery
}
Write-NeoOperations @op


$servicePlanAssignmentsQuery = @'
UNWIND $props AS prop 
MATCH (plan:AZ_ServicePlan {id:prop.ServicePlanId}) 
MATCH (sku:AZ_SKU {id:prop.ProductId}) 
MERGE (plan)-[r:AZ_INCLUDED_IN]->(sku) 
SET r.lastscan=$ScanID 
SET r.scannerid=$ScannerID 
RETURN count(r)
'@
$op = @{
    message = "Writing $($servicePlanAssignments.Count) service plan assignments"
    params = @{
        props = $servicePlanAssignments
        ScanID = $scanID
        ScannerID = $scriptFileName
    }
    query = $servicePlanAssignmentsQuery
}
Write-NeoOperations @op
#endregion

$op = @{
    message ="Cleaning up"
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