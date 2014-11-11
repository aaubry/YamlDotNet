. .\BuildUtils\build_utils.ps1

$logger = ""
if(Test-Path "C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll") {
    $logger = '/logger:"C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll"'
}

"Release-Unsigned", "Release-Signed", "Release-Portable-Unsigned", "Release-Portable-Signed" | % { $i = 0 } {

    if($i++ -gt 0) {
        Write-Host ""
        Write-Host "--------------------------------------------------------------------------------------"
        Write-Host ""
    }

    msbuild YamlDotNet.sln $logger /p:Configuration=$_
}

$version = $env:APPVEYOR_BUILD_VERSION
if($version -eq $null) {
    $version = "0.0.1"
}

"Unsigned", "Signed" | % {
    nuget pack YamlDotNet\YamlDotNet.$_.nuspec -Version $version -OutputDirectory YamlDotNet\bin
}