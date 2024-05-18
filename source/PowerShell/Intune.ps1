Param (
    [Parameter(Mandatory=$true)][string]$ScanID
)

Import-Module "$($PSScriptRoot)\AzureFunctions.psm1"
Import-Module "$($PSScriptRoot)\WriteToNeo.psm1"

$scriptFileName = $MyInvocation.MyCommand.Name


function Get-TranslatedPropertyValue {
    param (
        [Parameter(Mandatory=$true)][string]$propertyPath,
        [Parameter(Mandatory=$true)][hashtable]$InputObject
    )
    
    if ([string]::IsNullOrWhiteSpace($propertyPath)) { Write-Error "Property path can't be emmpty" }
    $splitPath = $propertyPath.Split('.')
    if ($splitPath.Count -eq 1) {
        return $InputObject[$propertyPath] 
    }
    else {
        #check if the property in first part of path exists
        $parentProp = $splitPath[0]
        if ($InputObject[$parentProp]) {
            #remove the first part of the path and recursively call again
            $popFirst = $splitPath[-$($splitPath.Count - 1)..-1]
            return Get-TranslatedPropertyValue -propertyPath ($popFirst -join '.') -InputObject $InputObject[$parentProp]
        }
        else {
            Write-Warning "Property not found $parentProp"
            return $null
        }
    }
}


$schema = Get-Content "$PSScriptRoot/intune_endpoints.json" | ConvertFrom-Json -Depth 5 -AsHashtable
$activity = "Processing endpoints"

$count = 0  
foreach ($key in $schema.Keys) {
    $count++
    $endpointName = $key
    Write-Progress -Activity $activity -PercentComplete (($count/$schema.Keys.Count)*100) -CurrentOperation $endpointName -Status $endpointName

    $endpoint = $schema[$endpointName]
    if ($null -eq $endpoint -or [string]::IsNullOrWhiteSpace($endpoint.listPath)) { Continue }
    $items = @(Get-GraphRequest -All -URI $endpoint.listPath)

    #skip if nothing comes back
    if ($items.Count -eq 0) { 
        Write-Information "Endpoint $endpointName returned no data"
        continue 
    }

    #sanitise the data
    $propNames = @()
    $nodes = @()

    foreach ($item in $items) {
        #start the node ** id is always required
        $newNode = @{}
        
        foreach ($propname in $item.Keys) {
            #check prop names and make sure they are in the list of things
            if (($propname -in $propNames) -eq $false ) {
                $propNames += $propname
            }
            
            if ($endpoint.properties.rename -and $propname -in $endpoint.properties.rename.Keys) {
                $propval = $item[$propname]
                $translatedPropName = $endpoint.properties.rename[$propname]
                $newNode.Add($translatedPropName,$propval)
            }
            elseif ($propname.Contains('.')) {
                Write-Debug "Property contains period character: $propname, skipping"
            }
            else {
                $propval = $item[$propname]
                if ($propval -and $propval.GetType().IsValueType) {
                    $newNode.Add($propname,$propval)
                }
            }
        }

        #now see if there are prop translations to do i.e. nested properties to unwind
        if ($endpoint.properties.translate) {
            foreach ($propPath in $endpoint.properties.translate.Keys) {
                $translatedValue = Get-TranslatedPropertyValue -propertyPath $propPath -InputObject $item

                $newNode.Add($endpoint.properties.translate[$propPath],$translatedValue)
            }
        }

        $nodes += $newNode
    }


    #create neo4j query props for each property in the return object
    $neo4jNodeProps = ""
    foreach ($propname in $propNames) {
        # '.' character not supported
        if ($propname.Contains( ".")) { continue }
        $neo4jNodeProps += "SET n.$($propname.ToLower()) = prop.$propname `n"
    }

    $query = @"
UNWIND `$props AS prop 
MERGE (n:$($endpoint.label) {id:prop.id}) 
SET n:AZ_Object 
$($neo4jNodeProps)SET n.lastscan=`$ScanID 
SET n.scannerid=`$ScannerID 
RETURN count(n)
"@
}

Write-Progress -Activity $activity -Completed