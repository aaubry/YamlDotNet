using Bullseye;
using Bullseye.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using static Bullseye.Targets;
using OperatingSystem = Bullseye.Internal.OperatingSystem;

namespace build
{
    class Program
    {
        public static string BasePath { get; private set; } = default!;
        private static Palette palette = default!;
        public static bool NoPrerelease { get; private set; }
        private static bool verbose;
        private static Host host;
        private static readonly Dictionary<Type, object> state = new Dictionary<Type, object>();

        private static T GetState<T>() where T : notnull
        {
            return state.TryGetValue(typeof(T), out var value)
                ? (T)value
                : throw new InvalidOperationException($"The target that produces state '{typeof(T).FullName}' has not been executed yet.");
        }

        private static void SetState<T>(T value) where T : notnull
        {
            state.Add(typeof(T), value);
        }

        private static void RegisterTargets()
        {
            var getState = typeof(Program).GetMethod(nameof(GetState), BindingFlags.Static | BindingFlags.NonPublic) ?? throw new MissingMethodException("Method not found", nameof(GetState));
            var setState = typeof(Program).GetMethod(nameof(SetState), BindingFlags.Static | BindingFlags.NonPublic) ?? throw new MissingMethodException("Method not found", nameof(SetState));

            var providerTargets = new Dictionary<Type, string>();

            var targetMethods = typeof(BuildDefinition).GetMethods(BindingFlags.Static | BindingFlags.Public);
            var targets = new List<(string name, Action action, IEnumerable<Type> dependencies)>();

            foreach (var targetMethod in targetMethods)
            {
                var dependencies = targetMethod
                    .GetParameters()
                    .Select(p => p.ParameterType)
                    .ToList();

                Expression actionExpression = Expression.Call(
                    targetMethod,
                    dependencies.Select(d => Expression.Call(getState.MakeGenericMethod(d)))
                );

                var returnType = targetMethod.ReturnType;
                var isAsync = typeof(Task).IsAssignableFrom(returnType);
                if (isAsync)
                {
                    returnType = returnType.IsGenericType ? returnType.GetGenericArguments()[0] : typeof(void);

                    if (returnType != typeof(void))
                    {
                        actionExpression = Expression.Property(
                            actionExpression,
                            nameof(Task<object>.Result)
                        ); ;
                    }
                    else
                    {
                        actionExpression = Expression.Call(
                            actionExpression,
                            nameof(Task.Wait),
                            null
                        );
                    }
                }

                if (returnType != typeof(void))
                {
                    actionExpression = Expression.Call(
                        setState.MakeGenericMethod(returnType),
                        actionExpression
                    );

                    if (providerTargets.ContainsKey(returnType))
                    {
                        var duplicates = targetMethods.Where(m => m.ReturnType == returnType).Select(m => m.Name);
                        throw new InvalidOperationException($"Multiple targets provide the same type '{returnType.FullName}': {string.Join(", ", duplicates)}");
                    }

                    providerTargets.Add(returnType, targetMethod.Name);
                }

                var action = Expression.Lambda<Action>(actionExpression);
                targets.Add((targetMethod.Name, action.Compile(), dependencies));
            }

            foreach (var (name, action, dependencies) in targets)
            {
                var dependendsOn = dependencies
                    .Where(d => !state.ContainsKey(d))
                    .Select(d => providerTargets.TryGetValue(d, out var dependencyName) ? dependencyName : throw new InvalidOperationException($"Target '{name}' depends on '{d.FullName}', but no target provides it."));

                Target(name, dependendsOn, action);
            }
        }

        static int Main(string[] args)
        {
            var filteredArguments = args
                .Where(a =>
                {
                    switch (a)
                    {
                        case "--no-prerelease":
                            NoPrerelease = true;
                            return false;

                        default:
                            return true;
                    }
                })
                .ToList();

            var (options, targets) = Options.Parse(filteredArguments);
            verbose = options.Verbose;
            host = options.Host.DetectIfUnknown().Item1;

            var operatingSystem =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? OperatingSystem.Windows
                    : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                        ? OperatingSystem.Linux
                        : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                            ? OperatingSystem.MacOS
                            : OperatingSystem.Unknown;

            palette = new Palette(options.NoColor, options.NoExtendedChars, options.Host, operatingSystem);

            if (targets.Count == 0 && !options.ShowHelp)
            {
                switch (options.Host)
                {
                    case Host.Appveyor:

                        // Default CI targets for AppVeyor
                        var isBuildingMaster = Environment.GetEnvironmentVariable("APPVEYOR_REPO_BRANCH")?.Equals("master", StringComparison.Ordinal) ?? true;
                        var isBuildingRelease = Environment.GetEnvironmentVariable("APPVEYOR_REPO_TAG")?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;
                        if (isBuildingRelease || !isBuildingMaster)
                        {
                            WriteInformation("Publishable build detected");
                            targets.Add(nameof(BuildDefinition.SetBuildVersion));
                            targets.Add(nameof(BuildDefinition.Publish));
                            targets.Add(nameof(BuildDefinition.TweetRelease));
                        }
                        else
                        {
                            WriteInformation("Non-publishable build detected");
                            targets.Add(nameof(BuildDefinition.SetBuildVersion));
                            targets.Add(nameof(BuildDefinition.Pack));
                        }
                        break;

                    default:
                        (options, targets) = Options.Parse(filteredArguments.Append("-?"));
                        break;
                }
            }

            SetState(options);

            var initialDirectory = Directory.GetCurrentDirectory();
            BasePath = initialDirectory;
            while (!File.Exists(Path.Combine(BasePath, "YamlDotNet.sln")))
            {
                BasePath = Path.GetDirectoryName(BasePath) ?? throw new InvalidOperationException($"Could not find YamlDotNet.sln starting from '{initialDirectory}'.");
            }

            WriteVerbose($"BasePath: {BasePath}");

            RegisterTargets();

            int exitCode = 0;
            try
            {
                RunTargetsWithoutExiting(targets, options);
            }
            catch (TargetFailedException)
            {
                exitCode = 1;
            }

            if (options.ShowHelp)
            {
                Console.WriteLine();
                Console.WriteLine($"{palette.Default}Additional options:");
                Console.WriteLine($"  {palette.Option}--no-prerelease            {palette.Default}Force the current version to be considered final{palette.Reset}");
            }

            return exitCode;
        }

        public static void WriteVerbose(string text)
        {
            if (verbose)
            {
                Write(text, palette.Verbose);
            }
        }

        public static void WriteInformation(string text)
        {
            Console.WriteLine($"  {palette.Default}(i)  {text}{palette.Reset}");
        }

        public static void WriteWarning(string text)
        {
            Console.WriteLine($"  {palette.Option}/!\\  {text}{palette.Reset}");
        }

        public static void WriteImportant(string text)
        {
            switch (host)
            {
                case Host.GitHubActions:
                    Console.WriteLine($"Writing a warning");
                    Console.WriteLine($"::warning ::{text.Replace("\\n", "%0A")}");
                    break;
            }
            WriteBoxed(text, palette.Warning);
        }

        private static void WriteBoxed(string text, string color)
        {
            var boxElements = palette.Dash == '─'
                ? "┌┐└┘│─"
                : "++++|-";

            var boxWidth = 50;
            var wrappedText = WrapText(text, boxWidth - 4).ToList();
            boxWidth = Math.Max(boxWidth, wrappedText.Max(l => l.Length) + 4);

            Console.WriteLine();
            Write($"       {boxElements[0]}{new string(boxElements[5], boxWidth - 2)}{boxElements[1]}", color);

            foreach (var line in WrapText(text, boxWidth - 4))
            {
                Write($"       {boxElements[4]} {line.PadRight(boxWidth - 4)} {boxElements[4]}", color);
            }

            Write($"       {boxElements[2]}{new string(boxElements[5], boxWidth - 2)}{boxElements[3]}", color);
            Console.WriteLine();
        }

        private static IEnumerable<string> WrapText(string text, int length)
        {
            foreach (var textLine in text.Split('\n'))
            {
                var line = new StringBuilder();
                foreach (var word in textLine.Split(' '))
                {
                    if (line.Length == 0)
                    {
                        line.Append(word);
                    }
                    else if (line.Length + word.Length + 1 < length)
                    {
                        line.Append(' ').Append(word);
                    }
                    else
                    {
                        yield return line.ToString();
                        line.Clear().Append(word);
                    }
                }

                if (line.Length > 0 || textLine.Length == 0)
                {
                    yield return line.ToString();
                }
            }
        }

        private static void Write(string text, string color)
        {
            Console.WriteLine($"{color}{text}{palette.Reset}");
        }

        public static IEnumerable<string> ReadLines(string name, string? args = null, string? workingDirectory = null) => SimpleExec.Command
            .Read(name, args, workingDirectory)
            .Split('\n')
            .Select(l => l.TrimEnd('\r'));

        public static string UnIndent(string text)
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
    }
}
