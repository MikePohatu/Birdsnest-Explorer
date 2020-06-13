[Hashtable]$Locations = @{
        "PROD" = "\\domain.local\netlogon\Prod\Flex_Profile"
        "UAT" = "\\domain.local\netlogon\Uat\Flex_Profile"
    }


Import-Module "$($PSScriptRoot)\WriteToNeo.psm1"
$neoURL="http://localhost:7474/db/data/transaction/commit"
$neoconf = "C:\birdsnest\Scanners\neoconfig.json"
$ShortDomain = 'AD'



Function Get-UemSettings {
    Param(
        [Parameter(Mandatory=$true)][String]$Path
    )

    #$SettingFiles = Get-ChildItem "$($Path)\general" | ?{$_.Name -ne "FlexRepository"} | %{Get-ChildItem -LiteralPath $_.FullName -Recurse} | ?{$_.Extension -eq ".ini"}
    $SettingFiles = Get-ChildItem "$($Path)\general" -Recurse | ?{($_.Extension -eq ".ini") -or ($_.Extension -eq ".xml")}

    $Output = foreach ($S in $SettingFiles){
        #$Content = Get-Content -LiteralPath $S.FullName
        #$Title = $Content | %{if($_ -match '^Title = (.*)$'){$Matches[1]}}
        #$PROCESSLINE = $Content | ?{$_ -match '^ProcessCriteria='}
        
        if ($S.Extension -eq ".ini"){
            $Content = Get-Content -LiteralPath $S.FullName
            $Title = $Content | %{if($_ -match '^Title = (.*)$'){$Matches[1]}}
            $SettingType = "Personalization"
            $PROCESSLINE = $Content | ?{$_ -match '^ProcessCriteria='}
        }
        elseif($S.Extension -eq ".xml"){
            [XML]$XML = Get-Content -LiteralPath $S.FullName
            #if (!($XML.userEnvironmentSettings.setting.disabled -eq "1")){
                $Title = $XML.userEnvironmentSettings.setting.label
                $SettingType = if($XML.userEnvironmentSettings.setting.type){$XML.userEnvironmentSettings.setting.type}else{$S.Directory.Name}
                $PROCESSLINE = $XML.userEnvironmentSettings.conditions.InnerXml
            #}
        }
        
        
        $AllGroups = $PROCESSLINE | %{([regex]'(<not>(?i)<member[^></]*/></not>)|((?i)<member[^></]*/>)').Matches($_)}
        $Includes = $AllGroups | ?{!$_.Groups[1].Success} | %{$_.Groups[2].Value}
        $Excludes = $AllGroups | ?{$_.Groups[1].Success} | %{$_.Groups[1].Value}
        $IncludesSid = if($Includes -match 's="([^"]*)'){$Matches[1]}
        $IncludesGID = if($Includes -match 'g="([^"]*)'){$Matches[1]}
        $ExcludesSid = if($Excludes -match 's="([^"]*)'){$Matches[1]}
        $ExcludesGID = if($Excludes -match 'g="([^"]*)'){$Matches[1]}
        $OUT = New-Object -TypeName psobject
        $OUT | Add-Member -NotePropertyName Environment -NotePropertyValue $KEY -Force
        $OUT | Add-Member -NotePropertyName SettingType -NotePropertyValue $SettingType -Force
        $OUT | Add-Member -NotePropertyName Title -NotePropertyValue $Title -Force
        $OUT | Add-Member -NotePropertyName Filename -NotePropertyValue $s.Name -Force
        $OUT | Add-Member -NotePropertyName FullName -NotePropertyValue $s.FullName -Force
        $OUT | Add-Member -NotePropertyName Includes -NotePropertyValue $Includes -Force
        $OUT | Add-Member -NotePropertyName Excludes -NotePropertyValue $Excludes -Force
        $OUT | Add-Member -NotePropertyName IncludesSid -NotePropertyValue $IncludesSid -Force
        $OUT | Add-Member -NotePropertyName IncludesGID -NotePropertyValue $IncludesGID -Force
        $OUT | Add-Member -NotePropertyName ExcludesSid -NotePropertyValue $ExcludesSid -Force
        $OUT | Add-Member -NotePropertyName ExcludesGID -NotePropertyValue $ExcludesGID -Force
        $OUT
    }

    $SettingInfo = $Output | Select-Object -Property Environment,Title,Filename,FullName,SettingType

    $IncludeSIDOutput = @()
    $IncludeGroupOutput = @()

    foreach ($A in $($Output | ?{$_.Includes})){
        foreach ($I in $A.Includes){
            if($I -match 's="([^"]*)'){
                $OUT = New-Object -TypeName psobject
                $OUT | Add-Member -NotePropertyName "FullName" -NotePropertyValue $A.FullName
                $OUT | Add-Member -NotePropertyName "SID" -NotePropertyValue $Matches[1]
                $IncludeSIDOutput += $OUT
            }
            elseif($I -match 'g="([^"]*)'){
                $OUT = New-Object -TypeName psobject
                $OUT | Add-Member -NotePropertyName "FullName" -NotePropertyValue $A.FullName
                $OUT | Add-Member -NotePropertyName "Group" -NotePropertyValue $Matches[1]
                $IncludeGroupOutput += $OUT
            }
            else{
                Write-Host "Something went wring with $($A.FullName)" -ForegroundColor Yellow
            }
        }
    }

    $ExcludeSIDOutput = @()
    $ExcludeGroupOutput = @()

    foreach ($A in $($Output | ?{$_.Excludes})){
        foreach ($I in $A.Excludes){
            if($I -match 's="([^"]*)'){
                $OUT = New-Object -TypeName psobject
                $OUT | Add-Member -NotePropertyName "FullName" -NotePropertyValue $A.FullName
                $OUT | Add-Member -NotePropertyName "SID" -NotePropertyValue $Matches[1]
                $ExcludeSIDOutput += $OUT
            }
            elseif($I -match 'g="([^"]*)'){
                $OUT = New-Object -TypeName psobject
                $OUT | Add-Member -NotePropertyName "FullName" -NotePropertyValue $A.FullName
                $OUT | Add-Member -NotePropertyName "Group" -NotePropertyValue $Matches[1]
                $ExcludeGroupOutput += $OUT
            }
            else{
                Write-Host "Something went wrong with $($A.FullName)" -ForegroundColor Yellow
            }
        }
    }

    $Data = @{
        SettingInfo = $SettingInfo
        IncludeSIDOutput = $IncludeSIDOutput
        IncludeGroupOutput = $IncludeGroupOutput
        ExcludeSIDOutput = $ExcludeSIDOutput
        ExcludeGroupOutput = $ExcludeGroupOutput
    }

    return $Data
}


#========================================================================


foreach($confname in $locations.Keys) {
    write-host "Getting UEM Settings for configuration: $confname"
    $resultsobj = Get-UemSettings -Path $locations[$confname]

    write-host "Writing $($resultsobj.SettingInfo.count) settings"
    $paramsobj = new-object 'system.collections.generic.dictionary[[string],[object]]'
    $paramsobj.Add('configuration',$confname)
    $paramsobj.Add('settings',$resultsobj.SettingInfo)

    $query = 'UNWIND $settings as setting ' +
        'MERGE (s:UEM_Setting {id:setting.FullName}) ' +
        'SET s.name = setting.Title '+   
        'SET s.filename = setting.Filename '+  
        'SET s.settingtype = setting.SettingType '+  
        'SET s.configuration = $configuration '+  
        'RETURN count(s) '

    $response = WriteToNeo -NeoConfigPath $neoconf -Query $query -Parameters $paramsobj -serverURL $neoUrl


    write-host "Writing $($resultsobj.IncludeSIDOutput.count) SID include mappings"
    $paramsobj = new-object 'system.collections.generic.dictionary[[string],[object]]'
    $paramsobj.Add('mappings',$resultsobj.IncludeSIDOutput)

    $query = 'UNWIND $mappings as rel ' +
        'MATCH (s:UEM_Setting {id:rel.FullName}) ' +
        'MATCH (g:AD_Group {id:rel.SID}) ' +
        'MERGE p=(g)-[:ACTIVATES]->(s) '+   
        'RETURN count(p) '

    $response = WriteToNeo -NeoConfigPath $neoconf -Query $query -Parameters $paramsobj -serverURL $neoUrl



    write-host "Writing $($resultsobj.ExcludeSIDOutput.count) SID exclude mappings"
    $paramsobj = new-object 'system.collections.generic.dictionary[[string],[object]]'
    $paramsobj.Add('mappings',$resultsobj.ExcludeSIDOutput)

    $query = 'UNWIND $mappings as rel ' +
        'MATCH (s:UEM_Setting {id:rel.FullName}) ' +
        'MATCH (g:AD_Group {id:rel.SID}) ' +
        'MERGE p=(g)-[:DEACTIVATES]->(s) '+   
        'RETURN count(p) '

    $response = WriteToNeo -NeoConfigPath $neoconf -Query $query -Parameters $paramsobj -serverURL $neoUrl




    write-host "Writing $($resultsobj.IncludeGroupOutput.count) group include mappings"
    $paramsobj = new-object 'system.collections.generic.dictionary[[string],[object]]'
    $paramsobj.Add('mappings',$resultsobj.IncludeGroupOutput)

    $query = 'UNWIND $mappings as rel ' +
        'MATCH (s:UEM_Setting {id:rel.FullName}) ' +
        'MATCH (g:AD_Group) WHERE g.samaccountname =~ (''(?i)'' + rel.Group) '+
        'MERGE p=(g)-[:ACTIVATES]->(s) '+   
        'RETURN count(p) '

    $response = WriteToNeo -NeoConfigPath $neoconf -Query $query -Parameters $paramsobj -serverURL $neoUrl



    write-host "Writing $($resultsobj.ExcludeGroupOutput.count) group exclude mappings"
    $paramsobj = new-object 'system.collections.generic.dictionary[[string],[object]]'
    $paramsobj.Add('mappings',$resultsobj.ExcludeGroupOutput)

    $query = 'UNWIND $mappings as rel ' +
        'MATCH (s:UEM_Setting {id:rel.FullName}) ' +
        'MATCH (g:AD_Group) WHERE g.samaccountname =~ (''(?i)'' + rel.Group) '+
        'MERGE p=(g)-[:DEACTIVATES]->(s) '+   
        'RETURN count(p) '

    $response = WriteToNeo -NeoConfigPath $neoconf -Query $query -Parameters $paramsobj -serverURL $neoUrl
}


write-host "Setting scopes"
$query = "MATCH (o) "+
    "WHERE o:AD_User OR o:AD_Computer " + 
    "MATCH (o)-[*]->(s:UEM_Setting) "+
    "WITH collect(DISTINCT o) as nodes, s " +
    "SET s.scope = size(nodes) " +
    "RETURN s";

$response = WriteToNeo -NeoConfigPath $neoconf -Query $query -serverURL $neoUrl
 
