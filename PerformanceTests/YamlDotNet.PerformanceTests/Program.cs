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

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using System;
using System.Reflection;

namespace YamlDotNet.PerformanceTests
{
    public class Program
    {
        private static BuildPropertiesAttribute buildProperties;

        public static BuildPropertiesAttribute BuildProperties
        {
            get
            {
                if (buildProperties is null)
                {
                    buildProperties = typeof(Program).Assembly.GetCustomAttribute<BuildPropertiesAttribute>()
                        ?? throw new InvalidOperationException("Missing build properties");
                }
                return buildProperties;
            }
        }

        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<ReceiptTest>(DefaultConfig.Instance
                .AddJob(Job.Default
                    //.WithArguments(new[]
                    //{
                    //    //new MsBuildArgument($"/p:{nameof(BuildProperties.BaseIntermediateOutputPath)}={BuildProperties.BaseIntermediateOutputPath}"),
                    //    //new MsBuildArgument($"/p:{nameof(BuildProperties.MSBuildProjectExtensionsPath)}={BuildProperties.MSBuildProjectExtensionsPath}"),
                    //    //new MsBuildArgument($"/p:{nameof(BuildProperties.TestVersion)}={BuildProperties.TestVersion}"),
                    //})
#if NETCOREAPP3_1
                    .WithToolchain(BenchmarkDotNet.Toolchains.CsProj.CsProjCoreToolchain.NetCoreApp31)
#endif
                )
            );
        }
    }
}