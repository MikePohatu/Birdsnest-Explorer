Param (
    [Parameter(Mandatory=$true)][string]$ScanID
)

Import-Module "$($PSScriptRoot)\AzureFunctions.psm1"
Import-Module "$($PSScriptRoot)\WriteToNeo.psm1"

$scriptFileName = $MyInvocation.MyCommand.Name


function Add-PropertyToNode {
    param (
        [Parameter(Mandatory=$true)][string]$Name,
        [Parameter(Mandatory=$true)][AllowNull()]$Value,
        [Parameter(Mandatory=$true)][hashtable]$InputObject,
        [string]$DateTimeFormat = 'yyyy:MM:dd_HHmmss'
    )

    if ($null -eq $Value) {
        $InputObject.Add($Name, $null)
    }
    else {
        $valueType = $Value.GetType().Name

        switch -WildCard ( $valueType )
        {
            "DateTime" { 
                $InputObject.Add($Name, $Value.ToString($DateTimeFormat))
                break
            }
            "*Object*" {
                break
            }
            "HashTable" {
                break
            }
            default {
                $InputObject.Add($Name, $Value)
                break
            }
        }

        #Write-Host "$valueType - $Name : $Value"
    }
}

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

        #first check if we have defined a list of props using 'select'
        $selectedPropnames = $item.Keys | Sort-Object
        if ($endpoint.properties.select) {
            Write-Debug 'using selected props'
            $selectedPropnames = $endpoint.properties.select
        }

        foreach ($propname in $selectedPropnames) {
            #check prop names and make sure they are in the list of things
            if (($propname -in $propNames) -eq $false ) {
                $propNames += $propname
            }
            
            if ($endpoint.properties.rename -and $propname -in $endpoint.properties.rename.Keys) {
                Add-PropertyToNode -InputObject $newNode -Name $endpoint.properties.rename[$propname] -Value $item[$propname]
            }
            elseif ($propname.Contains('.')) {
                Write-Debug "Property contains period character: $propname, skipping"
            }
            else {
                Add-PropertyToNode -InputObject $newNode -Name $propname -Value $item[$propname]
            }
        }
        

        #now see if there are prop translations to do i.e. nested properties to unwind
        if ($endpoint.properties.translate) {
            foreach ($propPath in $endpoint.properties.translate.Keys) {
                $translatedValue = Get-TranslatedPropertyValue -propertyPath $propPath -InputObject $item
                $translatedName = $endpoint.properties.translate[$propPath]
                Add-PropertyToNode -InputObject $newNode -Name $translatedName -Value $translatedValue
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