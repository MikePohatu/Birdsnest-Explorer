Param (
    [Parameter(Mandatory=$true)][string]$ScanID
)

$scriptFileName = $MyInvocation.MyCommand.Name

#get assignments

$apps = @()
$includes = @()
$excludes = @()

Write-Host "Processing apps" -ForegroundColor Cyan

$items = @(Get-GraphRequest -All -URI "https://graph.microsoft.com/beta/deviceAppManagement/mobileApps?`$select=displayName,isAssigned,id,assignments,publisher")

#$items | ForEach-Object {
for ($i = 0; $i -lt $items.Length; $i++) {    
    $item = $items[$i]
    $itemName=$item.displayName
    Write-Progress -Activity "Assignments for $itemName" -PercentComplete (($i/$items.Length)*100) -CurrentOperation "ID: $($assignment.id) | Include: $groupid"
            
    $apps += New-Object PSObject -Property @{
        #strip non-latin characters so it doesn't break json import
        #https://stackoverflow.com/a/68328388
        displayName = ($item.displayName -creplace '\P{IsBasicLatin}').Trim()
        isAssigned = $item.isAssigned
        id = $item.id
        publisher = ($item.publisher -creplace '\P{IsBasicLatin}').Trim()
        type = $item["@odata.type"].Replace("#microsoft.graph.","")
    }
    
    
    #region Assignments processing
    #Write-Host $itemName -ForegroundColor Cyan
    $assignmentsPath = "https://graph.microsoft.com/beta/deviceAppManagement/mobileApps/$($item.id)/assignments"
    $itemAssignments = @(Get-GraphRequest -All -URI $assignmentsPath)

    #foreach ($assignment in $itemAssignments) {
    for ($j = 0; $j -lt $itemAssignments.Length; $j++) {    
        $assignment = $itemAssignments[$j]
        $exclude = $false

        $groupid = $assignment.id.Split('_')[0]

        #deviceCompliancePolicies and deviceConfigurations have different schemas
        if ($assignment.target) {
            if ($assignment.target["@odata.type"] -eq "#microsoft.graph.exclusionGroupAssignmentTarget") {
                $exclude = $true
                Write-Verbose "EXCLUDE: #microsoft.graph.exclusionGroupAssignmentTarget matched"
            }
        }
        else {
            if ($assignment.ExcludeGroup) {
                $exclude = $true
                Write-Verbose 'EXCLUDE: $assignment.ExcludeGroup matched'
            }
        }


        if ($groupid) {
            Write-Progress -Activity "Assignments for $itemName" -PercentComplete (($j/$itemAssignments.Length)*100) -CurrentOperation "ID: $($assignment.id) | Include: $groupid"
                
            if ($exclude) {
                #Write-Host "ID: $($assignment.id) | Exclude: $groupid" -ForegroundColor DarkYellow
                $excludes += @{
                    GroupID = $groupid
                    AppID = $item.id
                    AssignmentID = $assignment.id
                }
                #Write-Verbose ($assignment | ConvertTo-Json)
            }
            else {
                #Write-Host "ID: $($assignment.id) | Include: $groupid" -ForegroundColor Green
                $includes += @{
                    GroupID = $groupid
                    AppID = $item.id
                    AssignmentID = $assignment.id
                }
                #Write-Verbose ($assignment | ConvertTo-Json)
            }
        }
    }
    Write-Progress -Activity "Assignments for $itemName" -Completed
    #endregion

}


$op = @{
        message = "Writing $($apps.Length) apps"
        params = @{
            props = $apps
            ScanID = $scanID
            ScannerID = $scriptFileName
        }
        query = @'
            UNWIND $props AS prop 
            MERGE (n:AZ_Intune_App {id:prop.id}) 
            SET n.name = prop.displayName 
            SET n.publisher = prop.publisher 
            SET n.isassigned = prop.isAssigned 
            SET n.type = prop.type 
            SET n.lastscan=$ScanID 
            SET n.scannerid=$ScannerID 
            RETURN count(n)
'@
}
Write-NeoOperations @op

$op = @{
        message = "Writing $($includes.Length) includes"
        params = @{
            props = $includes
            ScanID = $scanID
            ScannerID = $scriptFileName
        }
        query = @'
            UNWIND $props AS prop 
            MATCH (n:AZ_Intune_App {id:prop.AppID}) 
            MATCH (g:AZ_Group {id:prop.GroupID}) 
            MERGE p=(g)-[r:AZ_ASSIGNMENT_INCLUDE]->(n) 
            SET r.lastscan=$ScanID 
            SET r.scannerid=$ScannerID 
            SET r.layout='mesh' 
            SET r.id=prop.AssignmentID
            RETURN p
'@
}
Write-NeoOperations @op

$op = @{
        message = "Writing $($excludes.Length) excludes"
        params = @{
            props = $excludes
            ScanID = $scanID
            ScannerID = $scriptFileName
        }
        query = @'
            UNWIND $props AS prop 
            MATCH (n:AZ_Intune_App {id:prop.AppID}) 
            MATCH (g:AZ_Group {id:prop.GroupID}) 
            MERGE p=(g)-[r:AZ_ASSIGNMENT_EXCLUDE]->(n) 
            SET r.lastscan=$ScanID 
            SET r.scannerid=$ScannerID 
            SET r.layout='mesh' 
            SET r.id=prop.AssignmentID
            RETURN count(p)
'@ 
}
Write-NeoOperations @op

$op = @{
        message = "Cleaning up apps"
        params = @{
            ScanID = $scanID
            ScannerID = $scriptFileName
        }
        query = @'
            MATCH (n:AZ_Intune_App { scannerid:$ScannerID}) 
            WHERE n.lastscan <> $ScanID 
            DETACH DELETE n 
            RETURN count(n)
'@
}
Write-NeoOperations @op

$op = @{
        message = "Cleaning up assignment excludes"
        params = @{
            ScanID = $scanID
            ScannerID = $scriptFileName
        }
        query = @'
            MATCH ()-[r:AZ_ASSIGNMENT_EXCLUDE {scannerid:$ScannerID}]->() 
            WHERE r.lastscan <> $ScanID 
            DELETE r 
            RETURN count(r)
'@
}
Write-NeoOperations @op

$op = @{
        message = "Cleaning up assignment includes"
        params = @{
            ScanID = $scanID
            ScannerID = $scriptFileName
        }
        query = @'
            MATCH ()-[r:AZ_ASSIGNMENT_INCLUDE {scannerid:$ScannerID}]->() 
            WHERE r.lastscan <> $ScanID 
            DELETE r 
            RETURN count(r)
'@
}
Write-NeoOperations @op

