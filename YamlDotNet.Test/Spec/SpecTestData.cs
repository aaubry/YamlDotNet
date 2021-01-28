// This file is part of YamlDotNet - A .NET library for YAML.
// Copyright (c) Antoine Aubry and contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace YamlDotNet.Test.Spec
{
    internal abstract class SpecTestsData : IEnumerable<object[]>
    {
        private const string DescriptionFilename = "===";
        private const string InputFilename = "in.yaml";
        private const string OutputFilename = "out.yaml";
        private const string ExpectedEventFilename = "test.event";
        private const string ErrorFilename = "error";

        private static readonly string specFixtureDirectory = GetTestFixtureDirectory();

        protected abstract List<string> IgnoredSuites { get; }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<object[]> GetEnumerator()
        {
            var fixtures = Directory.EnumerateDirectories(specFixtureDirectory, "*", SearchOption.TopDirectoryOnly);

            foreach (var testPath in fixtures)
            {
                var testName = Path.GetFileName(testPath);
                // comment the following line to run spec tests (requires 'Rebuild')
                if (IgnoredSuites.Contains(testName)) continue;

                var inputFile = Path.Combine(testPath, InputFilename);
                if (!File.Exists(inputFile)) continue;

                var descriptionFile = Path.Combine(testPath, DescriptionFilename);
                var hasErrorFile = File.Exists(Path.Combine(testPath, ErrorFilename));
                var expectedEventFile = Path.Combine(testPath, ExpectedEventFilename);

                var outputFile = Path.Combine(testPath, OutputFilename);
                if (!File.Exists(outputFile)) outputFile = inputFile;

                yield return new object[]
                {
                    testName,
                    File.ReadAllText(descriptionFile).TrimEnd(),
                    inputFile,
                    (this is SerializerSpecTests.SerializerSpecTestsData) ? outputFile : expectedEventFile,
                    hasErrorFile
                };
            }
        }

        private static string GetTestFixtureDirectory()
        {
            // check if environment variable YAMLDOTNET_SPEC_SUITE_DIR is set
            string fixturesPath = Environment.GetEnvironmentVariable("YAMLDOTNET_SPEC_SUITE_DIR");

            if (!string.IsNullOrEmpty(fixturesPath))
            {
                if (!Directory.Exists(fixturesPath))
                {
                    throw new Exception("Path set as environment variable 'YAMLDOTNET_SPEC_SUITE_DIR' does not exist!");
                }

                return fixturesPath;
            }

            // In Microsoft.NET.Test.Sdk v15.0.0, the current working directory
            // is not set to project's root but instead the output directory.
            // see: https://github.com/Microsoft/vstest/issues/435.
            //
            // Let's use the strategry of finding the parent directory of
            // "yaml-test-suite" directory by walking from cwd backwards upto the
            // volume's root.
            var currentDirectory = Directory.GetCurrentDirectory();
            var currentDirectoryInfo = new DirectoryInfo(currentDirectory);

            do
            {
                if (Directory.Exists(Path.Combine(currentDirectoryInfo.FullName, "yaml-test-suite")))
                {
                    return Path.Combine(currentDirectoryInfo.FullName, "yaml-test-suite");
                }
                currentDirectoryInfo = currentDirectoryInfo.Parent;
            }
            while (currentDirectoryInfo.Parent != null);

            throw new DirectoryNotFoundException("Unable to find 'yaml-test-suite' directory");
        }
    }
}
