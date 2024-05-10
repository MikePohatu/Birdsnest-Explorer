#$VerbosePreference = "Continue"

Import-Module "$($PSScriptRoot)\AzureFunctions.psm1" -Force
Import-Module "$($PSScriptRoot)\WriteToNeo.psm1" -Force
Set-NeoConnection -neoURL "http://localhost:7474/db/data/transaction/commit" -neoconf "C:\birdsnest\Scanners\config\neoconfig.json"

$scanID = Get-ShortGuid 

Import-module Microsoft.Graph.DeviceManagement, Microsoft.Graph.Groups, Microsoft.Graph.Users -ErrorAction Stop -Force
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
    "AuditLog.Read.All" -ErrorAction Stop


& "$PSScriptRoot\Aad.ps1" -ScanID $scanID
& "$PSScriptRoot\ConditionalAccess.ps1" -ScanID $scanID
& "$PSScriptRoot\IntuneApps.ps1" -ScanID $scanID
& "$PSScriptRoot\IntuneConfigurations.ps1" -ScanID $scanID

