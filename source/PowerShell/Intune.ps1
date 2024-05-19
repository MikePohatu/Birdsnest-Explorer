Param (
    [Parameter(Mandatory=$true)][string]$ScanID
)

Import-Module "$($PSScriptRoot)\AzureFunctions.psm1" -Force
Import-Module "$($PSScriptRoot)\WriteToNeo.psm1" -force
$scriptFileName = $MyInvocation.MyCommand.Name

$allUsersId = 'acacacac-9df4-4c7d-9d50-4ef0226f57a9'
$allDevicesId = 'adadadad-808e-44e2-905a-0b7873a8a531'

$schemaFilePath = "$PSScriptRoot/intune_endpoints.json" 
$schema = Get-Content $schemaFilePath | ConvertFrom-Json -Depth 5 -AsHashtable
$activity = "Processing Intune endpoints"

$count = 0  
foreach ($key in $schema.Keys) {
    $count++
    $endpointName = $key
    Write-Progress -Activity $activity -PercentComplete (($count/$schema.Keys.Count)*100) -CurrentOperation $endpointName -Status $endpointName

    $endpoint = $schema[$endpointName]

    $nodes = Invoke-ProcessSchemaList -EndpointDefinition $endpoint -ScanID $ScanID -ScannerID $scriptFileName
    $includes = @()
    $excludes = @()

    if ($endpoint.assignment) {
        $assignmentsActivity = "Processing Intune assignments for $endpointName"
        Write-Progress -Activity $assignmentsActivity
        for ($j=0; $j -lt $nodes.Length; $j++) {
            $node = $nodes[$j]
            Write-Progress -Activity $assignmentsActivity -PercentComplete (($j/$nodes.Length)*100) -CurrentOperation $node.id -Status $node.id

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
        Invoke-CleanupRelationships -Label $endpoint.assignment.includeLabel -Message "Cleaning up $($endpoint.assignment.includeLabel) relationships" -ScanId $ScanID -ScannerID $scriptFileName
        Invoke-CleanupRelationships -Label $endpoint.assignment.excludeLabel -Message "Cleaning up $($endpoint.assignment.excludeLabel) relationships" -ScanId $ScanID -ScannerID $scriptFileName
        Write-Progress -Activity $assignmentsActivity -Completed
    }
}

Write-Progress -Activity $activity -Completed
