using System;
using System.IO;
using System.Threading;
using Xunit.Runners;

namespace YamlDotNet.Test
{
    class Program
    {
        // We use consoleLock because messages can arrive in parallel, so we want to make sure we get
        // consistent console output.
        private static readonly object consoleLock = new object();

        // Start out assuming success; we'll set this to 1 if we get a failed test
        private static int result = 0;

        public static int Main(string[] args)
        {
            var testAssembly = typeof(Program).Assembly.Location;

            if (args.Length > 0)
            {
                if (args.Length > 1 || args[0].StartsWith("-"))
                {
                    Console.WriteLine($"usage: {Path.GetFileNameWithoutExtension(testAssembly)} [typeName]");
                    return 2;
                }
            }

            var typeName = args.Length == 1 ? args[0] : null;

            var result = 1;
            var finished = new ManualResetEvent(false);
            using var runner = AssemblyRunner.WithoutAppDomain(testAssembly);
            runner.OnDiscoveryComplete = i => ColoredConsole.WriteLine($"Running {i.TestCasesToRun:White} of {i.TestCasesDiscovered:White} tests...");
            runner.OnExecutionComplete = i =>
            {
                ColoredConsole.WriteLine($"Finished: {i.TotalTests:White} tests in {Math.Round(i.ExecutionTime, 3):White}s ({i.TestsFailed:Red} failed, {i.TestsSkipped:Yellow} skipped)");
                result = i.TestsFailed == 0 ? 0 : 3;
                finished.Set();
            };

            runner.OnTestStarting = i => ColoredConsole.Write($"[{"STRT":White}] {i.TestDisplayName}");
            runner.OnTestPassed = i =>
            {
                Console.CursorLeft = 0;
                ColoredConsole.WriteLine($"[{"PASS":Green}] {i.TestDisplayName}");
            };

            runner.OnTestFailed = i =>
            {
                Console.CursorLeft = 0;
                if (i.ExceptionStackTrace is object)
                {
                    ColoredConsole.WriteLine($"[{"FAIL":Red}] {i.TestDisplayName}\n      {i.ExceptionMessage.Replace("\n", "\n      ")}\n      {i.ExceptionStackTrace.Replace("\n", "\n      "):DarkGray}");
                }
                else
                {
                    ColoredConsole.WriteLine($"[{"FAIL":Red}] {i.TestDisplayName}\n      {i.ExceptionMessage.Replace("\n", "\n      ")}");
                }
            };

            runner.OnTestSkipped = i =>
            {
                Console.CursorLeft = 0;
                ColoredConsole.WriteLine($"[{"SKIP":Yellow}] {i.TestDisplayName}");
            };

            Console.WriteLine("Discovering...");
            runner.Start(typeName, parallel: false);

            finished.WaitOne();
            finished.Dispose();

            return result;
        }
    }
}
