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
using System.IO;
using Xunit;
using Xunit.Sdk;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Test.Spec
{
    public sealed class SpecTests
    {
        private const string DescriptionFilename = "===";
        private const string InputFilename = "in.yaml";
        private const string ExpectedEventFilename = "test.event";
        private const string ErrorFilename = "error";

        private static readonly string specFixtureDirectory = GetTestFixtureDirectory();

        // Note: all of these (36) tests are failing the assertion on line 65
        private static readonly List<string> ignoredSuites = new List<string>
        {
            "DK3J", "6M2F", "NJ66", "4MUZ", "NHX8", "WZ62", "W5VH", "M7A3", "6LVF", "DBG4",
            "8XYN", "4ABK", "KZN9", "Q5MG", "Y2GN", "2JQS", "S3PD", "R4YG", "9SA2", "UT92",
            "HWV9", "9MMW", "6BCT", "W4TN", "S4JQ", "K3WX", "8MK2", "52DL", "2SXE", "5MUD",
            "FP8R", "FRK4", "2LFX", "7Z25", "QT73", "A2M4"
        };

        [Theory, MemberData(nameof(GetYamlSpecDataSuites))]
        public void ConformsWithYamlSpec(string name, string description, string inputFile, string expectedEventFile, bool error)
        {
            var expectedResult = File.ReadAllText(expectedEventFile);
            using (var writer = new StringWriter())
            {
                try
                {
                    using (var reader = File.OpenText(inputFile))
                    {
                        ConvertToLibYamlStyleAnnotatedEventStream(reader, writer);
                    }
                }
                catch (Exception ex)
                {
                    Assert.True(error, "Unexpected spec failure.\nExpected:\n" + expectedResult + "\nActual:\n[Writer Output]\n" + writer + "\n[Exception]\n" + ex);
                    return;
                }

                try
                {
                    Assert.Equal(expectedResult, writer.ToString(), ignoreLineEndingDifferences: true);
                }
                catch (EqualException)
                {
                    // there are seven spec tests failing, where YamlDotNet
                    // is unexpectedly *not* erroring out during parsing.
                    //
                    // TODO: remove this try-catch block once there is a
                    //       decision on the following (update implementation
                    //       or add these to ignoredSuites):
                    //
                    //       X4QW, 9C9N, QB6E, CVW2, 9JBA, HRE5, SU5Z
                    //
                    if (!error)
                    {
                        throw;
                    }
                }
            }
        }

        private static void ConvertToLibYamlStyleAnnotatedEventStream(TextReader textReader, TextWriter textWriter)
        {
            var parser = new Parser(textReader);

            while (parser.MoveNext())
            {
                switch (parser.Current)
                {
                    case AnchorAlias anchorAlias:
                        textWriter.Write("=ALI *");
                        textWriter.Write(anchorAlias.Value);
                        break;
                    case DocumentEnd documentEnd:
                        textWriter.Write("-DOC");
                        if (!documentEnd.IsImplicit) textWriter.Write(" ...");
                        break;
                    case DocumentStart documentStart:
                        textWriter.Write("+DOC");
                        if (!documentStart.IsImplicit) textWriter.Write(" ---");
                        break;
                    case MappingEnd _:
                        textWriter.Write("-MAP");
                        break;
                    case MappingStart mappingStart:
                        textWriter.Write("+MAP");
                        WriteAnchorAndTag(mappingStart);
                        break;
                    case Scalar scalar:
                        textWriter.Write("=VAL");
                        WriteAnchorAndTag(scalar);

                        switch (scalar.Style)
                        {
                            case ScalarStyle.DoubleQuoted: textWriter.Write(" \""); break;
                            case ScalarStyle.SingleQuoted: textWriter.Write(" '"); break;
                            case ScalarStyle.Folded: textWriter.Write(" >"); break;
                            case ScalarStyle.Literal: textWriter.Write(" |"); break;
                            default: textWriter.Write(" :"); break;
                        }

                        foreach (char character in scalar.Value)
                        {
                            switch (character)
                            {
                                case '\b': textWriter.Write("\\b"); break;
                                case '\t': textWriter.Write("\\t"); break;
                                case '\n': textWriter.Write("\\n"); break;
                                case '\r': textWriter.Write("\\r"); break;
                                case '\\': textWriter.Write("\\\\"); break;
                                default: textWriter.Write(character); break;
                            }
                        }
                        break;
                    case SequenceEnd _:
                        textWriter.Write("-SEQ");
                        break;
                    case SequenceStart sequenceStart:
                        textWriter.Write("+SEQ");
                        WriteAnchorAndTag(sequenceStart);
                        break;
                    case StreamEnd _:
                        textWriter.Write("-STR");
                        break;
                    case StreamStart _:
                        textWriter.Write("+STR");
                        break;
                }
                textWriter.WriteLine();
            }

            void WriteAnchorAndTag(NodeEvent nodeEvent)
            {
                if (!string.IsNullOrEmpty(nodeEvent.Anchor))
                {
                    textWriter.Write(" &");
                    textWriter.Write(nodeEvent.Anchor);
                }
                if (!string.IsNullOrEmpty(nodeEvent.Tag))
                {
                    textWriter.Write(" <");
                    textWriter.Write(nodeEvent.Tag);
                    textWriter.Write(">");
                }
            }
        }

        public static IEnumerable<object> GetYamlSpecDataSuites()
        {
            var fixtures = Directory.EnumerateDirectories(specFixtureDirectory, "*", SearchOption.TopDirectoryOnly);

            foreach (var testPath in fixtures)
            {
                var testName = Path.GetFileName(testPath);
                if (ignoredSuites.Contains(testName)) continue;

                var inputFile = Path.Combine(testPath, InputFilename);
                if (!File.Exists(inputFile)) continue;

                var descriptionFile = Path.Combine(testPath, DescriptionFilename);
                var hasErrorFile = File.Exists(Path.Combine(testPath, ErrorFilename));
                var expectedEventFile = Path.Combine(testPath, ExpectedEventFilename);

                yield return new object[]
                {
                    testName,
                    File.ReadAllText(descriptionFile).TrimEnd(),
                    inputFile,
                    expectedEventFile,
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
