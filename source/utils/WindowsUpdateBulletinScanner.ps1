
Import-Module "$($PSScriptRoot)\WriteToNeo.psm1"
$neoURL="http://localhost:7474/db/data/transaction/commit"
$neoconf = "C:\Source\repos\BirdsNest\source\BirdsNest.Net\ADScanner\bin\Debug\neoconfig.json"
$bulletinsfile = "C:\Source\repos\BirdsNest\source\utils\MSUpdate_Bulletins.csv"

$headers = 'DatePosted','BulletinId','BulletinKB','Severity','Impact','Title','AffectedProduct','ComponentKB','AffectedComponent','Impact2','Severity2','Supersedes','Reboot','CVEs'

$bulletins = Get-Content -Path $bulletinsfile | Select-Object -Skip 1 | ConvertFrom-Csv -Header $headers

$scanid = [guid]::newguid().ToString()


write-host "Writing $($bulletins.count) bulletins"
$paramsobj = new-object 'system.collections.generic.dictionary[[string],[object]]'
$paramsobj.Add('bulletins', $bulletins)
$paramsobj.Add('scanid', $scanid)

$query = 'UNWIND $bulletins as bulletin ' +
    'MERGE (b:WU_Bulletin {id:bulletin.BulletinId}) ' +
    'SET b.name = bulletin.BulletinKB ' + 
    'SET b.bulletinid = bulletin.BulletinId ' + 
    'SET b.bulletinkb = bulletin.BulletinKB ' + 
    'SET b.severity = bulletin.Severity ' + 
    'SET b.impact = bulletin.Impact ' + 
    'SET b.title = bulletin.Title ' + 
    'SET b.affectedproduct = bulletin.AffectedProduct ' + 
    'SET b.componentkb = bulletin.ComponentKB ' + 
    'SET b.affectedcomponent = bulletin.AffectedComponent ' + 
    'SET b.title = bulletin.Title ' + 
    'SET b.cves = bulletin.CVEs ' + 
    'SET b.lastscan = $scanid ' +
    'RETURN count(b) '

$response = WriteToNeo -NeoConfigPath $neoconf -Query $query -Parameters $paramsobj -serverURL $neoUrl



$supersedes = New-Object 'System.Collections.Generic.List[object]'
$bulletins | ForEach-Object {
    $bulletin = $_
    $sup = $bulletin.Supersedes
    

    if ($sup) {
        $sups = $sup.Split(",")
        $sups | ForEach-Object {
            $supsplit = $_.Split("[")
            $bullsup = new-object 'system.collections.generic.dictionary[[string],[object]]'
            $bullsup.Add('bullid',$bulletin.BulletinId)
            $bullsup.Add('supid', $supsplit[0])
            $bullsup.Add('supkb', $supsplit[1].Replace("]",""))
            $supersedes.Add($bullsup)
        }
    }
    
}

write-host "Writing $($supersedes.count) supersedes"
$paramsobj = new-object 'system.collections.generic.dictionary[[string],[object]]'
$paramsobj.Add('supersedes', $supersedes)
$paramsobj.Add('scanid', $scanid)

$query = 'UNWIND $supersedes as super ' +
    'MATCH (b:WU_Bulletin {id:super.bullid}) ' +
    'MATCH (s:WU_Bulletin {id:super.supid}) ' +
    'MERGE (b)-[r:WU_SUPERSEDES]->(s) ' +
    'SET r.lastscan = $scanid ' +
    'RETURN count(b) '

$response = WriteToNeo -NeoConfigPath $neoconf -Query $query -Parameters $paramsobj -serverURL $neoUrl

Write-Host "Updating metadata"
$query = Get-UpdateMetaDataQuery -Type "WU_Bulletin"
$response = WriteToNeo -NeoConfigPath $neoconf -Query $query -serverURL $neoUrl

