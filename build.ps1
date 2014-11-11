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
}

$version = $env:APPVEYOR_BUILD_VERSION
if($version -eq $null) {
    $version = "0.0.1"
}

"Unsigned", "Signed" | % {
    nuget pack YamlDotNet\YamlDotNet.$_.nuspec -Version $version -OutputDirectory YamlDotNet\bin
}