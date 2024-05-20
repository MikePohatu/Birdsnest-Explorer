#$VerbosePreference = "Continue"

Import-Module "$($PSScriptRoot)\AzureFunctions.psm1" -Force
Import-Module "$($PSScriptRoot)\WriteToNeo.psm1" -Force
Set-NeoConnection -neoURL "http://localhost:7474/db/data/transaction/commit" -neoconf "C:\birdsnest\Scanners\config\neoconfig.json"

$scanID = Get-ShortGuid 

Write-Host "Starting scan ID: $scanID"
Import-module Microsoft.Graph.DeviceManagement, Microsoft.Graph.Groups, Microsoft.Graph.Users -ErrorAction Stop
Connect-MgGraph -Scopes "DeviceManagementConfiguration.Read.All", 
    "DeviceManagementApps.Read.All", 
    "Application.Read.All", 
    "Policy.Read.All", 
    "RoleManagement.Read.Directory", 
    "Group.Read.All", 
    "User.Read.All", 
    "Device.Read.All", 
    "GroupMember.Read.All",
    "Organization.Read.All",
    "DeviceManagementManagedDevices.Read.All",
    "AuditLog.Read.All" -ErrorAction Stop


& "$PSScriptRoot\Aad.ps1" -ScanID $scanID
& "$PSScriptRoot\ConditionalAccess.ps1" -ScanID $scanID
& "$PSScriptRoot\Intune.ps1" -ScanID $scanID


$op = @{
    message = 'Setting AZ_Object scopes'
    params = @{ }
    query = @"
MATCH (o:AZ_Object) 
WHERE o:AZ_User OR o:AZ_Device AND o.lastscan = '$scanID' 
SET o.scope = 1 
WITH o 
MATCH (o)-[*1..7]->(n:AZ_Object) 
WITH collect(DISTINCT o) as nodes, n 
SET n.scope = size(nodes) 
RETURN n
"@
}

Write-NeoOperations @op


Write-Host "Finished scan ID: $scanID"