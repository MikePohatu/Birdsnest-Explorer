$FolderCount = 0
$FolderPath = Get-Location
$FolderSubPath = $FolderPath
$FileTotalCount = 0

While ($FolderCount -lt 10000) {
    $FolderSubPath = "$($FolderPath)\$($FolderCount)"
    New-Item -Path "$FolderSubPath" -ItemType directory
    $FolderCount++

    for ($FolderDepth=0; $FolderDepth -lt 10; $FolderDepth++) {
        #Create 100 files
        for ($FileCount=0; $FileCount -lt 100; $FileCount++) {
            #write-host "$($FolderSubPath)\$($FileTotalCount).txt"
            $FileTotalCount++
            New-Item -Path "$($FolderSubPath)\$($FileTotalCount).txt" -ItemType file
        }

        $FolderSubPath = "$($FolderSubPath)\$($FolderCount)"
        #write-host $FolderSubPath
        New-Item -Path "$FolderSubPath" -ItemType directory
        $FolderCount++
    }
}