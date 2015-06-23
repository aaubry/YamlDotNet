. .\BuildUtils\build_utils.ps1

function Download-File($url, $path, $description) {
    $wc = New-Object System.Net.WebClient

    $previousPercentage = $null
    $e = Register-ObjectEvent -InputObject $wc -EventName DownloadProgressChanged -Action {
      $currentPercentage = $Args[1].ProgressPercentage
      if ($previousPercentage -ne $currentPercentage) {
        Write-Host "Downloading $description... $currentPercentage %"
        $previousPercentage = $currentPercentage
      }
    }
    $e = Register-ObjectEvent -InputObject $wc -EventName DownloadFileCompleted -Action { New-Event "DownloadComplete" }

    $wc.DownloadFileAsync($url, $path)

    Write-Host "Download complete"
    $e = Wait-Event "DownloadComplete"
    Remove-Event "DownloadComplete"
}

Download-File "https://visualstudiogallery.msdn.microsoft.com/20b80b8c-659b-45ef-96c1-437828fe7cf2/file/92287/8/Visual%20Studio%202013%20Tools%20for%20Unity.msi" "$env:TEMP\Unity.msi" "VS Tools for Unity "
msiexec.exe /qn /lv "$env:TEMP\Unity.log" /i "$env:TEMP\Unity.msi" | Out-Null

"Release-Unsigned", "Release-Signed", "Release-Portable-Unsigned", "Release-Portable-Signed", "Release-UnitySubset-v35" |
  % { $i = 0 } {

    if($i++ -gt 0) {
        Write-Host ""
        Write-Host "--------------------------------------------------------------------------------------"
        Write-Host ""
    }

    if(Test-Path "C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll") {
        msbuild YamlDotNet.sln /logger:"C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll" /p:Configuration=$_
    } else {
        msbuild YamlDotNet.sln /p:Configuration=$_
    }

	if($LASTEXITCODE -ne 0) {
		throw "Build failed"
	}
}

"Unsigned", "Signed" | % {
    nuget pack YamlDotNet\YamlDotNet.$_.nuspec -OutputDirectory YamlDotNet\bin

	if($LASTEXITCODE -ne 0) {
		throw "Build failed"
	}
}
