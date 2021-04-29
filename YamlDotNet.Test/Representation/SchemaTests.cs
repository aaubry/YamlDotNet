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
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using YamlDotNet.Core;
using YamlDotNet.Representation.Schemas;
using Stream = YamlDotNet.Representation.Stream;

namespace YamlDotNet.Test.Representation
{
    public class SchemaTests
    {
        private readonly ITestOutputHelper output;

        public SchemaTests(ITestOutputHelper output)
        {
            this.output = output ?? throw new ArgumentNullException(nameof(output));
        }

        [Theory]
        [InlineData("plain", "!? 'plain'")]
        [InlineData("'single quoted'", "! 'single quoted'")]
        [InlineData("\"double quoted\"", "! 'double quoted'")]

        [InlineData("{ a: b }", "!? { a: b }")]
        [InlineData("[ a, b ]", "!? [ a, b ]")]
        public void ParseWithoutSchemaProducesNonSpecificTags(string input, string expected)
        {
            ParseWithSchemaProducesCorrectTags(NullSchema.Instance, input, expected);
        }

        [Theory]
        [InlineData("null", "!? 'null'")]
        [InlineData("Null", "!? 'Null'")]
        [InlineData("NULL", "!? 'NULL'")]
        [InlineData("~", "!? '~'")]
        [InlineData("---", "!? ''")]

        [InlineData("true", "!? 'true'")]
        [InlineData("True", "!? 'True'")]
        [InlineData("TRUE", "!? 'TRUE'")]
        [InlineData("false", "!? 'false'")]
        [InlineData("False", "!? 'False'")]
        [InlineData("FALSE", "!? 'FALSE'")]

        [InlineData("0", "!? '0'")]
        [InlineData("13", "!? '13'")]
        [InlineData("-6", "!? '-6'")]
        [InlineData("0o10", "!? '8'")]
        [InlineData("0x3A", "!? '58'")]

        [InlineData("0.", "!? '0.'")]
        [InlineData("-0.0", "!? '-0.0'")]
        [InlineData(".5", "!? '.5'")]
        [InlineData("+12e03", "!? '+12e03'")]
        [InlineData("-2E+05", "!? '-2E+05'")]
        [InlineData(".inf", "!? '.inf'")]
        [InlineData("-.Inf", "!? '-.Inf'")]
        [InlineData("+.INF", "!? '+.inf'")]
        [InlineData(".nan", "!? '.nan'")]

        [InlineData("'non-plain'", "!!str 'non-plain'")]

        [InlineData("{ a: b }", "!!map { a: b }")]
        [InlineData("! { a: b }", "!!map { a: b }")]
        [InlineData("[ a, b ]", "!!seq [ a, b ]")]
        [InlineData("! [ a, b ]", "!!seq [ a, b ]")]
        public void ParseWithFailsafeSchemaProducesCorrectTags(string input, string expected)
        {
            ParseWithSchemaProducesCorrectTags(FailsafeSchema.Strict, input, expected);
        }

        [Theory]
        [InlineData("null", "!!null")]
        [InlineData("! null", "!!str 'null'")]

        [InlineData("true", "!!bool true")]
        [InlineData("false", "!!bool false")]

        [InlineData("0", "!!int 0")]
        [InlineData("13", "!!int 13")]
        [InlineData("-6", "!!int -6")]

        [InlineData("0.", "!!float 0.")]
        [InlineData("-0.0", "!!float -0.0")]
        [InlineData("0.5", "!!float 0.5")]
        [InlineData("12e03", "!!float 12e03")]
        [InlineData("-2E+05", "!!float -2E+05")]

        [InlineData("{ 'a': 'b' }", "!!map { 'a': 'b' }")]
        [InlineData("! { 'a': 'b' }", "!!map { 'a': 'b' }")]
        [InlineData("[ 'a', 'b' ]", "!!seq [ 'a', 'b' ]")]
        [InlineData("! [ 'a', 'b' ]", "!!seq [ 'a', 'b' ]")]
        public void ParseWithJsonSchemaProducesCorrectTags(string input, string expected)
        {
            ParseWithSchemaProducesCorrectTags(JsonSchema.Strict, input, expected);
        }

        [Theory]
        [InlineData("null", "!!null")]
        [InlineData("Null", "!!null")]
        [InlineData("NULL", "!!null")]
        [InlineData("~", "!!null")]
        [InlineData("---", "!!null")]

        [InlineData("true", "!!bool true")]
        [InlineData("True", "!!bool true")]
        [InlineData("TRUE", "!!bool true")]
        [InlineData("false", "!!bool false")]
        [InlineData("False", "!!bool false")]
        [InlineData("FALSE", "!!bool false")]

        [InlineData("0", "!!int 0")]
        [InlineData("13", "!!int 13")]
        [InlineData("-6", "!!int -6")]
        [InlineData("0o10", "!!int 8")]
        [InlineData("0x3A", "!!int 58")]

        [InlineData("0.", "!!float 0")]
        [InlineData("-0.0", "!!float 0")]
        [InlineData(".5", "!!float 0.5")]
        [InlineData("+12e03", "!!float 12000")]
        [InlineData("-2E+05", "!!float -200000")]
        [InlineData(".inf", "!!float .inf")]
        [InlineData("-.Inf", "!!float -.Inf")]
        [InlineData("+.INF", "!!float +.inf")]
        [InlineData(".nan", "!!float .nan")]
        public void ParseWithCoreSchemaProducesCorrectTags(string input, string expected)
        {
            ParseWithSchemaProducesCorrectTags(CoreSchema.Complete, input, expected);
        }

        private void ParseWithSchemaProducesCorrectTags(ISchema schema, string input, string expected)
        {
            var actualNode = Stream.Load(Yaml.ParserForText(input), _ => schema).Single().Content;
            var expectedNode = Stream.Load(Yaml.ParserForText(expected), _ => schema).Single().Content;

            // Since we can't specify the '?' tag, we'll use '!?' and translate here
            var expectedTag = expectedNode.Tag;
            if (expectedTag.Value == "!?")
            {
                expectedTag = TagName.Empty;
            }

            Assert.Equal(expectedTag, actualNode.Tag);
        }
    }
}

