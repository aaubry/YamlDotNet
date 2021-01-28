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

using Xunit;
using YamlDotNet.Core;

namespace YamlDotNet.Test.Core
{
    public class ParserTests : EventsHelper
    {
        [Fact]
        public void EmptyDocument()
        {
            AssertSequenceOfEventsFrom(Yaml.ParserForEmptyContent(),
                StreamStart,
                StreamEnd);
        }

        [Fact]
        public void VerifyEventsOnExample1()
        {
            AssertSequenceOfEventsFrom(Yaml.ParserForResource("01-directives.yaml"),
                StreamStart,
                DocumentStart(Explicit, Version(1, 1),
                    TagDirective("!", "!foo"),
                    TagDirective("!yaml!", TagYaml),
                    TagDirective("!!", TagYaml)),
                PlainScalar(string.Empty),
                DocumentEnd(Implicit),
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample2()
        {
            AssertSequenceOfEventsFrom(Yaml.ParserForResource("02-scalar-in-imp-doc.yaml"),
                StreamStart,
                DocumentStart(Implicit),
                SingleQuotedScalar("a scalar"),
                DocumentEnd(Implicit),
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample3()
        {
            AssertSequenceOfEventsFrom(Yaml.ParserForResource("03-scalar-in-exp-doc.yaml"),
                StreamStart,
                DocumentStart(Explicit),
                SingleQuotedScalar("a scalar"),
                DocumentEnd(Explicit),
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample4()
        {
            AssertSequenceOfEventsFrom(Yaml.ParserForResource("04-scalars-in-multi-docs.yaml"),
                StreamStart,
                DocumentStart(Implicit),
                SingleQuotedScalar("a scalar"),
                DocumentEnd(Implicit),
                DocumentStart(Explicit),
                SingleQuotedScalar("another scalar"),
                DocumentEnd(Implicit),
                DocumentStart(Explicit),
                SingleQuotedScalar("yet another scalar"),
                DocumentEnd(Implicit),
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample5()
        {
            AssertSequenceOfEventsFrom(Yaml.ParserForResource("05-circular-sequence.yaml"),
                StreamStart,
                DocumentStart(Implicit),
                FlowSequenceStart.A("A"),
                AnchorAlias("A"),
                SequenceEnd,
                DocumentEnd(Implicit),
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample6()
        {
            var parser = Yaml.ParserForResource("06-float-tag.yaml");
            AssertSequenceOfEventsFrom(parser,
                StreamStart,
                DocumentStart(Implicit),
                DoubleQuotedScalar("3.14").T(TagYaml + "float"),
                DocumentEnd(Implicit),
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample7()
        {
            AssertSequenceOfEventsFrom(Yaml.ParserForResource("07-scalar-styles.yaml"),
                StreamStart,
                DocumentStart(Explicit),
                PlainScalar(string.Empty),
                DocumentEnd(Implicit),
                DocumentStart(Explicit),
                PlainScalar("a plain scalar"),
                DocumentEnd(Implicit),
                DocumentStart(Explicit),
                SingleQuotedScalar("a single-quoted scalar"),
                DocumentEnd(Implicit),
                DocumentStart(Explicit),
                DoubleQuotedScalar("a double-quoted scalar"),
                DocumentEnd(Implicit),
                DocumentStart(Explicit),
                LiteralScalar("a literal scalar"),
                DocumentEnd(Implicit),
                DocumentStart(Explicit),
                FoldedScalar("a folded scalar"),
                DocumentEnd(Implicit),
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample8()
        {
            AssertSequenceOfEventsFrom(Yaml.ParserForResource("08-flow-sequence.yaml"),
                StreamStart,
                DocumentStart(Implicit),
                FlowSequenceStart,
                PlainScalar("item 1"),
                PlainScalar("item 2"),
                PlainScalar("item 3"),
                SequenceEnd,
                DocumentEnd(Implicit),
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample9()
        {
            AssertSequenceOfEventsFrom(Yaml.ParserForResource("09-flow-mapping.yaml"),
                StreamStart,
                DocumentStart(Implicit),
                FlowMappingStart,
                PlainScalar("a simple key"),
                PlainScalar("a value"),
                PlainScalar("a complex key"),
                PlainScalar("another value"),
                MappingEnd,
                DocumentEnd(Implicit),
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample10()
        {
            AssertSequenceOfEventsFrom(Yaml.ParserForResource("10-mixed-nodes-in-sequence.yaml"),
                StreamStart,
                DocumentStart(Implicit),
                BlockSequenceStart,
                PlainScalar("item 1"),
                PlainScalar("item 2"),
                BlockSequenceStart,
                PlainScalar("item 3.1"),
                PlainScalar("item 3.2"),
                SequenceEnd,
                BlockMappingStart,
                PlainScalar("key 1"),
                PlainScalar("value 1"),
                PlainScalar("key 2"),
                PlainScalar("value 2"),
                MappingEnd,
                SequenceEnd,
                DocumentEnd(Implicit),
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample11()
        {
            AssertSequenceOfEventsFrom(Yaml.ParserForResource("11-mixed-nodes-in-mapping.yaml"),
                StreamStart,
                DocumentStart(Implicit),
                BlockMappingStart,
                PlainScalar("a simple key"),
                PlainScalar("a value"),
                PlainScalar("a complex key"),
                PlainScalar("another value"),
                PlainScalar("a mapping"),
                BlockMappingStart,
                PlainScalar("key 1"),
                PlainScalar("value 1"),
                PlainScalar("key 2"),
                PlainScalar("value 2"),
                MappingEnd,
                PlainScalar("a sequence"),
                BlockSequenceStart,
                PlainScalar("item 1"),
                PlainScalar("item 2"),
                SequenceEnd,
                MappingEnd,
                DocumentEnd(Implicit),
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample12()
        {
            AssertSequenceOfEventsFrom(Yaml.ParserForResource("12-compact-sequence.yaml"),
                StreamStart,
                DocumentStart(Implicit),
                BlockSequenceStart,
                BlockSequenceStart,
                PlainScalar("item 1"),
                PlainScalar("item 2"),
                SequenceEnd,
                BlockMappingStart,
                PlainScalar("key 1"),
                PlainScalar("value 1"),
                PlainScalar("key 2"),
                PlainScalar("value 2"),
                MappingEnd,
                BlockMappingStart,
                PlainScalar("complex key"),
                PlainScalar("complex value"),
                MappingEnd,
                SequenceEnd,
                DocumentEnd(Implicit),
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample13()
        {
            AssertSequenceOfEventsFrom(Yaml.ParserForResource("13-compact-mapping.yaml"),
                StreamStart,
                DocumentStart(Implicit),
                BlockMappingStart,
                PlainScalar("a sequence"),
                BlockSequenceStart,
                PlainScalar("item 1"),
                PlainScalar("item 2"),
                SequenceEnd,
                PlainScalar("a mapping"),
                BlockMappingStart,
                PlainScalar("key 1"),
                PlainScalar("value 1"),
                PlainScalar("key 2"),
                PlainScalar("value 2"),
                MappingEnd,
                MappingEnd,
                DocumentEnd(Implicit),
                StreamEnd);
        }

        [Fact]
        public void VerifyTokensOnExample14()
        {
            AssertSequenceOfEventsFrom(Yaml.ParserForResource("14-mapping-wo-indent.yaml"),
                StreamStart,
                DocumentStart(Implicit),
                BlockMappingStart,
                PlainScalar("key"),
                BlockSequenceStart,
                PlainScalar("item 1"),
                PlainScalar("item 2"),
                SequenceEnd,
                MappingEnd,
                DocumentEnd(Implicit),
                StreamEnd);
        }

        [Fact]
        public void VerifyTokenWithLocalTags()
        {
            AssertSequenceOfEventsFrom(Yaml.ParserForResource("local-tags.yaml"),
                StreamStart,
                DocumentStart(Explicit),
                BlockMappingStart.T("!MyObject").Explicit,
                PlainScalar("a"),
                PlainScalar("1.0"),
                PlainScalar("b"),
                PlainScalar("42"),
                PlainScalar("c"),
                PlainScalar("-7"),
                MappingEnd,
                DocumentEnd(Implicit),
                StreamEnd);
        }

        [Fact]
        public void CommentsAreReturnedWhenRequested()
        {
            AssertSequenceOfEventsFrom(new Parser(new Scanner(Yaml.ReaderForText(@"
                    # Top comment
                    - first # Comment on first item
                    - second
                    - # a mapping
                       ? key # my key
                       : value # my value
                    # Bottom comment
                "), skipComments: false)),
                StreamStart,
                DocumentStart(Implicit),
                StandaloneComment("Top comment"),
                BlockSequenceStart,
                PlainScalar("first"),
                InlineComment("Comment on first item"),
                PlainScalar("second"),
                InlineComment("a mapping"),
                BlockMappingStart,
                PlainScalar("key"),
                InlineComment("my key"),
                PlainScalar("value"),
                InlineComment("my value"),
                StandaloneComment("Bottom comment"),
                MappingEnd,
                SequenceEnd,
                DocumentEnd(Implicit),
                StreamEnd);
        }

        [Fact]
        public void CommentsDoNotInfluenceParsing_Issue_260()
        {
            AssertSequenceOfEventsFrom(new Parser(new Scanner(Yaml.ReaderForText(@"
                    # Some comment
                    key: value
                "), skipComments: false)),
                StreamStart,
                DocumentStart(Implicit),
                StandaloneComment("Some comment"),
                BlockMappingStart,
                PlainScalar("key"),
                PlainScalar("value"),
                MappingEnd,
                DocumentEnd(Implicit),
                StreamEnd);
        }

        [Fact]
        public void CommentsAreOmittedUnlessRequested()
        {
            AssertSequenceOfEventsFrom(Yaml.ParserForText(@"
                    # Top comment
                    - first # Comment on first item
                    - second
                    - # a mapping
                       ? key # my key
                       : value # my value
                    # Bottom comment
                "),
                StreamStart,
                DocumentStart(Implicit),
                BlockSequenceStart,
                PlainScalar("first"),
                PlainScalar("second"),
                BlockMappingStart,
                PlainScalar("key"),
                PlainScalar("value"),
                MappingEnd,
                SequenceEnd,
                DocumentEnd(Implicit),
                StreamEnd);
        }

        [Fact]
        public void VerifyTokenWithMultiDocTag()
        {
            AssertSequenceOfEventsFrom(Yaml.ParserForResource("multi-doc-tag.yaml"),
                StreamStart,
                DocumentStart(Explicit, Version(1, 1),
                    TagDirective("!x!", "tag:example.com,2014:"),
                    TagDirective("!", "!"),
                    TagDirective("!!", TagYaml)),
                BlockMappingStart.T("tag:example.com,2014:foo").Explicit,
                PlainScalar("x"),
                PlainScalar("0"),
                MappingEnd,
                DocumentEnd(Implicit),
                DocumentStart(Explicit),
                BlockMappingStart.T("tag:example.com,2014:bar").Explicit,
                PlainScalar("x"),
                PlainScalar("1"),
                MappingEnd,
                DocumentEnd(Implicit),
                StreamEnd);
        }

        [Fact]
        public void VerifyTokenWithUrlEncodedTagContainingPlusSpaces()
        {
            AssertSequenceOfEventsFrom(Yaml.ParserForText("!(%20%20%20hello+you%20+) value"),
                StreamStart,
                DocumentStart(Implicit),
                PlainScalar("value").T("!(   hello you  )"),
                DocumentEnd(Implicit),
                StreamEnd);
        }

        [Fact]
        public void VerifyTokenWithUrlEncoded32BitsUnicodeTags()
        {
            AssertSequenceOfEventsFrom(Yaml.ParserForText("!hel%F4%8F%BF%BFlo%E2%99%A5+A%20 value"),
                StreamStart,
                DocumentStart(Implicit),
                PlainScalar("value").T("!hel􏿿lo♥ A "),
                DocumentEnd(Implicit),
                StreamEnd);
        }

        [Fact]
        public void VerifyCommentTypeAfterScalarBlock()
        {
            AssertSequenceOfEventsFrom(new Parser(new Scanner(Yaml.ReaderForText("|-\r\n  text\r\n#comment"), false)),
                StreamStart,
                DocumentStart(Implicit),
                LiteralScalar("text"),
                StandaloneComment("comment"),
                DocumentEnd(Implicit),
                StreamEnd);
        }

        [Theory]
        [InlineData("|\n  b-carriage-return,b-line-feed\r\n  lll", "b-carriage-return,b-line-feed\nlll")]
        [InlineData("|\n  b-carriage-return\r  lll", "b-carriage-return\nlll")]
        [InlineData("|\n  b-line-feed\n  lll", "b-line-feed\nlll")]
        [InlineData("|\n  b-next-line\x85  lll", "b-next-line\nlll")]
        [InlineData("|\n  b-line-separator\x2028  lll", "b-line-separator\x2028lll")]
        [InlineData("|\n  b-paragraph-separator\x2029  lll", "b-paragraph-separator\x2029lll")]
        public void NewLinesAreParsedAccordingToTheSpecification(string yaml, string expected)
        {
            AssertSequenceOfEventsFrom(Yaml.ParserForText(yaml),
                StreamStart,
                DocumentStart(Implicit),
                LiteralScalar(expected),
                DocumentEnd(Implicit),
                StreamEnd);
        }
    }
}