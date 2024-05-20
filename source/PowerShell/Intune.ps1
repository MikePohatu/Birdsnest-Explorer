Param (
    [Parameter(Mandatory=$true)][string]$ScanID
)

Import-Module "$($PSScriptRoot)\AzureFunctions.psm1"
Import-Module "$($PSScriptRoot)\WriteToNeo.psm1"
$scriptFileName = $MyInvocation.MyCommand.Name

#region Add 'All Users' and 'All Devices' pseudo groups
$allUsersId = 'acacacac-9df4-4c7d-9d50-4ef0226f57a9'
$allDevicesId = 'adadadad-808e-44e2-905a-0b7873a8a531'
$op = @{
    message = "Writing 'All Users' pseudo group"
    params = @{}
    query = @"
MERGE (n:AZ_Object {id:'$allUsersId'}) 
SET n:AZ_Group 
SET n:AZ_Pseudo_Group 
SET n.name='All Users' 
SET n.descript='All Intune licensed users' 
RETURN n

"@
}
Write-NeoOperations @op

$op = @{
    message = "Writing 'All Devices' pseudo group"
    params = @{}
    query = @"
MERGE (n:AZ_Object {id:'$allDevicesId'}) 
SET n:AZ_Group 
SET n:AZ_Pseudo_Group 
SET n.name='All Devices' 
SET n.descript='All Intune devices' 
RETURN n
"@
}
Write-NeoOperations @op



$schemaFilePath = "$PSScriptRoot/intune_endpoints.json" 
$schema = @{} 
#import and convert to hashtable
(Get-Content $schemaFilePath | ConvertFrom-Json).psobject.properties | ForEach-Object { $schema[$_.Name] = $_.Value }
$activity = "Processing Intune endpoints"

$count = 0  
foreach ($key in $schema.Keys) {
    $count++
    $endpointName = $key
    Write-Progress -Activity $activity -PercentComplete (($count/$schema.Keys.Count)*100) -CurrentOperation $endpointName -Status $endpointName

    $endpoint = $schema[$endpointName]

    #main node gather and write piece
    $nodes = Invoke-ProcessSchemaList -EndpointDefinition $endpoint -ScanID $ScanID -ScannerID $scriptFileName

    #now deal with assignments
    $includes = @()
    $excludes = @()

    if ($endpoint.assignment) {
        $assignmentsActivity = "Processing Intune assignments for $endpointName"
        Write-Progress -Activity $assignmentsActivity
        for ($j=0; $j -lt $nodes.Length; $j++) {
            $node = $nodes[$j]
            Write-Progress -Activity $assignmentsActivity -PercentComplete (($j/$nodes.Length)*100) -Status $node.id

            $assignmentsPath = $endpoint.assignment.path.Replace('{{id}}', $node.id)
            $assignments = @(Get-GraphRequest -All -URI $assignmentsPath)

            foreach ($assignment in $assignments) {
                $targetType = Get-TranslatedPropertyValue -InputObject $assignment -propertyPath $endpoint.assignment.targetTypeField
                if (-not $targetType) {
                    Write-Error "Couldn't translate assignment target type, assignment id: $($assignment.id)"
                    contine
                }

                #figure out if the $assignment is an exclude or not
                $exclude = $false

                if ($targetType -eq $endpoint.assignment.excludeTarget) {
                    $exclude = $true
                    $targetId = Get-TranslatedPropertyValue -InputObject $assignment -propertyPath $endpoint.assignment.groupIdField
                }
                elseif ($targetType -eq $endpoint.assignment.allUsersTarget) {
                    $targetId = $allUsersId
                }
                elseif ($targetType -eq $endpoint.assignment.allDevicesTarget) {
                    $targetId = $allDevicesId
                }
                elseif ($targetType -eq $endpoint.assignment.groupTarget) {
                    $targetId = Get-TranslatedPropertyValue -InputObject $assignment -propertyPath $endpoint.assignment.groupIdField
                }
                else {
                    Write-Warning "Unknown assignment target: $targetType"
                }

                $assignmentObj = @{
                    targetId = $targetId
                    sourceId = $node.id
                    id = $assignment.id
                    isExclude = $exclude
                    intent = $assignment.intent
                }
                if ($exclude) { $excludes += $assignmentObj }
                else { $includes += $assignmentObj }
            }
        }

        #write to db
        $op = @{
            message = "Writing $($includes.Length) includes"
            params = @{
                props = $includes
                ScanID = $ScanID
                ScannerID = $scriptFileName
            }
            query = @"
UNWIND `$props AS prop 
MATCH (n:AZ_Object {id:prop.sourceId}) 
MATCH (g:AZ_Group {id:prop.targetId}) 
MERGE p=(g)-[r:$($endpoint.assignment.includeLabel)]->(n) 
SET r.lastscan=`$ScanID 
SET r.scannerid=`$ScannerID 
SET r.layout='mesh' 
SET r.id=prop.id 
SET r.intent = prop.intent 
RETURN p
"@
        }
        Write-NeoOperations @op

        $op = @{
            message = "Writing $($excludes.Length) excludes"
            params = @{
                props = $excludes
                ScanID = $ScanID
                ScannerID = $scriptFileName
            }
            query = @"
UNWIND `$props AS prop 
MATCH (n:AZ_Object {id:prop.sourceId}) 
MATCH (g:AZ_Group {id:prop.targetId}) 
MERGE p=(g)-[r:$($endpoint.assignment.excludeLabel)]->(n) 
SET r.lastscan=`$ScanID 
SET r.scannerid=`$ScannerID 
SET r.layout='mesh' 
SET r.id=prop.id 
SET r.intent = prop.intent 
RETURN p
"@
        }
        Write-NeoOperations @op

        Invoke-CleanupNodes -Label $endpoint.label -Message "Cleaning up $($endpoint.label) nodes" -ScanId $ScanID -ScannerID $scriptFileName       
        Invoke-CleanupRelationships -Label $endpoint.assignment.includeLabel -Message "Cleaning up $endpointName include relationships" -ScanId $ScanID -ScannerID $scriptFileName
        Invoke-CleanupRelationships -Label $endpoint.assignment.excludeLabel -Message "Cleaning up $endpointName exclude relationships" -ScanId $ScanID -ScannerID $scriptFileName
        Write-Progress -Activity $assignmentsActivity -Completed
    }
}

Write-Progress -Activity $activity -Completed

$op = @{
    message = "Writing 'All Users' pseudo group membership"
    params = @{
        ScanID = $ScanID
        ScannerID = $scriptFileName
    }
    query = @"
    MATCH (n:AZ_Object {id:'$allUsersId'})
    MATCH (u:AZ_User)-[*1..7]->(sp:AZ_ServicePlan) 
    WHERE sp.name CONTAINS 'Intune' 
    MERGE p=(u)-[r:AZ_PSEUDO_MEMBEROF]-(n) 
    SET r.lastscan = `$ScannerID 
    SET r.scannerid = `$ScanID 
    RETURN count(r)
"@
}
Write-NeoOperations @op

$op = @{
    message = "Writing 'All Devices' pseudo group membership"
    params = @{
        ScanID = $ScanID
        ScannerID = $scriptFileName
    }
    query = @"
MATCH (n:AZ_Object {id:'$allDevicesId'}) 
MATCH (d:AZ_Intune_Device) 
MERGE p=(d)-[r:AZ_INTUNE_MANAGED]-(n) 
SET r.lastscan = `$ScannerID 
SET r.scannerid = `$ScanID 
RETURN count(r)
"@
}
Write-NeoOperations @op

Invoke-CleanupRelationships -Label "AZ_PSEUDO_MEMBEROF" -Message "Cleaning up Intune psedo member of relationships" -ScanId $ScanID -ScannerID $scriptFileName
Invoke-CleanupRelationships -Label "AZ_INTUNE_MANAGED" -Message "Cleaning up Intune managed device relationships" -ScanId $ScanID -ScannerID $scriptFileName