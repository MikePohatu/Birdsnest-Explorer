class NeoConnection {
    static [string]$NeoURL
    static [string]$NeoConfigPath
    static $NeoConfig
    static $Neo4jCreds
}

Function Set-NeoConnection {
    Param (
        [Parameter(Mandatory=$true)][string]$NeoURL,
        [Parameter(Mandatory=$true)][string]$NeoConfigPath
    )

    [NeoConnection]::NeoConfigPath = $NeoConfigPath
    [NeoConnection]::NeoURL = $NeoURL
    [NeoConnection]::NeoConfig = Get-Content -Raw -Path $NeoConfigPath | ConvertFrom-Json
        
    Write-Host ([NeoConnection]::NeoConfig)
    $secPasswd = ConvertTo-SecureString $([NeoConnection]::NeoConfig.dbPassword) -AsPlainText -Force
    [NeoConnection]::Neo4jCreds = New-Object System.Management.Automation.PSCredential ($([NeoConnection]::NeoConfig.dbUsername), $secPasswd) 
}


Function WriteToNeo {
    Param (
        [Parameter(Mandatory=$true)][string]$Query,
        $Parameters,
        $Write = $true 
    )

    try {

        # Cypher query using parameters to pass in properties
        $statement = new-object 'system.collections.generic.dictionary[[string],[object]]'
        $statement.Add('statement',$Query)
        $statement.Add('parameters',$Parameters)
        $statements = new-object 'system.collections.generic.list[object]'
        $statements.Add($statement)

        $body = new-object 'system.collections.generic.dictionary[[string],[object]]'
        $body.Add('statements',$statements)

        $bodyjson = $body | ConvertTo-Json -Depth 10
        Write-Verbose $body
        
        # Call Neo4J HTTP EndPoint, Pass in creds & POST JSON Payload
        if ($Write) {
            if (-not ([NeoConnection]::NeoConfig)) {
                throw "Neo4j connection config not found, run Set-NeoConnection first"
            }
            $params = @{
                DisableKeepAlive =$true
                AllowUnencryptedAuthentication = $true
                Uri = ([NeoConnection]::NeoURL)
                Method = 'POST' 
                Body = $bodyjson 
                ContentType = "application/json"
            }
            # Call Neo4J HTTP EndPoint, Pass in creds & POST JSON Payload
            $response = Invoke-WebRequest @params -credential ([NeoConnection]::Neo4jCreds)
        }
        else {
            $response = @{
                Content = @'
{
    "data": "Write disabled"
}
'@
        }

        }
    } 
    finally {
      
    }
    return $response
}

#Based on https://github.com/csharpvitamins/CSharpVitamins.ShortGuid
function Get-ShortGuid {   
    $guid = New-Guid
    $Bytes = [System.Text.Encoding]::Unicode.GetBytes($guid)
    $EncodedText =[Convert]::ToBase64String($Bytes)
    $ShortGuid=$EncodedText.Replace('/','_').Replace('+','-').Substring(0, 22)
    $ShortGuid
} 



Function Write-NeoOperations {
    Param (
        [Parameter(Mandatory=$true)][string]$message,
        [Parameter(Mandatory=$true)][hashtable]$params,
        [Parameter(Mandatory=$true)][string]$query
    )

    Write-Host $message

    $response = WriteToNeo -Query $query -Parameters $params
    $content = $response.Content | ConvertFrom-Json
    if ($content.errors) {
        Write-Warning "Query reported an error:"
        $content.errors.message
    }
}

function Invoke-CleanupNodes {
    param (
        [Parameter(Mandatory=$true)][string]$Label,
        [Parameter(Mandatory=$true)][string]$Message,
        [Parameter(Mandatory=$true)][string]$ScanId,
        [Parameter(Mandatory=$true)][string]$ScannerId
    )

    $op = @{
        message = $Message
        params = @{
            ScanID = $ScanID
            ScannerID = $ScannerId
        }
        query = @"
MATCH (n:$Label { scannerid:'$ScannerID'}) 
WHERE n.lastscan <> '$ScanID' 
DETACH DELETE n 
RETURN count(n)
"@
    }

    Write-NeoOperations @op
}

function Invoke-CleanupRelationships {
    param (
        [Parameter(Mandatory=$true)][string]$Label,
        [Parameter(Mandatory=$true)][string]$Message,
        [Parameter(Mandatory=$true)][string]$ScanId,
        [Parameter(Mandatory=$true)][string]$ScannerId

    )

    $op = @{
        message = $Message
        params = @{
            ScanID = $ScanID
            ScannerID = $ScannerId
        }
        query = @"
MATCH ()-[r:$Label {scannerid:'$ScannerID'}]->() 
WHERE r.lastscan <> '$ScanID' 
DELETE r 
RETURN count(r)
"@
    }

    Write-NeoOperations @op
}