//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) Antoine Aubry and contributors

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Extensions;
using BenchmarkDotNet.Running;

namespace YamlDotNet.PerformanceTests.Runner
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var currentDir = Directory.GetCurrentDirectory();
            
            var baseDir = currentDir;
            for (var i = 0; i < 4; ++i)
            {
                baseDir = Path.GetDirectoryName(baseDir);
            }

            var baseDirLength = currentDir.IndexOf($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}");

            var configuration = currentDir.Substring(baseDirLength, currentDir.Length - baseDirLength).Trim(Path.DirectorySeparatorChar);

            Console.WriteLine($"Configuration: {configuration}\n");

            var testPrograms = Directory.GetDirectories(baseDir)
                .Select(d => Path.Combine(d, configuration))
                .Where(d => d != currentDir)
                .Where(Directory.Exists)
                .SelectMany(d => Directory.GetFiles(d, "*.exe"))
                .Where(f => Regex.IsMatch(f, @"YamlDotNet.PerformanceTests.(vlatest|v\d+\.\d+\.\d+)\.exe$"));

            var testResults = new List<TestResult>();
            foreach (var testProgram in testPrograms)
            {
                Console.Error.WriteLine("Running {0}", Path.GetFileName(testProgram));

                RunTest(testProgram, testResults);
            }

            PrintResult(testResults);
        }

        private static void PrintResult(List<TestResult> testResults)
        {
            var tests = testResults
                .Select(r => r.Test)
                .Distinct()
                .OrderBy(r => r)
                .ToList();

            const int columnWidth = 9;
            const int metricsCount = 6;

            foreach (var test in tests)
            {
                var resultsFromTest = testResults.Where(r => r.Test == test).OrderBy(p => p.Version).ToList();

                var initialColumnWidth = resultsFromTest.Max(r => r.Version.Length);
                var tableWith = initialColumnWidth + (columnWidth + 2) * metricsCount + metricsCount + 4;
                Console.WriteLine();

                PrintLine(tableWith);

                Console.WriteLine($"| {test.PadRight(tableWith - 3)}|");

                PrintLine(tableWith);

                Console.Write($"| {string.Empty.PadLeft(initialColumnWidth)} |");
                Console.Write($" {nameof(TestResult.Mean).PadLeft(columnWidth)} |");
                Console.Write($" {nameof(TestResult.Error).PadLeft(columnWidth)} |");
                Console.Write($" {nameof(TestResult.StdDev).PadLeft(columnWidth)} |");
                Console.Write($" {nameof(TestResult.Gen0).PadLeft(columnWidth)} |");
                Console.Write($" {nameof(TestResult.Gen1).PadLeft(columnWidth)} |");
                Console.Write($" {nameof(TestResult.Allocated).PadLeft(columnWidth)} |");

                Console.WriteLine();

                PrintLine(tableWith);

                foreach (var result in resultsFromTest)
                {
                    Console.Write($"| {result.Version.PadRight(initialColumnWidth)} |");
                    Console.Write($" {result.Mean?.PadLeft(columnWidth)} |");
                    Console.Write($" {result.Error?.PadLeft(columnWidth)} |");
                    Console.Write($" {result.StdDev?.PadLeft(columnWidth)} |");
                    Console.Write($" {result.Gen0?.PadLeft(columnWidth)} |");
                    Console.Write($" {result.Gen1?.PadLeft(columnWidth)} |");
                    Console.Write($" {result.Allocated?.PadLeft(columnWidth)} |");

                    Console.WriteLine();
                }
                PrintLine(tableWith);
            }

            Console.Error.WriteLine();
            Console.Error.WriteLine("Done.");
        }

        private static void PrintLine(int tableWith)
        {
            Console.WriteLine(new String('-', tableWith));
        }

        private static void RunTest(string testProgram, List<TestResult> testResults)
        {
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                    startInfo.FileName = "mono";
                    startInfo.Arguments = testProgram;
                    break;

                default:
                    startInfo.FileName = testProgram;
                    break;
            }

            var testProcess = Process.Start(startInfo);
            testProcess.OutputDataReceived += (s, e) => ProcessTestResult(e.Data, testResults);
            testProcess.BeginOutputReadLine();

            testProcess.WaitForExit();
        }

        private class TestResult
        {
            public string Test { get; set; }
            public string Version { get; set; }
            public string Mean { get; set; }
            public string Error { get; set; }
            public string StdDev { get; set; }
            public string Gen0 { get; set; }
            public string Gen1 { get; set; }
            public string Allocated { get; set; }
        }

        private static void ProcessTestResult(string data, List<TestResult> testResults)
        {
            if (data != null && data.StartsWith(" 'Serialize v"))
            {
                var parts = data.Split('|');
                var versionName = parts[0].Trim().Trim('\'').Split(' ');
                var result = new TestResult
                {
                    Test = versionName[0],
                    Version = versionName[1],
                    Mean = parts.Length >= 1 ? parts[1].Trim() : string.Empty,
                    Error = parts.Length >= 2 ? parts[2].Trim() : string.Empty,
                    StdDev = parts.Length >= 3 ? parts[3].Trim() : string.Empty,
                    Gen0 = parts.Length >= 4 ? parts[4].Trim() : string.Empty,
                    Gen1 = parts.Length >= 5 ? parts[5].Trim() : string.Empty,
                    Allocated = parts.Length >= 6 ? parts[6].Trim() : string.Empty,
                };

                testResults.Add(result);
            }
        }
    }
}
