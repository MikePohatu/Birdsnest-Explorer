Param (
    [Parameter(Mandatory=$true)][string]$ScanID
)

$scriptFileName = $MyInvocation.MyCommand.Name


<#
GET /deviceManagement/deviceConfigurations/{deviceConfigurationId}/groupAssignments
GET /deviceManagement/deviceConfigurations/{deviceConfigurationId}/rootCertificate/groupAssignments
GET /deviceManagement/deviceConfigurations/{deviceConfigurationId}/identityCertificate/groupAssignments
GET /deviceManagement/deviceConfigurations/{deviceConfigurationId}/identityCertificate/rootCertificate/groupAssignments
GET /deviceManagement/deviceConfigurations/{deviceConfigurationId}/microsoft.graph.iosScepCertificateProfile/rootCertificate/groupAssignments
GET /deviceManagement/deviceConfigurations/{deviceConfigurationId}/microsoft.graph.windowsPhone81VpnConfiguration/identityCertificate/groupAssignments
GET /deviceManagement/deviceConfigurations/{deviceConfigurationId}/microsoft.graph.macOSWiredNetworkConfiguration/rootCertificateForServerValidation/groupAssignments
GET /deviceManagement/deviceConfigurations/{deviceConfigurationId}/microsoft.graph.macOSWiredNetworkConfiguration/identityCertificateForClientAuthentication/groupAssignments
GET /deviceManagement/deviceConfigurations/{deviceConfigurationId}/microsoft.graph.windowsWifiEnterpriseEAPConfiguration/identityCertificateForClientAuthentication/groupAssignments
GET /deviceManagement/deviceConfigurations/{deviceConfigurationId}/microsoft.graph.windowsWifiEnterpriseEAPConfiguration/rootCertificatesForServerValidation/{windows81TrustedRootCertificateId}/groupAssignments
#>

#$VerbosePreference = "Continue"

#get assignments

#define graph API management paths
$replacePart = "{{replace}}"
$graphPaths = @{
    "Configs" = @{
        BasePath = "https://graph.microsoft.com/beta/deviceManagement/deviceConfigurations"
        AssignmentsPath = "https://graph.microsoft.com/beta/deviceManagement/deviceConfigurations/$($replacePart)/groupAssignments"
        nameField="displayName"
    }
    "AdminTemplateConfigs" = @{
        BasePath = "https://graph.microsoft.com/beta/deviceManagement/groupPolicyConfigurations"
        AssignmentsPath = "https://graph.microsoft.com/beta/deviceManagement/groupPolicyConfigurations/$($replacePart)/assignments"
        nameField="displayName"
    }
    "ManagedAppPolicies" = @{
        BasePath = "https://graph.microsoft.com/beta/deviceAppmanagement/managedAppPolicies"
        AssignmentsPath = $null
        nameField="displayName"
    }
    "Compliance" = @{
        BasePath = "https://graph.microsoft.com/beta/deviceManagement/deviceCompliancePolicies"
        AssignmentsPath = "https://graph.microsoft.com/beta/deviceManagement/deviceCompliancePolicies/$($replacePart)/assignments"
        nameField="displayName"
    }
    "SettingsCatalog" = @{
        BasePath = "https://graph.microsoft.com/beta/deviceManagement/configurationPolicies"
        AssignmentsPath = "https://graph.microsoft.com/beta/deviceManagement/configurationPolicies/$($replacePart)/assignments"
        nameField="name"
    }    
}


$includes = @()
$excludes = @()
$allConfigs = @()

#foreach ($key in $graphPaths.Keys) 
$activity = "Processing paths"

#for ($i = 0; $i -lt $graphPaths.Keys.Count; $i++) {
    #$key = $graphPaths.Keys.g[$i]
$count = 0  
foreach ($key in $graphPaths.Keys) {
    $count++
    Write-Progress -Activity $activity -PercentComplete (($count/$graphPaths.Keys.Count)*100) -CurrentOperation $key
    #Write-Host "Processing $key" -ForegroundColor Cyan
    $path = $graphPaths[$key]
    $items = @(Get-GraphRequest -All -URI $path.BasePath)

    #foreach ($item in $items) 
    for ($j = 0; $j -lt $items.Count; $j++) {
        $item = $items[$j]
        $itemName=$item[$graphPaths[$key].nameField]
        $allConfigs += @{
            name = $itemName
            description = $item.description
            id = $item.id
            type = $key.ToLower()
        }
        Write-Progress -Activity $key -PercentComplete (($j/$items.Count)*100) -CurrentOperation $itemName

        if ($path.AssignmentsPath) {
            $itemAssignments = @(Get-GraphRequest -All -URI $path.AssignmentsPath.Replace($replacePart, $item.Id))
            for ($k = 0; $k -lt $itemAssignments.Length; $k++) {
                $assignment = $itemAssignments[$k]
                
                $exclude = $false

                #Get the group ID, from the id: xxxxx_groupid
                $groupid = $assignment["id"].Split('_')[1]

                #deviceCompliancePolicies and deviceConfigurations have different schemas
                if ($assignment.target) {
                    if ($assignment.target["@odata.type"] -eq "#microsoft.graph.exclusionGroupAssignmentTarget") {
                        $exclude = $true
                        Write-Verbose "EXCLUDE: #microsoft.graph.exclusionGroupAssignmentTarget matched"
                    }
                }
                else {
                    if ($assignment.ExcludeGroup) {
                        $exclude = $true
                        Write-Verbose 'EXCLUDE: $assignment.ExcludeGroup matched'
                    }
                }

                if ($groupid) {
                    Write-Progress -Activity "Assignments for $itemName" -PercentComplete (($k/$itemAssignments.Length)*100) -CurrentOperation "ID: $($assignment.id) | Include: $groupid"
                        
                    if ($exclude) {
                        #Write-Host "ID: $($assignment.id) | Exclude: $groupid" -ForegroundColor DarkYellow
                        $excludes += @{
                            GroupID = $groupid
                            ConfID = $item.id
                            AssignmentID = $assignment.id
                        }
                        #Write-Verbose ($assignment | ConvertTo-Json)
                    }
                    else {
                        #Write-Host "ID: $($assignment.id) | Include: $groupid" -ForegroundColor Green
                        $includes += @{
                            GroupID = $groupid
                            ConfID = $item.id
                            AssignmentID = $assignment.id
                        }
                        #Write-Verbose ($assignment | ConvertTo-Json)
                    }
                }
            }
            Write-Progress -Activity "Assignments for $itemName" -Completed
        }
    }
    Write-Progress -Activity "$key" -Completed
}
Write-Progress -Activity $activity -Completed

#region neo4j queries

$op = @{
        message = "Writing $($allConfigs.Length) configs"
        params = @{
            props = $allConfigs
            ScanID = $scanID
            ScannerID = $scriptFileName
        }
        query = @'
            UNWIND $props AS prop 
            MERGE (n:AZ_Intune_Configuration {id:prop.id}) 
            SET n.name = prop.name 
            SET n.description = prop.description 
            SET n.type = prop.type 
            SET n.lastscan=$ScanID 
            SET n.scannerid=$ScannerID 
            RETURN count(n)
'@
}
Write-NeoOperations @op

$op = @{
        message = "Writing $($includes.Length) includes"
        params = @{
            props = $includes
            ScanID = $scanID
            ScannerID = $scriptFileName
        }
        query = @'
            UNWIND $props AS prop 
            MATCH (n:AZ_Intune_Configuration {id:prop.ConfID}) 
            MATCH (g:AZ_Group {id:prop.GroupID}) 
            MERGE p=(g)-[r:AZ_ASSIGNMENT_INCLUDE]->(n) 
            SET r.lastscan=$ScanID 
            SET r.scannerid=$ScannerID 
            SET r.layout='mesh' 
            SET r.id=prop.AssignmentID
            RETURN p
'@
}
Write-NeoOperations @op

$op = @{
        message = "Writing $($includes.Length) excludes"
        params = @{
            props = $excludes
            ScanID = $scanID
            ScannerID = $scriptFileName
        }
        query = @'
            UNWIND $props AS prop 
            MATCH (n:AZ_Intune_Configuration {id:prop.ConfID}) 
            MATCH (g:AZ_Group {id:prop.GroupID}) 
            MERGE p=(g)-[r:AZ_ASSIGNMENT_EXCLUDE]->(n) 
            SET r.lastscan=$ScanID 
            SET r.scannerid=$ScannerID 
            SET r.layout='mesh' 
            SET r.id=prop.AssignmentID
            RETURN p
'@
}
Write-NeoOperations @op

$op = @{
        message = "Cleaning up configs"
        params = @{
            ScanID = $scanID
            ScannerID = $scriptFileName
        }
        query = @'
            MATCH (n:AZ_Intune_Configuration { scannerid:$ScannerID}) 
            WHERE n.lastscan <> $ScanID 
            DETACH DELETE n 
            RETURN count(n)
'@
}
Write-NeoOperations @op

$op = @{
        message = "Cleaning up assignment excludes"
        params = @{
            ScanID = $scanID
            ScannerID = $scriptFileName
        }
        query = @'
            MATCH ()-[r:AZ_ASSIGNMENT_EXCLUDE {scannerid:$ScannerID}]->() 
            WHERE r.lastscan <> $ScanID 
            DELETE r 
            RETURN count(r)
'@
}
Write-NeoOperations @op

$op = @{
        message = "Cleaning up assignment includes"
        params = @{
            ScanID = $scanID
            ScannerID = $scriptFileName
        }
        query = @'
            MATCH ()-[r:ASSIGNMENT_INCLUDE {scannerid:$ScannerID}]->() 
            WHERE r.lastscan <> $ScanID 
            DELETE r 
            RETURN count(r)
'@
}
Write-NeoOperations @op
#endregion