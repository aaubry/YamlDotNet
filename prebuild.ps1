. .\BuildUtils\build_utils.ps1

$buildNumber = ($env:APPVEYOR_BUILD_NUMBER, "0" -ne $null)[0]
$version = Get-VersionFromTag

Update-AppveyorBuild -Version "$version.$buildNumber"

Patch-Xml "YamlDotNet\YamlDotNet.nuspec" $version "/package/metadata/version/text()" @{ }
Patch-AssemblyInfo "YamlDotNet\Properties\AssemblyInfo.cs" $version
