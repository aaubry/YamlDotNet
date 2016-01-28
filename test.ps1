
nuget install xunit.runner.console -Version 2.1.0 -OutputDirectory packages

@("Debug", "Release", "Release-Portable") | % {
  Write-Host ""
  Write-Host "Testing $_ build"
  Write-Host "----------------------------------------------"

  dir "YamlDotNet.Test\bin\$_\YamlDotNet.Test*dll" | % {
    Write-Host $_.FullName
    .\packages\xunit.runner.console.2.1.0\tools\xunit.console.exe $_.FullName
  }
}

.\YamlDotNet.AotTest\test.ps1
