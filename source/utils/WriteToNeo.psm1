Function WriteToNeo {
    Param (
        [Parameter(Mandatory=$true)][string]$NeoConfigPath,
        [Parameter(Mandatory=$true)][string]$serverURL,
        [Parameter(Mandatory=$true)][string]$Query,
        [System.Collections.Generic.IDictionary[[string],[object]]]$Parameters
    )
    $results = $null

    try {
        $neoconfig = Get-Content -Raw -Path $NeoConfigPath | ConvertFrom-Json
        

        $secPasswd = ConvertTo-SecureString $neoconfig.DB_Password -AsPlainText -Force
        $neo4jCreds = New-Object System.Management.Automation.PSCredential ($neoconfig.DB_Username, $secPasswd) 
        $paramsjson = $Parameters | ConvertTo-Json

        # Cypher query using parameters to pass in properties
        $statement = new-object 'system.collections.generic.dictionary[[string],[object]]'
        $statement.Add('statement',$Query)
        $statement.Add('parameters',$Parameters)
        $statements = new-object 'system.collections.generic.list[object]'
        $statements.Add($statement)

        #'{"statements" : [{' +
		#	        '"statement" : "' + $Query + '",' +
		#	        '"parameters" : ' + $paramsjson +
		#	        '}]' +
		#        '}'	
        $body = $statement = new-object 'system.collections.generic.dictionary[[string],[object]]'
        $body.Add('statements',$statements)

        $bodyjson = $body |ConvertTo-Json -Depth 5
        #write-host $bodyjson
        
        # Call Neo4J HTTP EndPoint, Pass in creds & POST JSON Payload
        $response = Invoke-WebRequest -DisableKeepAlive -Uri $serverURL -Method POST -Body $bodyjson -credential $neo4jCreds -ContentType "application/json"
        #$bodyjson | Out-File -FilePath 'c:\temp\ctxoutput.json'

    } 
    finally {
      
    }
    return $response
}