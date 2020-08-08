Import-Module "$($PSScriptRoot)\WriteToNeo.psm1"
$neoURL="http://localhost:7474/db/data/transaction/commit"
$neoconf = "C:\birdsnest\Scanners\config\neoconfig.json"
$scanid = Get-ShortGuid

Write-Host "Starting scan ID: $scanid"

write-host "Getting details from AD"
$resultsobj = Get-ADUser -Filter { Enabled -eq $true } -Properties SamAccountName, SID, Title, Department, Manager, Mobile `
    | select SamAccountName, @{N="SID";E={$_.SID.Value}}, Title, Department, Manager, Mobile

write-host "Writing settings"
$paramsobj = new-object 'system.collections.generic.dictionary[[string],[object]]'
$paramsobj.Add('users',$resultsobj)
$paramsobj.Add('scanid',$scanid)

$query = 'UNWIND $users as user' +
    ' MATCH (u:AD_User {id:user.SID})' +    
    " SET u.mobile = user.Mobile" + 
    " WITH u, user" + 
    ' MATCH (man:AD_User {dn:user.Manager})'+   
    ' MERGE (u)-[r:MANAGED_BY]->(man)'+  
    ' SET r.lastscan = $scanid' + 
    ' RETURN count(r)'

$response = WriteToNeo -NeoConfigPath $neoconf -Query $query -Parameters $paramsobj -serverURL $neoUrl
#$response

write-host "Cleaning up"
$paramsobj = new-object 'system.collections.generic.dictionary[[string],[object]]'
$paramsobj.Add('scanid',$scanid)

$query = 'MATCH ()-[r:MANAGED_BY]->()'+  
    ' WHERE r.lastscan <> $scanid'+
    ' DELETE r'

$response = WriteToNeo -NeoConfigPath $neoconf -Query $query -Parameters $paramsobj -serverURL $neoUrl
#$response 
