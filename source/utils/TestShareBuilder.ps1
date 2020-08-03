function Set-Perm {
    Param (
        [string][Parameter(Mandatory=$true)]$Path,
        [string][Parameter(Mandatory=$true)]$GroupName
    )
    
   
    write-host "Setting perm for $groupname to $Path" -ForegroundColor Red
    $acl = Get-Acl $Path
    $ar = New-Object System.Security.AccessControl.FileSystemAccessRule($GroupName, "ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow")
    $acl.SetAccessRule($ar)
    Set-Acl $Path $acl
}

[string]$Root = "C:\Share"
[int]$CurrentDepth = 0
[int]$MaxDepth = 10
[int]$CurrentFolderCount = 0
[int]$MaxFolderCount = 500000
[string]$GroupNamePrefix = 'Demo\LG-DemoGroup'
[int]$GroupNameMin = 1
[int]$GroupNameMax = 2500
[int]$PermOneEvery = 1000



if (!(Test-Path $Root)) {
    New-Item -Path $Root -ItemType directory | Out-Null
}

#init values
$list = New-Object Collections.Generic.List[String]
$list.Add($Root)
$layer = 0
$permcount = 0

while ($CurrentFolderCount -lt $MaxFolderCount) {
    if ($layer -gt $MaxDepth) { break }
    $subList = New-Object Collections.Generic.List[String]
    $list | ForEach-Object {
        if ($CurrentFolderCount -ge $MaxFolderCount) { break }
        Write-Progress -Activity $_ -PercentComplete (($CurrentFolderCount/$MaxFolderCount) * 100)

        for ($i=0; $i -lt 10; $i++) {
            if ($CurrentFolderCount -ge $MaxFolderCount) { break }
            $subPath = "$($_)\Layer$($layer)-$($CurrentFolderCount)"
            New-Item -Path "$subPath" -ItemType directory | Out-Null
            $CurrentFolderCount++

            $rand = Get-Random -Minimum 1 -Maximum 100
            if ($rand -lt (100 - (10 * $layer))) {
                $subList.Add($subPath)
            }

            #perms
            if ($layer -le 1) { $rand = 1 }
            else { $rand = Get-Random -Minimum 1 -Maximum 1000 }
            
            if ($rand -eq 1) {
                $rand = Get-Random -Minimum $GroupNameMin -Maximum $GroupNameMax
                $groupname = "$GroupNamePrefix$rand"

            
                SetPerm -Path $subpath -GroupName $groupname
                $permcount++
            }
        }

    }
    $list = $subList
    $layer++
}


Write-Host "Permissions created: $permcount"