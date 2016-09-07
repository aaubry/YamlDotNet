. .\BuildUtils\build_utils.ps1

$buildNumber = ($env:APPVEYOR_BUILD_NUMBER, "0" -ne $null)[0]

if($env:APPVEYOR_REPO_BRANCH -ne "release") {
	$version = Get-VersionFromTag $buildNumber
} else {
	$version = Get-VersionFromTag
}

Update-AppveyorBuild -Version $version

Patch-Xml "YamlDotNet\YamlDotNet.Unsigned.nuspec" $version "/package/metadata/version/text()" @{ }
Patch-Xml "YamlDotNet\YamlDotNet.Signed.nuspec" $version "/package/metadata/version/text()" @{ }
Patch-AssemblyInfo "YamlDotNet\Properties\AssemblyInfo.cs" $version

cd BuildUtils.UnityPrerequisites
.\install.ps1
cd ..
