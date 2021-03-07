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
using System.Globalization;
using System.IO;
using System.Linq;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Representation;
using YamlDotNet.Representation.Schemas;
using Events = YamlDotNet.Core.Events;
using Stream = YamlDotNet.Representation.Stream;

namespace YamlDotNet.Test.Spec
{
    public class SchemaTests
    {
        private static readonly string specFixtureDirectory = TestFixtureHelper.GetTestFixtureDirectory("YAMLDOTNET_SCHEMA_SUITE_DIR", "yaml-test-schema");

        public static IEnumerable<object[]> GetFilteredYamlSpecDataSuites(string schemaId)
        {
            return GetYamlSpecDataSuites().Where(s => schemaId.Equals(s[1]));
        }

        public static IEnumerable<object[]> GetYamlSpecDataSuites()
        {
            var fixturePath = Path.Combine(specFixtureDirectory, "yaml-schema.yaml");
            using var fixtureText = File.OpenText(fixturePath);

            var parser = new Parser(fixtureText);

            parser.TryConsume<StreamStart>(out _);
            parser.TryConsume<DocumentStart>(out _);
            parser.Consume<MappingStart>();

            var duplicateDetector = new HashSet<(string, string, string, string, string)>();

            while (!parser.TryConsume<MappingEnd>(out _))
            {
                var inputYaml = parser.Consume<Events.Scalar>();
                parser.Consume<MappingStart>();
                while (!parser.TryConsume<MappingEnd>(out _))
                {
                    var schemaIds = parser.Consume<Events.Scalar>();
                    parser.Consume<SequenceStart>();
                    var type = parser.Consume<Events.Scalar>().Value;
                    var loadedValueText = parser.Consume<Events.Scalar>().Value;
                    var dumpedYaml = parser.Consume<Events.Scalar>().Value;
                    parser.Consume<SequenceEnd>();

                    foreach (var schemaId in schemaIds.Value.Split(','))
                    {
                        var isDuplicate = !duplicateDetector.Add((inputYaml.Value, schemaId.Trim(), type, loadedValueText, dumpedYaml));
                        if (isDuplicate)
                        {
                            throw new Exception($"Duplicate test at line {inputYaml.Start.Line}: {inputYaml.Value}, {schemaId.Trim()}, {type}, {loadedValueText}, {dumpedYaml}");
                        }

                        yield return new object[]
                        {
                            inputYaml.Value,
                            schemaId.Trim(),
                            type,
                            loadedValueText,
                            dumpedYaml,
                            inputYaml.Start.Line
                        };
                    }
                }
            }
        }

        private static readonly Dictionary<string, ISchema> SchemasById = new Dictionary<string, ISchema>
        {
            { "failsafe", FailsafeSchema.Lenient },
            { "json", JsonSchema.Lenient },
            { "core", CoreSchema.Complete },
            { "yaml11", Yaml11Schema.Complete },
        };

        private static readonly Dictionary<string, object?> Functions = new Dictionary<string, object?>
        {
            { "true()", true },
            { "false()", false },
            { "null()", null },
            { "inf()", double.PositiveInfinity },
            { "inf-neg()", double.NegativeInfinity },
            { "nan()", double.NaN },
        };

        [Theory, MemberData(nameof(GetFilteredYamlSpecDataSuites), "failsafe")]
        public void Failsafe(string inputYaml, string schemaId, string type, string loadedValueText, string dumpedYaml, int sourceLineNumber)
        {
            ConformsWithYamlSpec(inputYaml, schemaId, type, loadedValueText, dumpedYaml, sourceLineNumber);
        }

        [Theory, MemberData(nameof(GetFilteredYamlSpecDataSuites), "json")]
        public void Json(string inputYaml, string schemaId, string type, string loadedValueText, string dumpedYaml, int sourceLineNumber)
        {
            ConformsWithYamlSpec(inputYaml, schemaId, type, loadedValueText, dumpedYaml, sourceLineNumber);
        }

        [Theory, MemberData(nameof(GetFilteredYamlSpecDataSuites), "core")]
        public void Core(string inputYaml, string schemaId, string type, string loadedValueText, string dumpedYaml, int sourceLineNumber)
        {
            ConformsWithYamlSpec(inputYaml, schemaId, type, loadedValueText, dumpedYaml, sourceLineNumber);
        }

        [Theory, MemberData(nameof(GetFilteredYamlSpecDataSuites), "yaml11")]
        public void Yaml11(string inputYaml, string schemaId, string type, string loadedValueText, string dumpedYaml, int sourceLineNumber)
        {
            ConformsWithYamlSpec(inputYaml, schemaId, type, loadedValueText, dumpedYaml, sourceLineNumber);
        }

        private void ConformsWithYamlSpec(string inputYaml, string schemaId, string type, string expectedLoadedValueText, string dumpedYaml, int sourceLineNumber)
        {
            if (!SchemasById.TryGetValue(schemaId, out var schema))
            {
                throw new KeyNotFoundException($"Schema '{schemaId}' not found");
            }

            var document = Stream.Load("--- " + inputYaml, _ => schema).Single();

            var actual = (YamlDotNet.Representation.Scalar)document.Content;

            // Check expected tag
            var expectedTag = YamlTagRepository.Prefix + type switch
            {
                "inf" => "float",
                "nan" => "float",
                _ => type
            };
            Assert.Equal(expectedTag, actual.Tag.Value);

            // Check loaded value
            var actualLoadedValue = actual.Mapper.Construct(actual);

            object? expectedLoadedValue;
            if (expectedLoadedValueText.EndsWith("()"))
            {
                if (!Functions.TryGetValue(expectedLoadedValueText, out expectedLoadedValue))
                {
                    throw new KeyNotFoundException($"Function '{expectedLoadedValueText}' not found");
                }
            }
            else if (actual.Tag == YamlTagRepository.Integer)
            {
                if (actualLoadedValue is long)
                {
                    expectedLoadedValue = long.Parse(expectedLoadedValueText, CultureInfo.InvariantCulture);
                }
                else
                {
                    expectedLoadedValue = ulong.Parse(expectedLoadedValueText, CultureInfo.InvariantCulture);
                }
            }
            else if (actual.Tag == YamlTagRepository.FloatingPoint)
            {
                expectedLoadedValue = double.Parse(expectedLoadedValueText, CultureInfo.InvariantCulture);
            }
            else
            {
                expectedLoadedValue = expectedLoadedValueText;
            }

            Assert.Equal(expectedLoadedValue, actualLoadedValue);

            // Check dumped value
            var dumpedScalar = actual.Mapper.RepresentMemorized(actualLoadedValue, schema.Root, new RepresentationState());

            var emittedYaml = Stream.Dump(new[] { new Document(dumpedScalar, schema) }, explicitSeparators: true);

            if (schemaId == "json")
            {
                dumpedYaml = dumpedYaml.Replace('\'', '"');
            }
            Assert.Equal("--- " + dumpedYaml + Environment.NewLine + "..." + Environment.NewLine, emittedYaml);
        }
    }
}

