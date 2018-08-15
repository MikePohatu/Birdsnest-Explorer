#$creds = Get-Credential
# -Credential $creds 

$server ='server'
[string]$sitecode = '001'
$neoURL="http://localhost:7474/db/data/transaction/commit"
$neoconfig = "C:\birdsnest\Scanners\neoconfig.json"

Function Get-SccmCollections {
    Param ([Parameter(Mandatory=$true)][string]$Server,
        [Parameter(Mandatory=$true)][string]$SiteCode)
    $block = {
        # Site configuration
        $ProviderMachineName = $args[0] # SMS Provider machine name
        $SiteCode = $args[1] # Site code 
        
        write-host "Connecting to site: $SiteCode on server: $ProviderMachineName"

        # Customizations
        $initParams = @{}
        #$initParams.Add("Verbose", $true) # Uncomment this line to enable verbose logging
        #$initParams.Add("ErrorAction", "Stop") # Uncomment this line to stop the script on any errors

        # Do not change anything below this line

        # Import the ConfigurationManager.psd1 module 
        if((Get-Module ConfigurationManager) -eq $null) {
            Import-Module "$($ENV:SMS_ADMIN_UI_PATH)\..\ConfigurationManager.psd1" @initParams 
        }

        # Connect to the site's drive if it is not already present
        if((Get-PSDrive -Name $SiteCode -PSProvider CMSite -ErrorAction SilentlyContinue) -eq $null) {
            New-PSDrive -Name $SiteCode -PSProvider CMSite -Root $ProviderMachineName @initParams
        }

        # Set the current location to be the site code.
        Set-Location "$($SiteCode):\" @initParams


        $cols = New-Object 'System.Collections.Generic.List[object]'
        $limits = New-Object 'System.Collections.Generic.List[object]'

        write-host "Getting SCCM collections"
        Get-CMCollection | select -Property Name, CollectionID, LimitToCollectionID, Comment | foreach {
            #write-host $_.Name
            $col = new-object 'system.collections.generic.dictionary[[string],[object]]'
            $encodename = [System.Text.Encoding]::UTF8.GetString([System.Text.Encoding]::GetEncoding(28591).GetBytes($_.Name))
            $col.Add('Name',$encodename)
            $col.Add('ID',$_.CollectionID)
            $col.Add('LimitingID',$_.LimitToCollectionID)
            $col.Add('Comment',"$($_.Comment)")
            
            $cols.Add($col)

            if ($_.LimitToCollectionID) {
                $limit = new-object 'system.collections.generic.dictionary[[string],[object]]'
                $limit.Add('CollectionID',$_.CollectionID)
                $limit.Add('LimitingID',$_.LimitToCollectionID)
                $limits.Add($limit)
            }
        }

        $result = new-object 'system.collections.generic.dictionary[[string],[object]]'
        $result.Add('Collections',$cols)
        $result.Add('Limits',$limits)

        return $result
    }

    write-host "Invoking command on $Server"
    $resultobj = Invoke-Command -ComputerName $Server -ScriptBlock $block -ArgumentList $Server,$SiteCode 
    return $resultobj
}


Function Get-SccmApplications {
    Param ([Parameter(Mandatory=$true)][string]$Server,
        [Parameter(Mandatory=$true)][string]$SiteCode)
    $block = {
        # Site configuration
        $ProviderMachineName = $args[0] # SMS Provider machine name
        $SiteCode = $args[1] # Site code 
        
        write-host "Connecting to site: $SiteCode on server: $ProviderMachineName"

        # Customizations
        $initParams = @{}
        #$initParams.Add("Verbose", $true) # Uncomment this line to enable verbose logging
        #$initParams.Add("ErrorAction", "Stop") # Uncomment this line to stop the script on any errors

        # Do not change anything below this line

        # Import the ConfigurationManager.psd1 module 
        if((Get-Module ConfigurationManager) -eq $null) {
            Import-Module "$($ENV:SMS_ADMIN_UI_PATH)\..\ConfigurationManager.psd1" @initParams 
        }

        # Connect to the site's drive if it is not already present
        if((Get-PSDrive -Name $SiteCode -PSProvider CMSite -ErrorAction SilentlyContinue) -eq $null) {
            New-PSDrive -Name $SiteCode -PSProvider CMSite -Root $ProviderMachineName @initParams
        }

        # Set the current location to be the site code.
        Set-Location "$($SiteCode):\" @initParams


        $apps = New-Object 'System.Collections.Generic.List[object]'

        write-host "Getting SCCM applications"
        Get-CMApplication -fast | select -Property CI_UniqueID, CI_ID, ModelID, LocalizedDisplayName, IsEnabled, IsSuperseded, IsSuperseding, PackageID, LastModifiedBy, DateLastModified, DateCreated | foreach {
            #write-host $_.Name
            $app = new-object 'system.collections.generic.dictionary[[string],[object]]'
            $encodename = [System.Text.Encoding]::UTF8.GetString([System.Text.Encoding]::GetEncoding(28591).GetBytes($_.LocalizedDisplayName))
            $app.Add('Name',$encodename)
            $app.Add('CI_ID',$_.CI_ID)
            $app.Add('CI_UniqueID',$_.CI_UniqueID)
            $app.Add('ModelID',$_.ModelID)
            $app.Add('PackageID',$_.PackageID)
            $app.Add('DateLastModified',$_.DateLastModified)
            $app.Add('DateCreated',$_.DateCreated)
            $app.Add('IsEnabled',$_.IsEnabled)
            $app.Add('IsSuperseded',$_.IsSuperseded)
            $app.Add('IsSuperseding',$_.IsSuperseding)

            $apps.Add($app)
        }

        return $apps
    }

    write-host "Invoking command on $Server"
    $resultobj = Invoke-Command -ComputerName $Server -ScriptBlock $block -ArgumentList $Server,$SiteCode 
    return $resultobj
}

Function Get-SccmApplicationDeployments {
    Param ([Parameter(Mandatory=$true)][string]$Server,
        [Parameter(Mandatory=$true)][string]$SiteCode)
    $block = {
        # Site configuration
        $ProviderMachineName = $args[0] # SMS Provider machine name
        $SiteCode = $args[1] # Site code 
        
        write-host "Connecting to site: $SiteCode on server: $ProviderMachineName"

        # Customizations
        $initParams = @{}
        #$initParams.Add("Verbose", $true) # Uncomment this line to enable verbose logging
        #$initParams.Add("ErrorAction", "Stop") # Uncomment this line to stop the script on any errors

        # Do not change anything below this line

        # Import the ConfigurationManager.psd1 module 
        if((Get-Module ConfigurationManager) -eq $null) {
            Import-Module "$($ENV:SMS_ADMIN_UI_PATH)\..\ConfigurationManager.psd1" @initParams 
        }

        # Connect to the site's drive if it is not already present
        if((Get-PSDrive -Name $SiteCode -PSProvider CMSite -ErrorAction SilentlyContinue) -eq $null) {
            New-PSDrive -Name $SiteCode -PSProvider CMSite -Root $ProviderMachineName @initParams
        }

        # Set the current location to be the site code.
        Set-Location "$($SiteCode):\" @initParams


        $deps = New-Object 'System.Collections.Generic.List[object]'

        write-host "Getting SCCM application deployments"
        Get-CMApplicationDeployment | select -Property TargetCollectionID,AppModelID | foreach {
            #write-host $_.Name
            $dep = new-object 'system.collections.generic.dictionary[[string],[object]]'          
            $dep.Add('TargetCollectionID',$_.TargetCollectionID)
            $dep.Add('AppModelID',$_.AppModelID)

            $deps.Add($dep)
        }

        return $deps
    }

    write-host "Invoking command on $Server"
    $resultobj = Invoke-Command -ComputerName $Server -ScriptBlock $block -ArgumentList $Server,$SiteCode 
    return $resultobj
}

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
        #           '"statement" : "' + $Query + '",' +
        #           '"parameters" : ' + $paramsjson +
        #           '}]' +
        #        '}'    
        $body = $statement = new-object 'system.collections.generic.dictionary[[string],[object]]'
        $body.Add('statements',$statements)

        $bodyjson = $body |ConvertTo-Json -Depth 5
        #write-host $bodyjson
        
        # Call Neo4J HTTP EndPoint, Pass in creds & POST JSON Payload
        $response = Invoke-WebRequest -Uri $serverURL -Method POST -Body $bodyjson -credential $neo4jCreds -ContentType "application/json"
    } 
    finally {
      
    }
    return $response
}

write-host "Getting Sccm Collections"
$resultsobj = Get-SccmCollections -Server $server -SiteCode $sitecode

write-host "Writing $($resultsobj.Collections.count) collections"
$paramsobj = new-object 'system.collections.generic.dictionary[[string],[object]]'
$paramsobj.Add('collections',$resultsobj.Collections)
$query = 'UNWIND $collections as col ' +
        'MERGE (c:CM_Collection {id:col.ID}) ' +
        'SET c.limitingcollection = col.LimitingID '+   
        'SET c.name = col.Name '+
        'SET c.comment = col.Comment '+
        'RETURN count(c) '

$response = WriteToNeo -NeoConfigPath $neoconfig -Query $query -Parameters $paramsobj -serverURL $neoUrl

write-host "Writing $($resultsobj.Limits.count) limiting collection mappings"
$paramsobj = new-object 'system.collections.generic.dictionary[[string],[object]]'
$paramsobj.Add('limits',$resultsobj.Limits)
$query = 'UNWIND $limits as limit ' +
        'MERGE (c:CM_Collection {id:limit.CollectionID}) ' +
        'MERGE (l:CM_Collection {id:limit.LimitingID}) '+
        'MERGE p=((l)-[:LIMITING_FOR]->(c)) ' +   
        'RETURN count(p) '

$response = WriteToNeo -NeoConfigPath $neoconfig -Query $query -Parameters $paramsobj -serverURL $neoUrl

#Finished with collections

write-host "Getting Sccm Applications"
$resultsobj = Get-SccmApplications -Server $server -SiteCode $sitecode

write-host "Writing to database"
$paramsobj = new-object 'system.collections.generic.dictionary[[string],[object]]'
$paramsobj.Add('apps',$resultsobj)
$query = 'UNWIND $apps as app ' +
        'MERGE (a:CM_Application {id:app.ModelID}) ' +
        'SET a.name = app.Name ' + 
        'SET a.CI_ID = app.CI_ID ' + 
        'SET a.CI_UniqueID = app.CI_UniqueID ' + 
        'SET a.ModelID = app.ModelID ' + 
        'SET a.PackageID = app.PackageID ' + 
        'SET a.DateLastModified = app.DateLastModified ' + 
        'SET a.DateCreated = app.DateCreated ' + 
        'SET a.IsEnabled = app.IsEnabled ' + 
        'SET a.IsSuperseded = app.IsSuperseded ' + 
        'SET a.IsSuperseding  = app.IsSuperseding ' +  
        'RETURN count(a) '

$response = WriteToNeo -NeoConfigPath $neoconfig -Query $query -Parameters $paramsobj -serverURL $neoUrl



write-host "Getting Sccm Application Deployments"
$resultsobj = Get-SccmApplicationDeployments -Server $server -SiteCode $sitecode

write-host "Writing to database"
$paramsobj = new-object 'system.collections.generic.dictionary[[string],[object]]'
$paramsobj.Add('deps',$resultsobj)
$query = 'UNWIND $deps as dep ' +
        'MATCH (a:CM_Application {id:dep.AppModelID}) ' +
        'MATCH (c:CM_Collection {id:dep.TargetCollectionID}) ' +
        'MERGE p=(a)-[:DEPLOYED_TO]->(c) ' +
        'RETURN count(p) '

$response = WriteToNeo -NeoConfigPath $neoconfig -Query $query -Parameters $paramsobj -serverURL $neoUrl
