Function Get-GraphRequest {
    Param (
        [Parameter(ParameterSetName="Token")][string]$AccessToken,
        [ValidateSet("GET", "POST", "DELETE")][string]$Method = "GET",
        [Parameter(Mandatory=$true)][string]$URI,
        [switch]$All,
        [int]$Top
    )

    if ([string]::IsNullOrWhiteSpace($URI)) {
        throw "URI can't be empty"
    }

    if ($All) {
        $Top = "999" 
        #max out the page size if not already set
        if ($URI.Contains('$top=')) {
            $topURI = $URI
        }
        elseif (-not $URI.Contains('?')) {
            $topURI = "$($URI)?`$top=$Top"
        }
        else {
            $topURI = "$($URI)&`$top=$Top"
        }
        Write-Verbose "Getting all pages" 
    }
    elseif ($Top) {
        if (-not $URI.Contains('?')) {
            $topURI = "$($URI)?`$top=$Top"
        }
        else {
            $topURI = "$($URI)&`$top=$Top"
        }
    }
    else {
        $topURI = $URI
    }

    
    Write-Verbose "Making graph $Method request to URI: $topURI"

    #create the header
    if ($AccessToken) {
        $graphGetParams = @{
            Headers     = @{
                "Content-Type"  = "application/json"
                Authorization = "Bearer $($AccessToken)"
            }
            Method      = $Method
            URI = $topURI
            #ErrorAction = "SilentlyContinue"
        }
    } 
    else {
        $graphGetParams = @{
            Headers     = @{
                "Content-Type"  = "application/json"
            }
            Method      = $Method
            URI = $topURI
            #ErrorAction = "SilentlyContinue"
        }
    }

    $allPages = @()
    do {
        $response = Invoke-MgGraphRequest @graphGetParams -ResponseHeadersVariable responseHeaders
        $allPages += $response.value
        $graphGetParams.URI = $response.'@odata.nextLink'

    } until ((-not $All) -or (-not $response.'@odata.nextLink'))
    
        
    Write-Output $allPages
}

class NeoConnection {
    static [string]$neoURL
    static [string]$neoconf
}

Function Set-NeoConnection {
    Param (
        [Parameter(Mandatory=$true)][string]$neoURL,
        [Parameter(Mandatory=$true)][string]$neoconf
    )

    [NeoConnection]::neoconf = $neoconf
    [NeoConnection]::neoUrl = $neoURL
}

Function Write-NeoOperations {
    Param (
        [Parameter(Mandatory=$true)][string]$message,
        [Parameter(Mandatory=$true)][hashtable]$params,
        [Parameter(Mandatory=$true)][string]$query
    )

    Write-Host $message
    
    Write-Verbose "neoconf: $([NeoConnection]::neoconf)"
    Write-Verbose "neoUrl: $([NeoConnection]::neoUrl)"
    $response = WriteToNeo -NeoConfigPath "$([NeoConnection]::neoconf)" -serverURL "$([NeoConnection]::neoUrl)" -Query $query -Parameters $params
    $content = $response.Content | ConvertFrom-Json
    if ($content.errors) {
        Write-Warning "Query reported an error:"
        $content.errors.message
    }
}


function Add-PropertyToNode {
    param (
        [Parameter(Mandatory=$true)][string]$Name,
        [Parameter(Mandatory=$true)][AllowNull()]$Value,
        [Parameter(Mandatory=$true)][hashtable]$InputObject,
        [string]$DateTimeFormat = 'yyyy-MM-dd_HHmmss'
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
            "String" {
                #strip non-latin characters so it doesn't break json import
                #https://stackoverflow.com/a/68328388
                $stripped = ($Value -creplace '\P{IsBasicLatin}').Trim()
                $InputObject.Add($Name, $stripped)
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
    if ($InputObject.ContainsKey($propertyPath)) { return $InputObject[$propertyPath] }
    
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


function Invoke-ProcessSchemaList {
    param (
        [Parameter(Mandatory=$true)]$EndpointDefinition,
        [Parameter(Mandatory=$true)][string]$ScanID,
        [Parameter(Mandatory=$true)][string]$ScannerID
    )

    if ($null -eq $EndpointDefinition -or [string]::IsNullOrWhiteSpace($EndpointDefinition.listPath)) { Continue }
    $items = @(Get-GraphRequest -All -URI $EndpointDefinition.listPath)

    #skip if nothing comes back
    if ($items.Count -eq 0) { 
        Write-Information "Endpoint $EndpointDefinitionName returned no data"
        return 
    }

    #sanitise the data
    $propNames = @()
    $nodes = @()

    foreach ($item in $items) {
        #start the node ** id is always required
        $newNode = @{}

        #first check if we have defined a list of props using 'select'
        $selectedPropnames = $item.Keys | Sort-Object
        if ($EndpointDefinition.properties.select) {
            Write-Debug 'using selected props'
            $selectedPropnames = $EndpointDefinition.properties.select
        }

        foreach ($propname in $selectedPropnames) {
            #check prop names and make sure they are in the list of things
            if (($propname -in $propNames) -eq $false ) {
                $propNames += $propname
            }
            
            if ($EndpointDefinition.properties.rename -and $propname -in $EndpointDefinition.properties.rename.Keys) {
                Add-PropertyToNode -InputObject $newNode -Name $EndpointDefinition.properties.rename[$propname] -Value $item[$propname]
            }
            elseif ($propname.Contains('.')) {
                Write-Debug "Property contains period character: $propname, skipping"
            }
            else {
                Add-PropertyToNode -InputObject $newNode -Name $propname -Value $item[$propname]
            }
        }
        

        #now see if there are prop translations to do i.e. nested properties to unwind
        if ($EndpointDefinition.properties.translate) {
            foreach ($propPath in $EndpointDefinition.properties.translate.Keys) {
                $translatedValue = Get-TranslatedPropertyValue -propertyPath $propPath -InputObject $item
                $translatedName = $EndpointDefinition.properties.translate[$propPath]
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
MERGE (n:AZ_Object {id:prop.id}) 
SET n:$($EndpointDefinition.label) 
$($neo4jNodeProps)SET n.lastscan=`$ScanID 
SET n.scannerid=`$ScannerID 
RETURN count(n)
"@



    $op = @{
        message = "Writing node list: "
        params = @{
            props = $nodes
            ScanID = $ScanID
            ScannerID = $ScannerID
        }
        query = $query
    }
    #Write-NeoOperations @op

    return $nodes
}
