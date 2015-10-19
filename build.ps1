. .\BuildUtils\build_utils.ps1

$build_unity = $true

function Download-File($url, $path, $description) {
  $wc = New-Object System.Net.WebClient

  Write-Host "Downloading $description"
  Write-Host ": |---------|---------|---------|---------|---------|---------|---------|---------|---------|---------|"
  Write-Host ": " -NoNewline

  $previousPercentage = 0
  $e = Register-ObjectEvent -InputObject $wc -EventName DownloadProgressChanged -Action {
    $currentPercentage = $Args[1].ProgressPercentage
    for(; $previousPercentage -le $currentPercentage; $previousPercentage++) {
      Write-Host "#" -NoNewline
    }
  }
  $e = Register-ObjectEvent -InputObject $wc -EventName DownloadFileCompleted -Action {
    Write-Host " Done"
    New-Event "DownloadComplete"
  }

  $canceling = $true
  $wc.DownloadFileAsync($url, $path, $description)
  try {
    $e = Wait-Event "DownloadComplete"
    $canceling = $false
  }
  finally {
    if($canceling) {
      Write-Host " - cancelling - " -NoNewline
    }
    $wc.CancelAsync()
    $e = Wait-Event "DownloadComplete"
    Remove-Event "DownloadComplete"
    Write-Host ""
  }
}

$targets = "Release-Unsigned", "Release-Signed", "Release-Portable-Unsigned", "Release-Portable-Signed"

if($build_unity) {
  $targets += "Release-UnitySubset-v35"

  Download-File "https://visualstudiogallery.msdn.microsoft.com/20b80b8c-659b-45ef-96c1-437828fe7cf2/file/92287/8/Visual%20Studio%202013%20Tools%20for%20Unity.msi" "$env:TEMP\Unity.msi" "VS Tools for Unity"

  Write-Host "Installing VS Tools for Unity... " -NoNewline
  & .\BuildUtils\MsiInstaller\bin\Release\MsiInstaller.exe "$env:TEMP\Unity.msi"
  Write-Host "Done"
}

$targets |
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
