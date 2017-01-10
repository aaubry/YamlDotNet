#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./src/Example/bin") + Directory(configuration);
var testsDir = Directory("./YamlDotNet.Test/bin") + Directory(configuration);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

// Task("Clean")
//     .Does(() =>
// {
//     CleanDirectory(buildDir);
// });

// Task("Restore-NuGet-Packages")
//     .IsDependentOn("Clean")
//     .Does(() =>
// {
//     NuGetRestore("./src/Example.sln");
// });

// Task("Build")
//     .IsDependentOn("Restore-NuGet-Packages")
//     .Does(() =>
// {
//     if(IsRunningOnWindows())
//     {
//       // Use MSBuild
//       MSBuild("./src/Example.sln", settings =>
//         settings.SetConfiguration(configuration));
//     }
//     else
//     {
//       // Use XBuild
//       XBuild("./src/Example.sln", settings =>
//         settings.SetConfiguration(configuration));
//     }
// });

// Task("Run-Unit-Tests")
//     .IsDependentOn("Build")
//     .Does(() =>
// {
//     NUnit3("./src/**/bin/" + configuration + "/*.Tests.dll", new NUnit3Settings {
//         NoResults = true
//         });
// });

Task("Document")
    .Does(() =>
    {
        // Console.WriteLine(testsDir + File("YamlDotNet.Test.dll"));
    });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
     .IsDependentOn("Document");
//     .IsDependentOn("Run-Unit-Tests");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
