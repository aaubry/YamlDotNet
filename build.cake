#tool "nuget:?package=xunit.runner.console&version=2.4.1"
#tool "nuget:?package=Mono.TextTransform&version=1.0.0"
#tool "nuget:?package=GitVersion.CommandLine&version=4.0.0"
#addin "nuget:?package=Cake.Incubator&version=4.0.1"
#addin "nuget:?package=Cake.SemVer&version=3.0.0"
#addin "nuget:?package=semver&version=2.0.4"
#addin "nuget:?package=ConsoleMenuLib&version=3.2.1"
#addin "nuget:?package=System.Interactive.Async&version=3.2.0"

using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Cake.Incubator.LoggingExtensions;
using ConsoleUi;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var buildVerbosity = (Verbosity)Enum.Parse(typeof(Verbosity), Argument("buildVerbosity", "Minimal"), ignoreCase: true);

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var solutionPath = "./YamlDotNet.sln";

var releaseConfigurations = new List<string>
{
    "Release"
};

if (!IsRunningOnWindows())
{
    // AOT requires mono
    releaseConfigurations.Add("Debug-AOT");
}

GitVersion version = null;

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
    {
        CleanDirectories(new[]
        {
            "./YamlDotNet/bin",
            "./YamlDotNet.AotTest/bin",
            "./YamlDotNet.Samples/bin",
            "./YamlDotNet.Test/bin",
            "./YamlDotNet/obj",
            "./YamlDotNet.AotTest/obj",
            "./YamlDotNet.Samples/obj",
            "./YamlDotNet.Test/obj",
        });
    });

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        NuGetRestore(solutionPath);
    });

Task("Get-Version")
    .Does(() =>
    {
        version = GitVersion(new GitVersionSettings
        {
            UpdateAssemblyInfo = false,
        });

        if (AppVeyor.IsRunningOnAppVeyor)
        {
            if (!string.IsNullOrEmpty(version.PreReleaseTag))
            {
                version.NuGetVersion = string.Format("{0}-{1}{2}", version.MajorMinorPatch, version.PreReleaseLabel, AppVeyor.Environment.Build.Version.Replace("0.0.", "").PadLeft(4, '0'));
            }
            AppVeyor.UpdateBuildVersion(version.NuGetVersion);
        }

        Information("Building release:\n{0}", version.Dump());
    });

Task("Set-Build-Version")
    .IsDependentOn("Get-Version")
    .Does(() =>
    {
        var assemblyInfo = TransformTextFile("YamlDotNet/Properties/AssemblyInfo.template")
            .WithToken("assemblyVersion", $"{version.Major}.0.0.0")
            .WithToken("assemblyFileVersion", $"{version.MajorMinorPatch}.0")
            .WithToken("assemblyInformationalVersion", version.NuGetVersion)
            .ToString();

        System.IO.File.WriteAllText("YamlDotNet/Properties/AssemblyInfo.cs", assemblyInfo);
    });

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
    {
        BuildSolution(solutionPath, configuration, buildVerbosity);
    });

Task("Quick-Build")
    .Does(() =>
    {
        BuildSolution(solutionPath, configuration, buildVerbosity);
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        RunUnitTests(configuration);
    });

Task("Build-Release-Configurations")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("Set-Build-Version")
    .Does(() =>
    {
        foreach(var releaseConfiguration in releaseConfigurations)
        {
            Information("");
            Information("----------------------------------------");
            Information("Building {0}", releaseConfiguration);
            Information("----------------------------------------");
            BuildSolution(solutionPath, releaseConfiguration, buildVerbosity);
        }
    });

Task("Test-Release-Configurations")
    .IsDependentOn("Build-Release-Configurations")
    .Does(() =>
    {
        foreach(var releaseConfiguration in releaseConfigurations)
        {
            if (releaseConfiguration.Equals("Debug-AOT"))
            {
                RunProcess("mono", "--aot=full", "YamlDotNet.AotTest/bin/Debug/YamlDotNet.dll");
                RunProcess("mono", "--aot=full", "YamlDotNet.AotTest/bin/Debug/YamlDotNet.AotTest.exe");
                RunProcess("mono", "--full-aot", "YamlDotNet.AotTest/bin/Debug/YamlDotNet.AotTest.exe");
            }
            else
            {
                RunUnitTests(releaseConfiguration);
            }
        }
    });

Task("Package")
    .IsDependentOn("Test-Release-Configurations")
    .Does(() =>
    {
        // Replace directory separator char
        var baseNuspecFile = "YamlDotNet/YamlDotNet.nuspec";
        var nuspec = System.IO.File.ReadAllText(baseNuspecFile);

        var finalNuspecFile = baseNuspecFile + ".tmp";
        nuspec = nuspec.Replace('\\', System.IO.Path.DirectorySeparatorChar);
        System.IO.File.WriteAllText(finalNuspecFile, nuspec);

        NuGetPack(finalNuspecFile, new NuGetPackSettings
        {
            Version = version.NuGetVersion,
            OutputDirectory = Directory("YamlDotNet/bin"),
        });
    });

Task("Release")
    .IsDependentOn("Get-Version")
    .WithCriteria(() => version.BranchName == "master", "Releases must be created from the master branch")
    .Does(() =>
    {
        // Find previous release
        var releases = RunProcess("git", "tag", "--list", "--merged", "master", "--format=\"%(refname:short)\"", "v*")
            .Select(tag => new
            {
                Tag = tag,
                Version = ParseSemVer(tag.TrimStart('v')),
            })
            .OrderByDescending(v => v.Version)
            .ToList();

        var previousVersion = releases.First();

        Information("The previous release was {0}", previousVersion.Version);

        var releaseNotesPath = Directory("releases").Path.CombineWithFilePath($"{version.NuGetVersion}.md").FullPath;
        Action scaffoldReleaseNotes = () =>
        {
            // Get the git log to scaffold the release notes
            string currentHash = null;
            var commits = RunProcess("git", "rev-list", $"{previousVersion.Tag}..HEAD", "--first-parent", "--reverse", "--pretty=tformat:%B")
                .Select(l =>
                {
                    var match = Regex.Match(l, "^commit (?<hash>[a-f0-9]+)$");
                    if (match.Success)
                    {
                        currentHash = match.Groups["hash"].Value;
                    }
                    return new
                    {
                        message = l,
                        commit = currentHash
                    };
                })
                .GroupBy(l => l.commit, (k, list) => new
                {
                    commit = k,
                    message = list
                        .Skip(1)
                        .Select(l => Regex.Replace(l.message, @"\+semver:\s*\w+", "").Trim())
                        .Where(l => !string.IsNullOrEmpty(l))
                        .ToList()
                });
            
            var log = commits
                .Select(c => c.message.Select((l, i) => $"{(i == 0 ? '-' : ' ')} {l}"))
                .Select(c => string.Join("  \n", c));

            var releaseNotes = $"# Release {version.NuGetVersion}\n\n{string.Join("\n\n", log)}";
            System.IO.File.WriteAllText(releaseNotesPath, releaseNotes);
        };

        if (!FileExists(releaseNotesPath))
        {
            scaffoldReleaseNotes();
        }

        // Show release menu
        Menu menu = null;

        Action updateMenuDescription = () => menu.Description = System.IO.File.ReadAllText(releaseNotesPath);

        menu = new Menu(
            "Release",
            new ActionMenuItem("Edit release notes", ctx =>
            {
                ctx.SuppressPause();
                RunProcess(IsRunningOnWindows() ? "notepad" : "nano", releaseNotesPath);
                updateMenuDescription();
            }),
            new ActionMenuItem("Scaffold release notes", async ctx =>
            {
                ctx.SuppressPause();
                if (await ctx.UserInterface.Confirm(true, "This will erase the current draft. Are you sure ?"))
                {
                    scaffoldReleaseNotes();
                    updateMenuDescription();
                }
            }),
            new ActionMenuItem("Release", async ctx =>
            {
                ctx.SuppressPause();
                if (await ctx.UserInterface.Confirm(true, "This will publish a new release. Are you sure ?"))
                {
                    menu.ShouldExit = true;

                    var previousReleases = releases
                        .Select(r => new
                        {
                            r.Version,
                            Path = $"releases/{r.Version}.md"
                        })
                        .Where(r => FileExists(r.Path))
                        .Select(r => $"- [{r.Version}]({r.Path})");

                    var releaseNotesFile = string.Join("\n",
                        "# Release notes",
                        menu.Description.Replace("# Release", "## Release"),
                        "# Previous releases",
                        string.Join("\n", previousReleases)
                    );

                    System.IO.File.WriteAllText("RELEASE_NOTES.md", releaseNotesFile);

                    RunProcess("git", "add", $"\"{releaseNotesPath}\"", "RELEASE_NOTES.md");
                    RunProcess("git", "commit", "-m", $"\"Prepare release {version.NuGetVersion}\"");
                    RunProcess("git", "tag", $"v{version.NuGetVersion}");

                    ctx.UserInterface.Info($"Your release is ready. Remember to push it using the following commands:\n\n    git push && git push origin v{version.NuGetVersion}");
                }
            })
        );
        
        updateMenuDescription();
        new ConsoleUi.Console.ConsoleMenuRunner().Run(menu).Wait();
    });

Task("Document")
    .IsDependentOn("Build")
    .Does(() =>
    {
        var samplesBinDir = "YamlDotNet.Samples/bin/" + configuration;
        var testAssemblyFileName = samplesBinDir + "/YamlDotNet.Samples.dll";

        var samplesAssembly = Assembly.LoadFrom(testAssemblyFileName);

        XUnit2(testAssemblyFileName, new XUnit2Settings
        {
            OutputDirectory = Directory(samplesBinDir),
            XmlReport = true
        });

        var samples = XDocument.Load(samplesBinDir + "/YamlDotNet.Samples.dll.xml")
            .Descendants("test")
            .Select(e => new
            {
                Title = e.Attribute("name").Value,
                Type = samplesAssembly.GetType(e.Attribute("type").Value),
                Method = e.Attribute("method").Value,
                Output = e.Element("output") != null ? e.Element("output").Value : null,
            });

        var sampleList = new StringBuilder();

        foreach (var sample in samples)
        {
            var fileName = sample.Type.Name;
            Information("Generating sample documentation page for {0}", fileName);

            var code = System.IO.File.ReadAllText("YamlDotNet.Samples/" + fileName + ".cs");

            var sampleAttr = sample.Type
                .GetMethod(sample.Method)
                .GetCustomAttributes()
                .Single(a => a.GetType().Name == "SampleAttribute");

            var description = UnIndent((string)sampleAttr.GetType().GetProperty("Description").GetValue(sampleAttr, null));

            var samplePage = TransformTextFile("YamlDotNet.Samples/build/SampleTransform.md")
                .WithToken("title", sample.Title)
                .WithToken("description", description)
                .WithToken("code", code)
                .WithToken("output", sample.Output)
                .ToString();

            System.IO.File.WriteAllText("../YamlDotNet.wiki/Samples." + fileName + ".md", samplePage);

            sampleList
                .AppendFormat("* *[{0}](Samples.{1})*  \n", sample.Title, fileName)
                .AppendFormat("  {0}\n", description.Replace("\n", "\n  "));
        }

        var sampleIndexPage = TransformTextFile("YamlDotNet.Samples/build/SampleIndexTransform.md")
            .WithToken("sampleList", sampleList.ToString())
            .ToString();

        System.IO.File.WriteAllText("../YamlDotNet.wiki/Samples.md", sampleIndexPage);
    });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
     // .IsDependentOn("Document");
     .IsDependentOn("Test");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

//////////////////////////////////////////////////////////////////////
// HELPERS
//////////////////////////////////////////////////////////////////////

string UnIndent(string text)
{
    var lines = text
        .Split('\n')
        .Select(l => l.TrimEnd('\r', '\n'))
        .SkipWhile(l => l.Trim(' ', '\t').Length == 0)
        .ToList();

    while (lines.Count > 0 && lines[lines.Count - 1].Trim(' ', '\t').Length == 0)
    {
        lines.RemoveAt(lines.Count - 1);
    }

    if (lines.Count > 0)
    {
        var indent = Regex.Match(lines[0], @"^(\s*)");
        if (!indent.Success)
        {
            throw new ArgumentException("Invalid indentation");
        }

        lines = lines
            .Select(l => l.Substring(indent.Groups[1].Length))
            .ToList();
    }

    return string.Join("\n", lines.ToArray());
}

void BuildSolution(string solutionPath, string configuration, Verbosity verbosity)
{
    const string appVeyorLogger = @"""C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll""";
    MSBuild(solutionPath, settings =>
    {
        if (System.IO.File.Exists(appVeyorLogger)) settings.WithLogger(appVeyorLogger);

        if (IsRunningOnUnix())
        {
            settings.ToolPath = "/usr/bin/msbuild";
        }

        settings
            .SetVerbosity(verbosity)
            .SetConfiguration(configuration)
            .WithProperty("Version", version.NuGetVersion);
    });
}

IEnumerable<string> RunProcess(string processName, params string[] arguments)
{
    var settings = new ProcessSettings()
        .SetRedirectStandardOutput(true)
        .WithArguments(a =>
        {
            foreach (var argument in arguments)
            {
                a.Append(argument);
            }
        });

    using (var process = StartAndReturnProcess(processName, settings))
    {
        var output = new List<string>(process.GetStandardOutput());
        process.WaitForExit();

        var exitCode = process.GetExitCode();
        if (exitCode != 0)
        {
            throw new Exception(string.Format("{0} failed with exit code {1}", processName, exitCode));
        }

        return output;
    }
}

void RunUnitTests(string configurationName)
{
    if (configurationName.Contains("DotNetStandard"))
    {
        // Execute .NETCoreApp tests using `dotnet test`.
        var settings = new DotNetCoreTestSettings
        {
            Framework = "netcoreapp1.0",
            Configuration = configurationName,
            NoBuild = true
        };

        // if (AppVeyor.IsRunningOnAppVeyor)
        // {
        //     settings.ArgumentCustomization = args => args.Append("-appveyor");
        // }

        var path = MakeAbsolute(File("./YamlDotNet.Test/YamlDotNet.Test.csproj"));
        DotNetCoreTest(path.FullPath, settings);
    }
    else
    {
        // Execute the full framework tests using xunit.console.runner.
        // var settings = new XUnit2Settings
        // {
        //     Parallelism = ParallelismOption.All
        // };

        // if (AppVeyor.IsRunningOnAppVeyor)
        // {
        //     settings.ArgumentCustomization = args => args.Append("-appveyor");
        // }

        XUnit2("YamlDotNet.Test/bin/" + configurationName + "/net452/YamlDotNet.Test*.dll");
    }
}
