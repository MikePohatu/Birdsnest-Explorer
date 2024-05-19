
 Function WriteToNeo {
    Param (
        [Parameter(Mandatory=$true)][string]$NeoConfigPath,
        [Parameter(Mandatory=$true)][string]$serverURL,
        [Parameter(Mandatory=$true)][string]$Query,
        $Parameters,
        $Write = $false 
    )

    try {
        $neoconfig = Get-Content -Raw -Path $NeoConfigPath | ConvertFrom-Json
        

        $secPasswd = ConvertTo-SecureString $neoconfig.dbPassword -AsPlainText -Force
        $neo4jCreds = New-Object System.Management.Automation.PSCredential ($neoconfig.dbUsername, $secPasswd) 

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
            $response = Invoke-WebRequest -AllowUnencryptedAuthentication -DisableKeepAlive -Uri $serverURL -Method POST -Body $bodyjson -credential $neo4jCreds -ContentType "application/json"
        }
        else {
            $response = @{
                data = "Write disabled"
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
MATCH (n:$Label { scannerid:$ScannerID}) 
WHERE n.lastscan <> $ScanID 
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
MATCH ()-[r:$Label {scannerid:$ScannerID}]->() 
WHERE r.lastscan <> $ScanID 
DELETE r 
RETURN count(r)
"@
    }

    Write-NeoOperations @op
}
