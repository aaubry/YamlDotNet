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
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Core.Tokens;

namespace YamlDotNet.Test.Core
{
    public class ScannerTests : TokenHelper
    {
        [Fact]
        public void VerifyTokensOnExample1()
        {
            AssertSequenceOfTokensFrom(Yaml.ScannerForResource("01-directives.yaml"),
                StreamStart,
                VersionDirective(1, 1),
                TagDirective("!", "!foo"),
                TagDirective("!yaml!", "tag:yaml.org,2002:"),
                DocumentStart,
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample2()
        {
            AssertSequenceOfTokensFrom(Yaml.ScannerForResource("02-scalar-in-imp-doc.yaml"),
                StreamStart,
                SingleQuotedScalar("a scalar"),
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample3()
        {
            var scanner = Yaml.ScannerForResource("03-scalar-in-exp-doc.yaml");
            AssertSequenceOfTokensFrom(scanner,
                StreamStart,
                DocumentStart,
                SingleQuotedScalar("a scalar"),
                DocumentEnd,
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample4()
        {
            AssertSequenceOfTokensFrom(Yaml.ScannerForResource("04-scalars-in-multi-docs.yaml"),
                StreamStart,
                SingleQuotedScalar("a scalar"),
                DocumentStart,
                SingleQuotedScalar("another scalar"),
                DocumentStart,
                SingleQuotedScalar("yet another scalar"),
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample5()
        {
            AssertSequenceOfTokensFrom(Yaml.ScannerForResource("05-circular-sequence.yaml"),
                StreamStart,
                Anchor("A"),
                FlowSequenceStart,
                AnchorAlias("A"),
                FlowSequenceEnd,
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample6()
        {
            AssertSequenceOfTokensFrom(Yaml.ScannerForResource("06-float-tag.yaml"),
                StreamStart,
                Tag("!!", "float"),
                DoubleQuotedScalar("3.14"),
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample7()
        {
            AssertSequenceOfTokensFrom(Yaml.ScannerForResource("07-scalar-styles.yaml"),
                StreamStart,
                DocumentStart,
                DocumentStart,
                PlainScalar("a plain scalar"),
                DocumentStart,
                SingleQuotedScalar("a single-quoted scalar"),
                DocumentStart,
                DoubleQuotedScalar("a double-quoted scalar"),
                DocumentStart,
                LiteralScalar("a literal scalar"),
                DocumentStart,
                FoldedScalar("a folded scalar"),
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample8()
        {
            AssertSequenceOfTokensFrom(Yaml.ScannerForResource("08-flow-sequence.yaml"),
                StreamStart,
                FlowSequenceStart,
                PlainScalar("item 1"),
                FlowEntry,
                PlainScalar("item 2"),
                FlowEntry,
                PlainScalar("item 3"),
                FlowSequenceEnd,
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample9()
        {
            AssertSequenceOfTokensFrom(Yaml.ScannerForResource("09-flow-mapping.yaml"),
                StreamStart,
                FlowMappingStart,
                Key,
                PlainScalar("a simple key"),
                Value,
                PlainScalar("a value"),
                FlowEntry,
                Key,
                PlainScalar("a complex key"),
                Value,
                PlainScalar("another value"),
                FlowEntry,
                FlowMappingEnd,
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample10()
        {
            AssertSequenceOfTokensFrom(Yaml.ScannerForResource("10-mixed-nodes-in-sequence.yaml"),
                StreamStart,
                BlockSequenceStart,
                BlockEntry,
                PlainScalar("item 1"),
                BlockEntry,
                PlainScalar("item 2"),
                BlockEntry,
                BlockSequenceStart,
                BlockEntry,
                PlainScalar("item 3.1"),
                BlockEntry,
                PlainScalar("item 3.2"),
                BlockEnd,
                BlockEntry,
                BlockMappingStart,
                Key,
                PlainScalar("key 1"),
                Value,
                PlainScalar("value 1"),
                Key,
                PlainScalar("key 2"),
                Value,
                PlainScalar("value 2"),
                BlockEnd,
                BlockEnd,
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample11()
        {
            AssertSequenceOfTokensFrom(Yaml.ScannerForResource("11-mixed-nodes-in-mapping.yaml"),
                StreamStart,
                BlockMappingStart,
                Key,
                PlainScalar("a simple key"),
                Value,
                PlainScalar("a value"),
                Key,
                PlainScalar("a complex key"),
                Value,
                PlainScalar("another value"),
                Key,
                PlainScalar("a mapping"),
                Value,
                BlockMappingStart,
                Key,
                PlainScalar("key 1"),
                Value,
                PlainScalar("value 1"),
                Key,
                PlainScalar("key 2"),
                Value,
                PlainScalar("value 2"),
                BlockEnd,
                Key,
                PlainScalar("a sequence"),
                Value,
                BlockSequenceStart,
                BlockEntry,
                PlainScalar("item 1"),
                BlockEntry,
                PlainScalar("item 2"),
                BlockEnd,
                BlockEnd,
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample12()
        {
            AssertSequenceOfTokensFrom(Yaml.ScannerForResource("12-compact-sequence.yaml"),
                StreamStart,
                BlockSequenceStart,
                BlockEntry,
                BlockSequenceStart,
                BlockEntry,
                PlainScalar("item 1"),
                BlockEntry,
                PlainScalar("item 2"),
                BlockEnd,
                BlockEntry,
                BlockMappingStart,
                Key,
                PlainScalar("key 1"),
                Value,
                PlainScalar("value 1"),
                Key,
                PlainScalar("key 2"),
                Value,
                PlainScalar("value 2"),
                BlockEnd,
                BlockEntry,
                BlockMappingStart,
                Key,
                PlainScalar("complex key"),
                Value,
                PlainScalar("complex value"),
                BlockEnd,
                BlockEnd,
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample13()
        {
            AssertSequenceOfTokensFrom(Yaml.ScannerForResource("13-compact-mapping.yaml"),
                StreamStart,
                BlockMappingStart,
                Key,
                PlainScalar("a sequence"),
                Value,
                BlockSequenceStart,
                BlockEntry,
                PlainScalar("item 1"),
                BlockEntry,
                PlainScalar("item 2"),
                BlockEnd,
                Key,
                PlainScalar("a mapping"),
                Value,
                BlockMappingStart,
                Key,
                PlainScalar("key 1"),
                Value,
                PlainScalar("value 1"),
                Key,
                PlainScalar("key 2"),
                Value,
                PlainScalar("value 2"),
                BlockEnd,
                BlockEnd,
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample14()
        {
            AssertSequenceOfTokensFrom(Yaml.ScannerForResource("14-mapping-wo-indent.yaml"),
                StreamStart,
                BlockMappingStart,
                Key,
                PlainScalar("key"),
                Value,
                BlockEntry,
                PlainScalar("item 1"),
                BlockEntry,
                PlainScalar("item 2"),
                BlockEnd,
                StreamEnd);
        }

        [Fact]
        public void SupportRealyLongStrings()
        {
            var longKey = string.Concat(Enumerable.Repeat("x", 1500));
            var yamlString = $"{longKey}: value";
            var scanner = new Scanner(new StringReader(yamlString), true, 1500);
            AssertSequenceOfTokensFrom(scanner,
                StreamStart,
                BlockMappingStart,
                Key,
                PlainScalar(longKey),
                Value,
                PlainScalar("value"),
                BlockEnd,
                StreamEnd);
        }

        [Fact]
        public void CommentsAreReturnedWhenRequested()
        {
            AssertSequenceOfTokensFrom(new Scanner(Yaml.ReaderForText(@"
                    # Top comment
                    - first # Comment on first item
                    - second
                    # Bottom comment
                "), skipComments: false),
                StreamStart,
                StandaloneComment("Top comment"),
                BlockSequenceStart,
                BlockEntry,
                PlainScalar("first"),
                InlineComment("Comment on first item"),
                BlockEntry,
                PlainScalar("second"),
                StandaloneComment("Bottom comment"),
                BlockEnd,
                StreamEnd);
        }

        [Fact]
        public void CommentsAreCorrectlyMarked()
        {
            var sut = new Scanner(Yaml.ReaderForText(@"
                - first # Comment on first item
            "), skipComments: false);

            while (sut.MoveNext())
            {
                if (sut.Current is Comment comment)
                {
                    Assert.Equal(8, comment.Start.Index);
                    Assert.Equal(31, comment.End.Index);

                    return;
                }
            }

            Assert.Fail("Did not find a comment");
        }

        [Fact]
        public void CommentsAreOmittedUnlessRequested()
        {
            AssertSequenceOfTokensFrom(Yaml.ScannerForText(@"
                    # Top comment
                    - first # Comment on first item
                    - second
                    # Bottom comment
                "),
                StreamStart,
                BlockSequenceStart,
                BlockEntry,
                PlainScalar("first"),
                BlockEntry,
                PlainScalar("second"),
                BlockEnd,
                StreamEnd);
        }

        [Fact]
        public void MarksOnDoubleQuotedScalarsAreCorrect()
        {
            var scanner = Yaml.ScannerForText(@"
                ""x""
            ");

            Scalar scalar = null;
            while (scanner.MoveNext() && scalar == null)
            {
                scalar = scanner.Current as Scalar;
            }
            Assert.Equal(4, scalar.End.Column);
        }

        [Fact]
        public void Slow_stream_is_parsed_correctly()
        {
            var buffer = new MemoryStream();
            Yaml.StreamFrom("04-scalars-in-multi-docs.yaml").CopyTo(buffer);

            var slowStream = new SlowStream(buffer.ToArray());

            var scanner = new Scanner(new StreamReader(slowStream));

            scanner.MoveNext();

            // Should not fail
            scanner.MoveNext();
        }

        [Fact]
        public void Issue_553_562()
        {
            var yaml = "MainItem4:\n" + string.Join("\n", Enumerable.Range(1, 100).Select(e => $"- {{item: {{foo1: {e}, foo2: 'bar{e}' }}}}"));

            var scanner = new Scanner(new StringReader(yaml));
            while (scanner.MoveNext())
            {
            }
        }

        [Fact]
        public void Plain_Scalar_outside_of_flow_allows_braces()
        {
            AssertSequenceOfTokensFrom(Yaml.ScannerForText(@"value: -[123]"),
                StreamStart,
                BlockMappingStart,
                Key,
                PlainScalar("value"),
                Value,
                PlainScalar("-[123]"),
                BlockEnd,
                StreamEnd);
        }

        [Fact]
        public void Plain_Scalar_inside_flow_does_not_allow_braces()
        {
            AssertPartialSequenceOfTokensFrom(Yaml.ScannerForText(@"
[
  value: -[123]
]"),
                StreamStart,
                FlowSequenceStart,
                Key,
                PlainScalar("value"),
                Value,
                Error("Invalid key indicator format."));
        }

        [Fact]
        public void Keys_can_start_with_colons_in_nested_block()
        {
            AssertSequenceOfTokensFrom(Yaml.ScannerForText("root:\n  :first: 1\n  :second: 2"),
                StreamStart,
                BlockMappingStart,
                Key,
                PlainScalar("root"),
                Value,
                BlockMappingStart,
                Key,
                PlainScalar(":first"),
                Value,
                PlainScalar("1"),
                Key,
                PlainScalar(":second"),
                Value,
                PlainScalar("2"),
                BlockEnd,
                BlockEnd,
                StreamEnd);
        }

        [Fact]
        public void Keys_can_start_with_colons_after_quoted_values()
        {
            AssertSequenceOfTokensFrom(Yaml.ScannerForText(":first: '1'\n:second: 2"),
                StreamStart,
                BlockMappingStart,
                Key,
                PlainScalar(":first"),
                Value,
                SingleQuotedScalar("1"),
                Key,
                PlainScalar(":second"),
                Value,
                PlainScalar("2"),
                BlockEnd,
                StreamEnd);
        }

        [Fact]
        public void Keys_can_start_with_colons_after_single_quoted_values_in_nested_block()
        {
            AssertSequenceOfTokensFrom(Yaml.ScannerForText("xyz:\n  :hello: 'world'\n  :goodbye: world"),
                StreamStart,
                BlockMappingStart,
                Key,
                PlainScalar("xyz"),
                Value,
                BlockMappingStart,
                Key,
                PlainScalar(":hello"),
                Value,
                SingleQuotedScalar("world"),
                Key,
                PlainScalar(":goodbye"),
                Value,
                PlainScalar("world"),
                BlockEnd,
                BlockEnd,
                StreamEnd);
        }

        [Fact]
        public void Keys_can_start_with_colons_after_double_quoted_values_in_nested_block()
        {
            AssertSequenceOfTokensFrom(Yaml.ScannerForText("xyz:\n  :hello: \"world\"\n  :goodbye: world"),
                 StreamStart,
                 BlockMappingStart,
                 Key,
                 PlainScalar("xyz"),
                 Value,
                 BlockMappingStart,
                 Key,
                 PlainScalar(":hello"),
                 Value,
                 DoubleQuotedScalar("world"),
                 Key,
                 PlainScalar(":goodbye"),
                 Value,
                 PlainScalar("world"),
                 BlockEnd,
                 BlockEnd,
                 StreamEnd);
        }

        [Fact]
        public void Utf16StringsAsUtf8SurrogatesWorkCorrectly()
        {
            AssertSequenceOfTokensFrom(Yaml.ScannerForText("Test: \"\\uD83D\\uDC4D\""),
                StreamStart,
                BlockMappingStart,
                Key,
                PlainScalar("Test"),
                Value,
                DoubleQuotedScalar("\uD83D\uDC4D"), // guaranteed thumbs up emoticon that will work in Windows Terminal since it pukes on displaying it.
                BlockEnd,
                StreamEnd);
        }

        [Fact]
        public void Utf16CharactersAreReadCorrectly()
        {
            AssertSequenceOfTokensFrom(Yaml.ScannerForText("Test: \"\uD83D\uDC4D\""),
                StreamStart,
                BlockMappingStart,
                Key,
                PlainScalar("Test"),
                Value,
                DoubleQuotedScalar("\uD83D\uDC4D"), // guaranteed thumbs up emoticon that will work in Windows Terminal since it pukes on displaying it.
                BlockEnd,
                StreamEnd);
        }

        // https://github.com/aaubry/YamlDotNet/issues/523
        // https://github.com/aaubry/YamlDotNet/issues/535
        // The scanner is fed the raw text: Yaml.ScannerForText trims trailing spaces, which are what is tested here.
        // Leading empty lines of a block scalar may contain spaces, as long as they
        // do not contain more spaces than the first non-empty line (YAML 1.2.2, 8.1.1.1).
        [Theory]
        [InlineData("|\n  \n  text\n", "\ntext\n")]
        [InlineData("|\n \n  text\n", "\ntext\n")]
        [InlineData("key: |+\n      \n      text\n", "\ntext\n")]
        [InlineData("a:\n  b: |\n    \n    text\n", "\ntext\n")]
        [InlineData("- |\n    \n    text\n", "\ntext\n")]
        [InlineData("key: |\n \n  \n  text\n", "\n\ntext\n")]
        [InlineData("key: >\n  \n  text\n", "\ntext\n")]
        [InlineData("key: |-4\n        \n    text\n", "    \ntext")]
        [InlineData("a:\n- b: |\n    \n    \n  c: d\n", "")]
        [InlineData("a:\n  b: |\n     \n     \n  c: d\n", "")]
        [InlineData("a:\n  b: |\n  c: d\n", "")]
        [InlineData("a:\n  b: >\n\n  c: d\n", "")]
        [InlineData("- example: |+\n    \n- next\n", "\n")]
        [InlineData("|\n\ntext\n", "\ntext\n")]
        [InlineData("a:\n  description: >\n    \n  operationId: x\n", "")]
        public void Block_scalar_allows_leading_empty_lines_with_spaces(string yaml, string expected)
        {
            var scanner = new Scanner(new StringReader(yaml));
            var values = new System.Collections.Generic.List<string>();
            while (scanner.MoveNext())
            {
                if (scanner.Current is Scalar scalar && (scalar.Style == ScalarStyle.Literal || scalar.Style == ScalarStyle.Folded))
                {
                    values.Add(scalar.Value);
                }
            }

            values.Should().Equal(expected);
        }

        [Theory]
        [InlineData("|\n   \n  text\n")]
        [InlineData("key: |\n  \n    \n  text\n")]
        [InlineData("key: >\n    \n  text\n")]
        [InlineData("|\n   \ntext\n")]
        [InlineData(">\n   \ntext\n")]
        [InlineData("---\n|\n   \ntext\n---\nnext\n")]
        public void Block_scalar_rejects_leading_empty_line_with_more_spaces_than_content(string yaml)
        {
            var scanner = new Scanner(new StringReader(yaml));

            Action act = () =>
            {
                while (scanner.MoveNext())
                {
                }
            };

            act.Should().Throw<SemanticErrorException>();
        }

        [Theory]
        [InlineData("key: |\n    \n  text\n", 2)]
        [InlineData("key: |\n \n    \n  \n  text\n", 3)]
        [InlineData("---\n|\n   \ntext\n", 3)]
        public void Block_scalar_error_points_at_the_over_indented_empty_line(string yaml, int expectedLine)
        {
            var scanner = new Scanner(new StringReader(yaml));

            Action act = () =>
            {
                while (scanner.MoveNext())
                {
                }
            };

            act.Should().Throw<SemanticErrorException>()
                .Which.Start.Line.Should().Be(expectedLine);
        }

        // At the root of a document the content of a block scalar may start at column 0. Its
        // indentation is then 0, so spaces at the start of later lines are content, and a line
        // made of spaces between content lines is not a leading empty line.
        [Theory]
        [InlineData("--- |\ntext\n   \nmore\n", "text\n   \nmore\n")]
        [InlineData("--- >\ntext\n   \nmore\n", "text\n   \nmore\n")]
        [InlineData("|\ntext\n   \nmore\n", "text\n   \nmore\n")]
        [InlineData("--- |\r\ntext\r\n   \r\nmore\r\n", "text\n   \nmore\n")]
        [InlineData("--- |\ntext\n  more\n", "text\n  more\n")]
        [InlineData("--- >\ntext\n  more\n", "text\n  more\n")]
        [InlineData("--- |+\ntext\n   \n", "text\n   \n")]
        [InlineData("|2\n    \n  text\n", "  \ntext\n")]
        public void Block_scalar_at_root_keeps_spaces_when_content_starts_at_column_0(string yaml, string expected)
        {
            var scanner = new Scanner(new StringReader(yaml));
            var values = new System.Collections.Generic.List<string>();
            while (scanner.MoveNext())
            {
                if (scanner.Current is Scalar scalar && (scalar.Style == ScalarStyle.Literal || scalar.Style == ScalarStyle.Folded))
                {
                    values.Add(scalar.Value);
                }
            }

            values.Should().Equal(expected);
        }

        // A document marker at column 0 ends the content of a block scalar at the root of a
        // document, even though content at the root may otherwise start at column 0.
        [Theory]
        [InlineData("--- |\ntext\n---\nnext\n", new[] { "---", "text\n", "---", "next" })]
        [InlineData("---\n|\n\ntext\n---\nnext\n", new[] { "---", "\ntext\n", "---", "next" })]
        [InlineData("|\n   \n---\nnext\n", new[] { "", "---", "next" })]
        public void Block_scalar_at_root_ends_at_document_marker(string yaml, string[] expected)
        {
            var scanner = new Scanner(new StringReader(yaml));
            var actual = new System.Collections.Generic.List<string>();
            while (scanner.MoveNext())
            {
                if (scanner.Current is DocumentStart)
                {
                    actual.Add("---");
                }
                else if (scanner.Current is Scalar scalar)
                {
                    actual.Add(scalar.Value);
                }
            }

            actual.Should().Equal(expected);
        }

        private void AssertPartialSequenceOfTokensFrom(Scanner scanner, params Token[] tokens)
        {
            var tokenNumber = 1;
            foreach (var expected in tokens)
            {
                scanner.MoveNext().Should().BeTrue("Missing token number {0}", tokenNumber);
                AssertToken(expected, scanner.Current, tokenNumber);
                tokenNumber++;
            }
        }

        private void AssertSequenceOfTokensFrom(Scanner scanner, params Token[] tokens)
        {
            AssertPartialSequenceOfTokensFrom(scanner, tokens);
            scanner.MoveNext().Should().BeFalse("Found extra tokens");
        }

        private void AssertToken(Token expected, Token actual, int tokenNumber)
        {
            actual.Should().NotBeNull();
            actual.GetType().Should().Be(expected.GetType(), "Token {0} is not of the expected type", tokenNumber);

            foreach (var property in expected.GetType().GetTypeInfo().GetProperties())
            {
                if (property.PropertyType != typeof(Mark) && property.CanRead)
                {
                    var value = property.GetValue(actual, null);
                    var expectedValue = property.GetValue(expected, null);
                    value.Should().Be(expectedValue, "Comparing property {0} in token {1}", property.Name, tokenNumber);
                }
            }
        }

        /// <summary>
        /// A stream that reads one byte at the time.
        /// </summary>
        public class SlowStream : Stream
        {
            private readonly byte[] data;
            private int position;

            public SlowStream(byte[] data)
            {
                this.data = data;
            }

            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => false;

            public override long Length => data.Length;

            public override long Position
            {
                get => position;
                set => throw new NotSupportedException();
            }

            public override void Flush()
            {
                throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (count == 0 || position == data.Length)
                {
                    return 0;
                }

                buffer[offset] = data[position];
                ++position;
                return 1;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }
        }
    }
}
