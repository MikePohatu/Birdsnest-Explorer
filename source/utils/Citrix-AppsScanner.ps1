#$creds = Get-Credential
#-Credential $creds 

$zdc ='server'

write-host "Querying $zdc"

Function Get-CitrixApps {
    Param ([Parameter(Mandatory=$true)][string]$DataCollector)
    $resultobj = Invoke-Command -ComputerName $DataCollector -ScriptBlock {
        $results = new-object 'system.collections.generic.dictionary[[string],[object]]'

        Add-PSSnapin “Citrix.XenApp.Commands”
        $appslist = New-Object 'System.Collections.Generic.List[object]'
        $relationshipslist = New-Object 'System.Collections.Generic.List[object]'
        
        $FarmObj = Get-XAFarm | Select FarmName
        $AllApplications = Get-XAApplication

        ForEach ($Application in $AllApplications) {
            $AppGroupObj = Get-XAAccount -BrowserName $Application.BrowserName | Select *

            $appobj = new-object 'system.collections.generic.dictionary[[string],[string]]'
            $appobj.Add('BrowserName', $Application.BrowserName)
            $appobj.Add('ApplicationId',$Application.ApplicationId)
            $appobj.Add('Enabled', $Application.Enabled)
            $appobj.Add('FolderPath', $Application.FolderPath)
            $appobj.Add('LoadBalancingApplicationCheckEnabled', $Application.LoadBalancingApplicationCheckEnabled)
            $appobj.Add('ClientFolder', $Application.ClientFolder)

            $appslist.Add($appobj)

            foreach ($groupdn in $AppGroupObj.SearchPath) {
                if ([string]::IsNullOrEmpty($groupdn) -eq $false) {
                    $relobj = new-object 'system.collections.generic.dictionary[[string],[string]]'
                    $relobj.Add('dn',$groupdn)
                    $relobj.Add('appid',$Application.ApplicationId)
                    $relationshipslist.Add($relobj)
                }
            }
        }

        $results.Add('applications',$appslist)
        $results.Add('relationships',$relationshipslist)
        $results.Add('farmname',$FarmObj.FarmName)
        return $results
    }

    write-host "Farm: $($resultobj.farmname)"
    write-host "Found $($resultobj.applications.Count) applications"
    write-host "Found $($resultobj.relationships.Count) relationships"
    return $resultobj
}



Function WriteToNeo {
    Param (
        [Parameter(Mandatory=$true)][string]$NeoConfigPath,
        [Parameter(Mandatory=$true)][string]$Query,
        [System.Collections.Generic.IDictionary[[string],[object]]]$Parameters
    )
    $results = $null

    try {
        $neoconfig = Get-Content -Raw -Path $NeoConfigPath | ConvertFrom-Json
        $serverURL="http://localhost:7474/db/data/transaction/commit"

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
        #           '"statement" : "' + $Query + '",' +
        #           '"parameters" : ' + $paramsjson +
        #           '}]' +
        #        '}'    
        $body = $statement = new-object 'system.collections.generic.dictionary[[string],[object]]'
        $body.Add('statements',$statements)

        $bodyjson = $body |ConvertTo-Json -Depth 5

        
        # Call Neo4J HTTP EndPoint, Pass in creds & POST JSON Payload
        $response = Invoke-WebRequest -Uri $serverURL -Method POST -Body $bodyjson -credential $neo4jCreds -ContentType "application/json"
        $bodyjson | Out-File -FilePath 'c:\temp\ctxoutput.json'

    } 
    finally {
      
    }
    return $response
}





$resultsobj = Get-CitrixApps -DataCollector $zdc

write-host "Writing to neo4j database"

$installpath = 'C:\Tools\Temp\birdsnest\Scanners'


$nparamsobj = new-object 'system.collections.generic.dictionary[[string],[object]]'
$nparamsobj.Add('farmname',$resultsobj.farmname)
$nparamsobj.Add('applications',$resultsobj.applications)

write-host "Writing $($nparamsobj.applications.count) nodes from $($nparamsobj.farmname)"

$query = 'WITH $farmname as farm, $applications as applist ' +
        'MERGE (cfarm:CTX_Farm {name:farm}) ' +
        'WITH cfarm,farm,applist '+ 
        'UNWIND applist as app ' +
        'MERGE (n:CTX_Application {id:app.ApplicationId}) ' +
        'SET n.applicationid = app.ApplicationId ' +
        'SET n.browsername = app.BrowserName ' +
        'SET n.name = app.BrowserName ' +
        'SET n.enabled = app.Enabled ' +
        'SET n.folderpath = app.FolderPath ' +
        'SET n.path = app.FolderPath ' +
        'SET n.clientfolder = app.ClientFolder ' +
        'SET n.farm = farm ' +   
        'WITH n,cfarm ' +
        'MERGE (n)-[:PUBLISHED_FROM]->(cfarm) '+    
        'RETURN count(n) '

$noderesponse = WriteToNeo -NeoConfigPath "$($installpath)\neoconfig.json" -Query $query -Parameters $nparamsobj


$rparamsobj = new-object 'system.collections.generic.dictionary[[string],[object]]'
$rparamsobj.Add('relationships',$resultsobj.relationships)

write-host "Writing $($rparamsobj.relationships.count) relationships"

$query = 'UNWIND $relationships as rel ' +
        'MATCH (o:AD_Object {dn:rel.dn}) ' +
        'MATCH (a:CTX_Application {id:rel.appid}) '+
        'MERGE p=((o)-[:GIVES_ACCESS_TO]->(a)) ' +   
        'RETURN count(p) '

$relresponse = WriteToNeo -NeoConfigPath "$($installpath)\neoconfig.json" -Query $query -Parameters $rparamsobj