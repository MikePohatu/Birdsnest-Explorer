
Function Get-TestData {
    $testlist = New-Object "System.Collections.Generic.List[object]" 
    
    For ($i=0; $i -lt 100; $i++) {
        $listobj = New-Object "System.Collections.Generic.Dictionary[string,object]" 
        $listobj.Add('testid',$i)  
        $listobj.Add('Test1',"LlamaLlamaDuck")
        $listobj.Add('Test2',"LlamaLlamaDuck")
        $listobj.Add('Test3',"LlamaLlamaDuck")
        $listobj.Add('Test4', "LlamaLlamaDuck")
        $testlist.Add($listobj)
    }

    write-host "$($testlist.count) elements created"
    return $testlist
}



Function WriteToNeo {
    Param (
        [Parameter(Mandatory=$true)][string]$NeoConfigPath,
        [Parameter(Mandatory=$true)][string]$NeoDriverPath,
        [Parameter(Mandatory=$true)][string]$Query,
        [object]$Parameters
    )
    write-host "Writing to neo4j database"
    $results = $null

    try {
        # Import DLLs
        Add-Type -Path "$NeoDriverPath"

        $neoconfig = Get-Content -Raw -Path $NeoConfigPath | ConvertFrom-Json
        $authToken = [Neo4j.Driver.V1.AuthTokens]::Basic($neoconfig.DB_Username,$neoconfig.DB_Password)

        $dbDriver = [Neo4j.Driver.V1.GraphDatabase]::Driver($neoconfig.DB_URI,$authToken)
        $session = $dbDriver.Session()

        if ($Parameters) { $results = $session.WriteTransaction({param ($tx) $tx.Run($Query,$Parameters) }) }
        else { $results = $session.WriteTransaction({param ($tx) $tx.Run($Query) }) }
    } 
    finally {
      $session.Dispose()
      $dbDriver.Dispose()
      
    }
    return $results
}




$data = Get-TestData

$installpath = 'C:\temp\TEST'



$paramsobj = New-Object "System.Collections.Generic.Dictionary[string,object]"     
$paramsobj.Add('env', "TEST")
$paramsobj.Add('testdata',$data)

$query = 'WITH $env as envname, $testdata as datalist ' +
        'MERGE (cenv:TEST_ENV {env:envname}) ' +
        'WITH cenv,envname,datalist '+
        'UNWIND datalist as d ' +
        'MERGE (n:TEST_Datum {id:d.testid}) ' +
        'SET n.Test1 = d.Test1 ' +
        'SET n.Test2 = d.Test2 ' +
        'SET n.Test3 = d.Test3 ' +
        'SET n.Test4 = d.Test4 ' +
        'SET n.env = envname ' +
        'WITH n,cenv ' +
        'MERGE (n)-[:PUBLISHED_FROM]->(cenv) '+
        'RETURN count(n) '


$r = WriteToNeo -NeoConfigPath "$($installpath)\neoconfig.json" -NeoDriverPath "$($installpath)\Neo4j.Driver.dll" -Query $query -Parameters $paramsobj
$r