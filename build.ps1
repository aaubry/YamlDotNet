. .\BuildUtils\build_utils.ps1

"Release-Unsigned", "Release-Signed", "Release-Portable-Unsigned", "Release-Portable-Signed" | % { $i = 0 } {

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