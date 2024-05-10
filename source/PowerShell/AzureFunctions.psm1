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