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

using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;

namespace YamlDotNet.Test.Core
{
    public class EmitterTests : EmitterTestsHelper
    {
        [Theory]
        [InlineData("01-directives.yaml")]
        [InlineData("02-scalar-in-imp-doc.yaml")]
        [InlineData("03-scalar-in-exp-doc.yaml")]
        [InlineData("04-scalars-in-multi-docs.yaml")]
        [InlineData("05-circular-sequence.yaml")]
        [InlineData("06-float-tag.yaml")]
        [InlineData("07-scalar-styles.yaml")]
        [InlineData("08-flow-sequence.yaml")]
        [InlineData("09-flow-mapping.yaml")]
        [InlineData("10-mixed-nodes-in-sequence.yaml")]
        [InlineData("11-mixed-nodes-in-mapping.yaml")]
        [InlineData("12-compact-sequence.yaml")]
        [InlineData("13-compact-mapping.yaml")]
        [InlineData("14-mapping-wo-indent.yaml")]
        public void CompareOriginalAndEmittedText(string filename)
        {
            var stream = Yaml.StreamFrom(filename);

            var originalEvents = ParsingEventsOf(stream.ReadToEnd());
            originalEvents.Run(x => Dump.WriteLine(x));
            var emittedText = EmittedTextFrom(originalEvents);
            Dump.WriteLine(emittedText);
            var emittedEvents = ParsingEventsOf(emittedText);
            emittedEvents.Run(x => Dump.WriteLine(x));

            emittedEvents.ShouldAllBeEquivalentTo(originalEvents,
                opt => opt.Excluding(@event => @event.Start)
                          .Excluding(@event => @event.End)
                          .Excluding((ParsingEvent @event) => ((DocumentEnd)@event).IsImplicit));
        }

        private IList<ParsingEvent> ParsingEventsOf(string text)
        {
            var parser = new Parser(new StringReader(text));
            return EnumerationOf(parser).ToList();
        }

        private IEnumerable<ParsingEvent> EnumerationOf(IParser parser)
        {
            while (parser.MoveNext())
            {
                yield return parser.Current;
            }
        }

        [Fact]
        public void PlainScalarCanBeFollowedByImplicitDocument()
        {
            var events = StreamOf(
                DocumentWith(PlainScalar("test")),
                DocumentWith(PlainScalar("test")));

            var yaml = EmittedTextFrom(events);

            yaml.Should().Contain(Lines("test", "--- test"));
        }

        [Fact]
        public void PlainScalarCanBeFollowedByDocumentWithVersion()
        {
            var events = StreamOf(
                DocumentWith(PlainScalar("test")),
                DocumentWithVersion(PlainScalar("test")));

            var yaml = EmittedTextFrom(events);

            Dump.WriteLine(yaml);
            yaml.Should().Contain(Lines("test", "...", "%YAML 1.1", "--- test"));
        }

        [Fact]
        public void PlainScalarCanBeFollowedByDocumentWithDefaultTags()
        {
            var events = StreamOf(
                DocumentWith(PlainScalar("test")),
                DocumentWithDefaultTags(PlainScalar("test")));

            var yaml = EmittedTextFrom(events);

            Dump.WriteLine(yaml);
            yaml.Should().Contain(Lines("test", "--- test"));
        }

        [Fact]
        public void PlainScalarCanBeFollowedByDocumentWithCustomTags()
        {
            var events = StreamOf(
                DocumentWith(PlainScalar("test")),
                DocumentWithCustomTags(PlainScalar("test")));

            var yaml = EmittedTextFrom(events);

            Dump.WriteLine(yaml);
            yaml.Should().Contain(Lines("test", "...", FooTag, ExTag, ExExTag, "--- test"));
        }

        [Fact]
        public void BlockCanBeFollowedByImplicitDocument()
        {
            var events = StreamOf(
                DocumentWith(SequenceWith(SingleQuotedScalar("test"))),
                DocumentWith(PlainScalar("test")));

            var yaml = EmittedTextFrom(events);

            Dump.WriteLine(yaml);

            yaml.Should().Contain(Lines("- 'test'", "--- test"));
        }

        [Fact]
        public void BlockCanBeFollowedByDocumentWithVersion()
        {
            var events = StreamOf(
                DocumentWith(SequenceWith(SingleQuotedScalar("test"))),
                DocumentWithVersion(PlainScalar("test")));

            var yaml = EmittedTextFrom(events);

            Dump.WriteLine(yaml);
            yaml.Should().Contain(Lines("- 'test'", "...", "%YAML 1.1", "--- test"));
        }

        [Fact]
        public void BlockCanBeFollowedByDocumentWithDefaultTags()
        {
            var events = StreamOf(
                DocumentWith(SequenceWith(SingleQuotedScalar("test"))),
                DocumentWithDefaultTags(PlainScalar("test")));

            var yaml = EmittedTextFrom(events);

            Dump.WriteLine(yaml);
            yaml.Should().Contain(Lines("- 'test'", "--- test"));
        }

        [Fact]
        public void BlockCanBeFollowedByDocumentWithCustomTags()
        {
            var events = StreamOf(
                DocumentWith(SequenceWith(SingleQuotedScalar("test"))),
                DocumentWithCustomTags(PlainScalar("test")));

            var yaml = EmittedTextFrom(events);

            Dump.WriteLine(yaml);
            yaml.Should().Contain(Lines("- 'test'", "...", FooTag, ExTag, ExExTag, "--- test"));
        }

        [Theory]
        [InlineData("LF hello\nworld")]
        [InlineData("CRLF hello\r\nworld")]
        public void FoldedStyleDoesNotLooseCharacters(string text)
        {
            var events = SequenceWith(FoldedScalar(text));

            var yaml = EmittedTextFrom(StreamedDocumentWith(events));

            Dump.WriteLine(yaml);
            yaml.Should().Contain("world");
        }

        [Fact]
        public void FoldedStyleIsSelectedWhenNewLinesAreFoundInLiteral()
        {
            var events = SequenceWith(Scalar("hello\nworld").ImplicitPlain);

            var yaml = EmittedTextFrom(StreamedDocumentWith(events));

            Dump.WriteLine(yaml);
            yaml.Should().Contain(">");
        }

        [Fact]
        public void FoldedStyleDoesNotGenerateExtraLineBreaks()
        {
            var events = SequenceWith(FoldedScalar("hello\nworld"));

            var yaml = EmittedTextFrom(StreamedDocumentWith(events));

            // Todo: Why involve the rep. model when testing the Emitter? Can we match using a regex?
            var stream = new YamlStream();
            stream.Load(new StringReader(yaml));
            var sequence = (YamlSequenceNode)stream.Documents[0].RootNode;
            var scalar = (YamlScalarNode)sequence.Children[0];

            Dump.WriteLine(yaml);
            scalar.Value.Should().Be("hello\nworld");
        }

        [Fact]
        public void FoldedStyleDoesNotCollapseLineBreaks()
        {
            var events = SequenceWith(FoldedScalar(">+\n"));

            var yaml = EmittedTextFrom(StreamedDocumentWith(events));

            var stream = new YamlStream();
            stream.Load(new StringReader(yaml));
            var sequence = (YamlSequenceNode)stream.Documents[0].RootNode;
            var scalar = (YamlScalarNode)sequence.Children[0];

            Dump.WriteLine("${0}$", yaml);
            scalar.Value.Should().Be(">+\n");
        }

        [Fact]
        [Trait("motive", "issue #39")]
        public void FoldedStylePreservesNewLines()
        {
            var input = "id: 0\nPayload:\n  X: 5\n  Y: 6\n";
            var events = MappingWith(
                Scalar("Payload").ImplicitPlain,
                FoldedScalar(input));

            var yaml = EmittedTextFrom(StreamedDocumentWith(events));
            Dump.WriteLine(yaml);

            var stream = new YamlStream();
            stream.Load(new StringReader(yaml));

            var mapping = (YamlMappingNode)stream.Documents[0].RootNode;
            var value = (YamlScalarNode)mapping.Children.First().Value;

            Dump.WriteLine(value.Value);
            value.Value.Should().Be(input);
        }

        [Fact]
        public void CommentsAreEmittedCorrectly()
        {
            var events = SequenceWith(
                StandaloneComment("Top comment"),
                StandaloneComment("Second line"),
                Scalar("first").ImplicitPlain,
                InlineComment("The first value"),
                Scalar("second").ImplicitPlain,
                InlineComment("The second value"),
                StandaloneComment("Bottom comment")
            );

            var yaml = EmittedTextFrom(StreamedDocumentWith(events));

            Dump.WriteLine("${0}$", yaml);
            yaml.Should()
                .Contain("# Top comment")
                .And.Contain("# Second line")
                .And.NotContain("# Top comment # Second line")
                .And.Contain("first # The first value")
                .And.Contain("second # The second value")
                .And.Contain("# Bottom comment");
        }

        [Theory]
        [InlineData("Гранит", 28595)] // Cyrillic (ISO)
        [InlineData("ГÀƊȽ˱ώҔׂۋᵷẁό₩וּﺪﺸﻸﭧ╬♫₹Ὰỗ᷁ݭ٭ӢР͞ʓǈĄë0", 65001)] // UTF-8
        public void UnicodeInScalarsCanBeSingleQuotedWhenOutputEncodingSupportsIt(string text, int codePage)
        {
            var document = StreamedDocumentWith(
                SequenceWith(
                    SingleQuotedScalar(text)
                )
            );

            var buffer = new MemoryStream();
            var encoding = Encoding.GetEncoding(codePage);

            using (var writer = new StreamWriter(buffer, encoding))
            {
                var emitter = new Emitter(writer, 2, int.MaxValue, false);
                foreach (var evt in document)
                {
                    emitter.Emit(evt);
                }
            }

            var yaml = encoding.GetString(buffer.ToArray());

            yaml.Should()
                .Contain("'" + text + "'");
        }

        [Fact]
        public void EmptyStringsAreQuoted()
        {
            var events = SequenceWith(
                Scalar(string.Empty).ImplicitPlain
            );

            var yaml = EmittedTextFrom(StreamedDocumentWith(events));
            yaml.Should()
                .Contain("- ''");
        }

        [Theory]
        [InlineData("b-carriage-return,b-line-feed\r\nlll", "b-carriage-return,b-line-feed\nlll")]
        [InlineData("b-carriage-return\rlll", "b-carriage-return\nlll")]
        [InlineData("b-line-feed\nlll", "b-line-feed\nlll")]
        [InlineData("b-next-line\x85lll", "b-next-line\nlll")]
        [InlineData("b-line-separator\x2028lll", "b-line-separator\x2028lll")]
        [InlineData("b-paragraph-separator\x2029lll", "b-paragraph-separator\x2029lll")]
        public void NewLinesAreNotDuplicatedWhenEmitted(string input, string expected)
        {
            var yaml = EmittedTextFrom(StreamOf(DocumentWith(
                LiteralScalar(input)
            )));

            var parser = new Parser(new StringReader(yaml));

            AssertSequenceOfEventsFrom(Yaml.ParserForText(yaml),
                StreamStart,
                DocumentStart(Implicit),
                LiteralScalar(expected),
                DocumentEnd(Implicit),
                StreamEnd);
        }

        [Theory]
        [InlineData("'.'test")]
        [InlineData("'")]
        [InlineData("'.'")]
        [InlineData("'test")]
        [InlineData("'test'")]
        public void SingleQuotesAreDoubleQuoted(string input)
        {
            var events = StreamOf(DocumentWith(PlainScalar(input)));
            var yaml = EmittedTextFrom(events);

            string expected = string.Format("\"{0}\"", input);

            yaml.Should().Contain(expected);
        }


        private string Lines(params string[] lines)
        {
            return string.Join(Environment.NewLine, lines);
        }
    }
}