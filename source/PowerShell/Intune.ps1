Param (
    [Parameter(Mandatory=$true)][string]$ScanID
)

Import-Module "$($PSScriptRoot)\AzureFunctions.psm1" -Force
Import-Module "$($PSScriptRoot)\WriteToNeo.psm1"

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
                #figure out if the $assignment is an exclude or not
                $exclude = $false
                $exclValue = Get-TranslatedPropertyValue -InputObject $assignment -propertyPath $endpoint.assignment.excludeField
                if ($exclValue -eq $endpoint.assignment.excludeValue) {
                    $exclude = $true
                }

                $targetId = Get-TranslatedPropertyValue -InputObject $assignment -propertyPath $endpoint.assignment.target
                $sourceId = Get-TranslatedPropertyValue -InputObject $assignment -propertyPath $endpoint.assignment.source
                
                $assignmentObj = @{
                    targetId = $targetId
                    sourceId = $sourceId
                    id = $assignment.id
                    isExclude = $exclude
                }
                if ($exclude) { $excludes += $assignmentObj }
                else { $includes += $assignmentObj }
            }
        }
        Write-Progress -Activity $assignmentsActivity -Completed
    }
}

Write-Progress -Activity $activity -Completed
