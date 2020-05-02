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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Core.Schemas;

namespace YamlDotNet.Test.Core
{
    public class SchemaTests : EventsHelper
    {
        private readonly ITestOutputHelper output;

        public SchemaTests(ITestOutputHelper output)
        {
            this.output = output ?? throw new ArgumentNullException(nameof(output));
        }

        [Fact]
        public void ParseWithoutSchemaProducesNonSpecificTags()
        {
            AssertParseWithSchemaProducesCorrectTags(
                new NullSchema(),
                @"
                    - { actual: plain, expected: !? 'plain' }
                    - { actual: 'single quoted', expected: ! 'single quoted' }
                    - { actual: ""double quoted"", expected: ! 'double quoted' }

                    - { actual: { a: b }, expected: !? { a: b } }
                    - { actual: [ a, b ], expected: !? [ a, b ] }
                "
            );
        }

        [Fact]
        public void ParseWithFailsafeSchemaProducesCorrectTags()
        {
            AssertParseWithSchemaProducesCorrectTags(
                FailsafeSchema.Strict,
                @"
                    - { actual: null, expected: !? 'null' }
                    - { actual: Null, expected: !? 'Null' }
                    - { actual: NULL, expected: !? 'NULL' }
                    - { actual: ~, expected: !? '~' }
                    - { actual: , expected: !? '' }

                    - { actual: true, expected: !? 'true' }
                    - { actual: True, expected: !? 'True' }
                    - { actual: TRUE, expected: !? 'TRUE' }
                    - { actual: false, expected: !? 'false' }
                    - { actual: False, expected: !? 'False' }
                    - { actual: FALSE, expected: !? 'FALSE' }

                    - { actual: 0, expected: !? '0' }
                    - { actual: 13, expected: !? '13' }
                    - { actual: -6, expected: !? '-6' }
                    - { actual: 0o10, expected: !? '8' }
                    - { actual: 0x3A, expected: !? '58' }

                    - { actual: 0., expected: !? '0.' }
                    - { actual: -0.0, expected: !? '-0.0' }
                    - { actual: .5, expected: !? '.5' }
                    - { actual: +12e03, expected: !? '+12e03' }
                    - { actual: -2E+05, expected: !? '-2E+05' }
                    - { actual: .inf, expected: !? '.inf' }
                    - { actual: -.Inf, expected: !? '-.Inf' }
                    - { actual: +.INF, expected: !? '+.inf' }
                    - { actual: .nan, expected: !? '.nan' }

                    - { actual: 'non-plain', expected: !!str 'non-plain' }

                    - { actual: { a: b }, expected: !? { a: b } }
                    - { actual: ! { a: b }, expected: !!map { a: b } }
                    - { actual: [ a, b ], expected: !? [ a, b ] }
                    - { actual: ! [ a, b ], expected: !!seq [ a, b ] }
                "
            );
        }

        [Fact]
        public void ParseWithJsonSchemaProducesCorrectTags()
        {
            AssertParseWithSchemaProducesCorrectTags(
                JsonSchema.Strict,
                @"
                    - { 'actual': null, 'expected': !!null }
                    - { 'actual': ! null, 'expected': !!str 'null' }

                    - { 'actual': true, 'expected': !!bool true }
                    - { 'actual': false, 'expected': !!bool false }

                    - { 'actual': 0, 'expected': !!int 0 }
                    - { 'actual': 13, 'expected': !!int 13 }
                    - { 'actual': -6, 'expected': !!int -6 }

                    - { 'actual': 0., 'expected': !!float 0. }
                    - { 'actual': -0.0, 'expected': !!float -0.0 }
                    - { 'actual': 0.5, 'expected': !!float 0.5 }
                    - { 'actual': 12e03, 'expected': !!float 12e03 }
                    - { 'actual': -2E+05, 'expected': !!float -2E+05 }

                    - { 'actual': { 'a': 'b' }, 'expected': !!map { 'a': 'b' } }
                    - { 'actual': ! { 'a': 'b' }, 'expected': !!map { 'a': 'b' } }
                    - { 'actual': [ 'a', 'b' ], 'expected': !!seq [ 'a', 'b' ] }
                    - { 'actual': ! [ 'a', 'b' ], 'expected': !!seq [ 'a', 'b' ] }
                "
            );
        }

        [Fact]
        public void ParseWithCoreSchemaProducesCorrectTags()
        {
            AssertParseWithSchemaProducesCorrectTags(
                CoreSchema.Instance,
                @"
                    - { actual: null, expected: !!null }
                    - { actual: Null, expected: !!null }
                    - { actual: NULL, expected: !!null }
                    - { actual: ~, expected: !!null }
                    - { actual: , expected: !!null }

                    - { actual: true, expected: !!bool true }
                    - { actual: True, expected: !!bool true }
                    - { actual: TRUE, expected: !!bool true }
                    - { actual: false, expected: !!bool false }
                    - { actual: False, expected: !!bool false }
                    - { actual: FALSE, expected: !!bool false }

                    - { actual: 0, expected: !!int 0 }
                    - { actual: 13, expected: !!int 13 }
                    - { actual: -6, expected: !!int -6 }
                    - { actual: 0o10, expected: !!int 8 }
                    - { actual: 0x3A, expected: !!int 58 }

                    - { actual: 0., expected: !!float 0 }
                    - { actual: -0.0, expected: !!float 0 }
                    - { actual: .5, expected: !!float 0.5 }
                    - { actual: +12e03, expected: !!float 12000 }
                    - { actual: -2E+05, expected: !!float -200000 }
                    - { actual: .inf, expected: !!float .inf }
                    - { actual: -.Inf, expected: !!float -.Inf }
                    - { actual: +.INF, expected: !!float +.inf }
                    - { actual: .nan, expected: !!float .nan }
                "
            );
        }

        private IParser ParserForText(string yamlText)
        {
            var lines = yamlText
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
                var indent = Regex.Match(lines[0], @"^(\s+)");
                if (!indent.Success)
                {
                    throw new ArgumentException("Invalid indentation");
                }

                lines = lines
                    .Select(l => l.Substring(Math.Min(indent.Groups[1].Length, l.Length)))
                    .ToList();
            }

            var reader = new StringReader(string.Join("\n", lines.ToArray()));
            return new Parser(reader);
        }

        private void AssertParseWithSchemaProducesCorrectTags(ISchema schema, string yaml)
        {
            var parser = new SchemaAwareParser(
                ParserForText(yaml),
                schema
            );

            parser.TryConsume<StreamStart>(out _);
            parser.TryConsume<DocumentStart>(out _);
            parser.TryConsume<SequenceStart>(out _);

            while (!parser.TryConsume<SequenceEnd>(out _))
            {
                parser.Consume<MappingStart>();

                NodeEvent? expected = null;
                NodeEvent? actual = null;
                for (int i = 0; i < 2; i++)
                {
                    var key = parser.Consume<Scalar>();
                    switch (key.Value)
                    {
                        case nameof(expected):
                            expected = ConsumeObject(parser);
                            break;

                        case nameof(actual):
                            actual = ConsumeObject(parser);
                            break;

                        default:
                            throw new ApplicationException($"Invalid test data key: '{key.Value}'");
                    }
                }

                if (expected == null || actual == null)
                {
                    throw new ApplicationException("Invalid test data");
                }

                output.WriteLine("actual: {0}\nexpected: {1}\n\n", actual, expected);

                // Since we can't specify the '?' tag, we'll use '!?' and translate here
                var expectedTag = expected.Tag.Name;
                if (expectedTag.Value == "!?")
                {
                    expectedTag = TagName.Empty;
                }

                Assert.Equal(expectedTag, actual.Tag.Name);

                parser.Consume<MappingEnd>();
            }
        }

        private NodeEvent ConsumeObject(IParser parser)
        {
            if (parser.TryConsume<Scalar>(out var scalar))
            {
                return scalar;
            }

            if (parser.Accept<MappingStart>(out var mapping))
            {
                parser.SkipThisAndNestedEvents();
                return mapping;
            }

            if (parser.Accept<SequenceStart>(out var sequence))
            {
                parser.SkipThisAndNestedEvents();
                return sequence;
            }

            parser.Accept<ParsingEvent>(out var parsingEvent);
            throw new InvalidOperationException(
                string.Format(
                    "Invalid node type {0} at {1}",
                    parsingEvent!.GetType().Name,
                    parsingEvent!.Start
                )
            );
        }

        private sealed class NullSchema : ISchema
        {
            public bool ResolveNonSpecificTag(Scalar node, IEnumerable<NodeEvent> path, [NotNullWhen(true)] out ITag? resolvedTag)
            {
                resolvedTag = null;
                return false;
            }

            public bool ResolveNonSpecificTag(MappingStart node, IEnumerable<NodeEvent> path, [NotNullWhen(true)] out ITag? resolvedTag)
            {
                resolvedTag = null;
                return false;
            }

            public bool ResolveNonSpecificTag(SequenceStart node, IEnumerable<NodeEvent> path, [NotNullWhen(true)] out ITag? resolvedTag)
            {
                resolvedTag = null;
                return false;
            }

            public bool ResolveSpecificTag(TagName tag, [NotNullWhen(true)] out ITag? resolvedTag)
            {
                resolvedTag = null;
                return false;
            }
        }
    }
}

