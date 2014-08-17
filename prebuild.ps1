. .\BuildUtils\build_utils.ps1

$buildNumber = ($env:APPVEYOR_BUILD_NUMBER, "0" -ne $null)[0]
$version = Get-VersionFromTag

Patch-Xml "YamlDotNet\YamlDotNet.nuspec" $version $buildNumber "/package/metadata/version/text()" @{ }
Patch-AssemblyInfo "YamlDotNet\Properties\AssemblyInfo.cs" $version $buildNumber
