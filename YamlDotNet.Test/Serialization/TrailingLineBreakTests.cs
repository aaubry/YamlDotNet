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

using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test.Serialization
{
    public class TrailingLineBreakTests
    {
        private static readonly string[][] Cases =
        {
            new[] { "asdf", "asdf", "-" },
            new[] { "asdf\n", "asdf\n", "" },
            new[] { "asdf\r", "asdf\n", "" },
            new[] { "asdf\r\n", "asdf\n", "" },
            new[] { "asdf\n\n", "asdf\n\n", "+" },
            new[] { "asdf\r\r", "asdf\n\n", "+" },
            new[] { "asdf\r\n\r\n", "asdf\n\n", "+" },
            new[] { "asdf\n\r\n", "asdf\n\n", "+" },
            new[] { "asdf\r\n\n", "asdf\n\n", "+" },
            new[] { "asdf\r\r\n", "asdf\n\n", "+" },
            new[] { "\n", "\n", "+" },
            new[] { "\r", "\n", "+" },
            new[] { "\r\n", "\n", "+" },
            new[] { "\r\n\r\n", "\n\n", "+" }
        };

        public static IEnumerable<object[]> BlockScalarCases()
        {
            foreach (var style in new[] { ScalarStyle.Literal, ScalarStyle.Folded })
            {
                foreach (var item in Cases)
                {
                    yield return new object[] { style, item[0], item[1], item[2] };
                }
            }
        }

        [Theory]
        [MemberData(nameof(BlockScalarCases))]
        public void BlockScalarChompingPreservesValuesAndCanonicalOutput(
            ScalarStyle style, string input, string expected, string hint)
        {
            var serializer = new SerializerBuilder()
                .WithNewLine("\n")
                .WithDefaultScalarStyle(style)
                .Build();
            var yaml = serializer.Serialize(input);
            var actual = new DeserializerBuilder().Build().Deserialize<string>(yaml);

            actual.Should().Be(expected);
            serializer.Serialize(actual).Should().Be(yaml);

            var header = yaml.Split('\n')[0];
            if (hint.Length == 0)
            {
                header.Should().NotContain("+").And.NotContain("-");
            }
            else
            {
                header.Should().EndWith(hint);
            }
        }

        public static IEnumerable<object[]> DoubleQuotedCases()
        {
            foreach (var item in Cases)
            {
                yield return new object[] { item[0] };
            }
        }

        [Theory]
        [MemberData(nameof(DoubleQuotedCases))]
        public void DoubleQuotedScalarsKeepTheirOriginalLineBreaks(string input)
        {
            var serializer = new SerializerBuilder()
                .WithNewLine("\n")
                .WithDefaultScalarStyle(ScalarStyle.DoubleQuoted)
                .Build();
            var yaml = serializer.Serialize(input);
            var actual = new DeserializerBuilder().Build().Deserialize<string>(yaml);

            actual.Should().Be(input);
            serializer.Serialize(actual).Should().Be(yaml);
        }

        [Theory]
        [InlineData("asdf", "asdf")]
        [InlineData("asdf\n", "asdf\n")]
        [InlineData("asdf\r\n", "asdf\n")]
        public void SingleQuotedControlsRetainTheirExistingCanonicalOutput(string input, string expected)
        {
            var serializer = new SerializerBuilder()
                .WithNewLine("\n")
                .WithDefaultScalarStyle(ScalarStyle.SingleQuoted)
                .Build();
            var yaml = serializer.Serialize(input);
            var actual = new DeserializerBuilder().Build().Deserialize<string>(yaml);

            actual.Should().Be(expected);
            serializer.Serialize(actual).Should().Be(yaml);
        }

        [Fact]
        public void DefaultMappingWithOneCrLfHasStableChomping()
        {
            var serializer = new SerializerBuilder().WithNewLine("\n").Build();
            var yaml = serializer.Serialize(new { c = "asdf\r\n" });
            var actual = new DeserializerBuilder().Build().Deserialize<Dictionary<string, string>>(yaml);

            actual["c"].Should().Be("asdf\n");
            yaml.Should().NotContain(">+");
            serializer.Serialize(actual).Should().Be(yaml);
        }
    }
}
