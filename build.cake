#tool "nuget:?package=xunit.runner.console"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var releaseConfigurations = new[] { "Release-Unsigned", "Release-Signed", "Release-Portable-Unsigned", "Release-Portable-Signed" };
var configuration = Argument("configuration", "Release-Unsigned");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./YamlDotNet/bin") + Directory(configuration);

var solutionPath = "./YamlDotNet.sln";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Console.WriteLine(configuration);

Task("Clean")
    .Does(() =>
    {
        CleanDirectories(new[]
        {
            "./YamlDotNet/bin",
            "./YamlDotNet.AotTest/bin",
            "./YamlDotNet.Samples/bin",
            "./YamlDotNet.Test/bin",
        });
    });

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        NuGetRestore(solutionPath);
    });

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
    {
        if(IsRunningOnWindows())
        {
            // Use MSBuild
            MSBuild(solutionPath, settings => settings
                .SetConfiguration(configuration));
        }
        else
        {
            // Use XBuild
            XBuild(solutionPath, settings => settings
                .SetConfiguration(configuration));
        }
    });

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
    {
        XUnit2("YamlDotNet.Test/bin/" + configuration + "/YamlDotNet.Test*.dll");
    });

Task("Package")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
    {
        foreach(var releaseConfiguration in releaseConfigurations)
        {
            if(IsRunningOnWindows())
            {
                // Use MSBuild
                MSBuild(solutionPath, settings => settings
                    .SetConfiguration(releaseConfiguration));
            }
            else
            {
                // Use XBuild
                XBuild(solutionPath, settings => settings
                    .SetConfiguration(releaseConfiguration)
                    .UseToolVersion(XBuildToolVersion.NET40));
            }

            XUnit2("YamlDotNet.Test/bin/" + releaseConfiguration + "/YamlDotNet.Test*.dll");
        }
    });

Task("Document")
    // .IsDependentOn("Build")
    .Does(() =>
    {
        XUnit2("YamlDotNet.Samples/bin/" + configuration + "/YamlDotNet.Samples.dll", new XUnit2Settings
        {
            OutputDirectory = Directory("YamlDotNet.Samples/bin/" + configuration),
            XmlReport = true
        });
        
        // Console.WriteLine(testsDir + File("YamlDotNet.Test.dll"));
    });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
     // .IsDependentOn("Document");
     .IsDependentOn("Run-Unit-Tests");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
