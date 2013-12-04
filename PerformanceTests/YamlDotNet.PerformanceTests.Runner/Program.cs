//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2013 Antoine Aubry
    
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

namespace YamlDotNet.PerformanceTests.Runner
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			var configuration = Path.GetFileName(Directory.GetCurrentDirectory());

			var currentDir = Directory.GetCurrentDirectory();
			var baseDir = currentDir;
			for(var i = 0; i < 3; ++i)
			{
				baseDir = Path.GetDirectoryName(baseDir);
			}

			var testPrograms = Directory.GetDirectories(baseDir)
				.Select(d => Path.Combine(d, Path.Combine("bin", configuration)))
				.Where(d => d != currentDir)
				.SelectMany(d => Directory.GetFiles(d, "*.exe"));

			var testResults = new List<TestResult>();
			foreach(var testProgram in testPrograms)
			{
				Console.Error.WriteLine("Running {0}", Path.GetFileName(testProgram));
				RunTest(testProgram, testResults);
			}

			var versions = testResults
				.Select(r => r.Version)
				.Distinct()
				.OrderBy(r => r)
				.ToList();

			const int columnWidth = 10;
			var initialColumnWidth = testResults.Max(r => r.Test.Length) + 1;

			Console.WriteLine();
			Console.Write(new String(' ', initialColumnWidth));
			foreach(var version in versions)
			{
				Console.Write("{0}", version.PadLeft(10));
			}
			Console.WriteLine();

			Console.WriteLine(new String('-', initialColumnWidth + columnWidth * versions.Count));

			foreach(var resultGroup in testResults.GroupBy(r => r.Test))
			{
				Console.Write(resultGroup.Key.PadRight(initialColumnWidth));

				foreach(var version in versions)
				{
					var result = resultGroup.FirstOrDefault(r => r.Version == version);
					if(result != null)
					{
						Console.Write(result.Duration.ToString("##0.00 ms").PadLeft(columnWidth));
					}
					else
					{
						Console.Write("N/A".PadRight(columnWidth));
					}
				}

				Console.WriteLine();
			}

			Console.Error.WriteLine();
			Console.Error.WriteLine("Done.");
		}

		private static void RunTest(string testProgram, List<TestResult> testResults)
		{
			var startInfo = new ProcessStartInfo
			{
				UseShellExecute = false,
				RedirectStandardOutput = true,
			};

			switch(Environment.OSVersion.Platform)
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
			public string Version { get; set; }
			public string Test { get; set; }
			public double Duration { get; set; }
		}

		private static void ProcessTestResult(string data, List<TestResult> testResults)
		{
			if (data != null)
			{
				var parts = data.Split('\t');
				testResults.Add(new TestResult
				{
					Version = parts[0],
					Test = parts[1],
					Duration = double.Parse(parts[2]),
				});
			}
		}
	}
}
