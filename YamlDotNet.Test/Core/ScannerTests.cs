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

using Xunit;
using FluentAssertions;
using YamlDotNet.Core;
using YamlDotNet.Core.Tokens;
using System.IO;
using System.Reflection;

#if !PORTABLE && !NETCOREAPP1_0
using System.Runtime.Serialization.Formatters.Binary;
#endif

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
                var comment = sut.Current as Comment;
                if (comment != null)
                {
                    Assert.Equal(8, comment.Start.Index);
                    Assert.Equal(31, comment.End.Index);

                    return;
                }
            }

            Assert.True(false, "Did not find a comment");
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
    }
}