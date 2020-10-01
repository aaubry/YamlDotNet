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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;

namespace YamlDotNet.PerformanceTests.Runner
{
    class MainClass
    {
        public static void Main()
        {
            var currentDir = Directory.GetCurrentDirectory();

            var baseDir = currentDir;
            for (var i = 0; i < 4; ++i)
            {
                baseDir = Path.GetDirectoryName(baseDir);
            }

            var testsBaseDir = Path.Combine(baseDir, "YamlDotNet.PerformanceTests", "bin");

            var testPrograms = Directory.GetDirectories(testsBaseDir)
                .SelectMany(d => Directory.GetDirectories(d))
                .SelectMany(d => new[] { Path.Combine(d, "YamlDotNet.PerformanceTests.exe"), Path.Combine(d, "YamlDotNet.PerformanceTests.dll") })
                .Where(f => File.Exists(f))
                .GroupBy(f => Path.GetDirectoryName(f), (_, f) => f.OrderBy(fn => Path.GetExtension(fn)).First()) // Favor .dll over .exe
                //.Where(d => d.Contains("5.2.0")).Take(1)
                .Select(f => new
                {
                    Path = f,
                    Framework = Path.GetFileName(Path.GetDirectoryName(f)),
                    Version = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(f))),
                })
                .OrderBy(p => p.Version == "latest" ? new Version(999, 999, 999) : Version.Parse(p.Version)).ThenBy(p => p.Framework)
                .ToList();

            Console.Error.WriteLine($"Base dir: {testsBaseDir}");
            Console.Error.WriteLine("Discovered the following tests:");
            foreach (var testProgram in testPrograms)
            {
                Console.Error.WriteLine($"  - version {testProgram.Version,-7} {testProgram.Framework,-13}  .{testProgram.Path.Substring(testsBaseDir.Length)}");
            }

            var testResults = new List<TestResult>();

            for (int i = 0; i < testPrograms.Count; i++)
            {
                var testProgram = testPrograms[i];

                Console.Title = $"Running tests for YamlDotNet {testProgram.Version} for {testProgram.Framework} ({i + 1} of {testPrograms.Count})";

                RunTest(testProgram.Path, testProgram.Version, testProgram.Framework, testResults);
            }

            Console.Title = "Performance test results for YamlDotNet";

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
                var tableWith = initialColumnWidth + 16 + (columnWidth + 2) * metricsCount + metricsCount + 4;
                Console.WriteLine();

                PrintLine(tableWith);

                Console.WriteLine($"| {test.PadRight(tableWith - 3)}|");

                PrintLine(tableWith);

                Console.Write($"| {string.Empty.PadLeft(initialColumnWidth)} |");
                Console.Write($" {nameof(TestResult.Version),-13} |");
                Console.Write($" {nameof(TestResult.Mean),columnWidth} |");
                Console.Write($" {nameof(TestResult.Error),columnWidth} |");
                Console.Write($" {nameof(TestResult.StdDev),columnWidth} |");
                Console.Write($" {nameof(TestResult.Gen0),columnWidth} |");
                Console.Write($" {nameof(TestResult.Gen1),columnWidth} |");
                Console.Write($" {nameof(TestResult.Allocated),columnWidth} |");

                Console.WriteLine();

                PrintLine(tableWith);

                static string FormatTime(double time)
                {
                    if (double.IsNaN(time))
                    {
                        return "N/A";
                    }

                    var units = new[] { "ns", "us", "ms", "s" };
                    var currentUnit = units[0];
                    foreach (var unit in units)
                    {
                        currentUnit = unit;

                        if (time < 1000.0)
                        {
                            break;
                        }

                        time /= 1000.0;
                    }

                    return $"{time:G5} {currentUnit}";
                }

                foreach (var result in resultsFromTest)
                {
                    Console.Write($"| {result.Version.PadRight(initialColumnWidth)} |");
                    Console.Write($" {result.Framework,-13} |");
                    Console.Write($" {FormatTime(result.Mean),columnWidth} |");
                    Console.Write($" {FormatTime(result.Error),columnWidth} |");
                    Console.Write($" {FormatTime(result.StdDev),columnWidth} |");
                    Console.Write($" {result.Gen0,columnWidth} |");
                    Console.Write($" {result.Gen1,columnWidth} |");
                    Console.Write($" {result.Allocated,columnWidth} |");

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

        private static void RunTest(string testProgram, string version, string framework, List<TestResult> testResults)
        {
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
            };

            switch (Path.GetExtension(testProgram))
            {
                case ".dll":
                    startInfo.FileName = "dotnet";
                    startInfo.Arguments = testProgram;
                    break;

                case ".exe" when Environment.OSVersion.Platform == PlatformID.Unix:
                    startInfo.FileName = "mono";
                    startInfo.Arguments = testProgram;
                    break;

                case ".exe":
                    startInfo.FileName = testProgram;
                    break;

                default:
                    throw new NotSupportedException();
            }

            var reportPath = Path.Combine("BenchmarkDotNet.Artifacts", "results", "YamlDotNet.PerformanceTests.ReceiptTest-report-brief.json");
            if (File.Exists(reportPath))
            {
                File.Delete(reportPath);
            }

            var testProcess = Process.Start(startInfo);
            testProcess.WaitForExit();

            if (File.Exists(reportPath))
            {
                using var reportFile = File.OpenText(reportPath);
                var report = JsonParser.Deserialize<BriefReport>(reportFile);

                foreach (var benchmark in report.Benchmarks)
                {
                    testResults.Add(new TestResult
                    {
                        Test = $"{benchmark.Type}.{benchmark.Method}",
                        Version = version,
                        Framework = framework,
                        Mean = benchmark.Statistics?.Mean ?? double.NaN,
                        Error = benchmark.Statistics?.StandardError ?? double.NaN,
                        StdDev = benchmark.Statistics?.StandardDeviation ?? double.NaN,
                        Gen0 = benchmark.Memory?.Gen0Collections ?? -1,
                        Gen1 = benchmark.Memory?.Gen1Collections ?? -1,
                        Allocated = benchmark.Memory?.BytesAllocatedPerOperation ?? -1,
                    });
                }
            }
            else
            {
                testResults.Add(new TestResult
                {
                    Test = "INVALID",
                    Version = version,
                    Framework = framework,
                    Mean = double.NaN,
                    Error = double.NaN,
                    StdDev = double.NaN,
                    Gen0 = -1,
                    Gen1 = -1,
                    Allocated = -1,
                });
            }
        }

        private static readonly IDeserializer JsonParser = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();

        private class BriefReport
        {
            public List<Benchmark> Benchmarks { get; set; }
        }

        private class Benchmark
        {
            public string Type { get; set; }
            public string Method { get; set; }

            public Statistics Statistics { get; set; }
            public Memory Memory { get; set; }
        }

        private class Statistics
        {
            public int N { get; set; }
            public double Median { get; set; }
            public double Mean { get; set; }
            public double StandardError { get; set; }
            public double StandardDeviation { get; set; }
        }

        private class Memory
        {
            public int Gen0Collections { get; set; }
            public int Gen1Collections { get; set; }
            public int Gen2Collections { get; set; }
            public int TotalOperations { get; set; }
            public int BytesAllocatedPerOperation { get; set; }
        }

        private class TestResult
        {
            public string Test { get; set; }
            public string Version { get; set; }
            public string Framework { get; internal set; }
            public double Mean { get; set; }
            public double Error { get; set; }
            public double StdDev { get; set; }
            public int Gen0 { get; set; }
            public int Gen1 { get; set; }
            public int Allocated { get; set; }
        }
    }
}
