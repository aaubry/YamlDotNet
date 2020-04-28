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
using System.IO;

namespace YamlDotNet.Test.Spec
{
    public static class TestFixtureHelper
    {
        public static string GetTestFixtureDirectory(string environmentVariableName, string directoryName)
        {
            // check if environment variable 'environmentVariableName' is set
            string fixturesPath = Environment.GetEnvironmentVariable(environmentVariableName);

            if (!string.IsNullOrEmpty(fixturesPath))
            {
                if (!Directory.Exists(fixturesPath))
                {
                    throw new Exception($"Path set as environment variable '{environmentVariableName}'({fixturesPath}) does not exist!");
                }

                return fixturesPath;
            }

            // In Microsoft.NET.Test.Sdk v15.0.0, the current working directory
            // is not set to project's root but instead the output directory.
            // see: https://github.com/Microsoft/vstest/issues/435.
            //
            // Let's use the strategry of finding the parent directory of
            // 'directoryName' directory by walking from cwd backwards upto the
            // volume's root.
            var currentDirectory = Directory.GetCurrentDirectory();
            var currentDirectoryInfo = new DirectoryInfo(currentDirectory);

            do
            {
                if (Directory.Exists(Path.Combine(currentDirectoryInfo.FullName, directoryName)))
                {
                    return Path.Combine(currentDirectoryInfo.FullName, directoryName);
                }
                currentDirectoryInfo = currentDirectoryInfo.Parent;
            }
            while (currentDirectoryInfo.Parent != null);

            throw new DirectoryNotFoundException($"Unable to find '{directoryName}' directory");
        }
    }
}
