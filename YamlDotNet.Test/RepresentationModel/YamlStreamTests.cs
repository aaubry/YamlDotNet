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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization.Schemas;

namespace YamlDotNet.Test.RepresentationModel
{
    public class YamlStreamTests
    {
        [Fact]
        public void LoadSimpleDocument()
        {
            var stream = new YamlStream();
            stream.Load(Yaml.ReaderFrom("02-scalar-in-imp-doc.yaml"));

            Assert.Equal(1, stream.Documents.Count);
            Assert.IsType<YamlScalarNode>(stream.Documents[0].RootNode);
            Assert.Equal("a scalar", ((YamlScalarNode)stream.Documents[0].RootNode).Value);
            Assert.Equal(YamlNodeType.Scalar, stream.Documents[0].RootNode.NodeType);
        }

        [Theory]
        [InlineData("&a [*a]")]
        [InlineData("?\n  key: &id1\n    recursion: *id1\n: foo")]
        public void AccessingAllNodesOnInfinitelyRecursiveDocumentThrows(string yaml)
        {
            var stream = new YamlStream();
            stream.Load(Yaml.ParserForText(yaml));

            var accessAllNodes = new Action(() => stream.Documents.Single().AllNodes.ToList());

            accessAllNodes.ShouldThrow<MaximumRecursionLevelReachedException>("because the document is infinitely recursive.");
        }

        [Theory]
        [InlineData("&a [*a]")]
        [InlineData("?\n  key: &id1\n    recursion: *id1\n: foo")]
        public void InfinitelyRecursiveNodeToStringSucceeds(string yaml)
        {
            var stream = new YamlStream();
            stream.Load(Yaml.ParserForText(yaml));

            var toString = stream.Documents.Single().RootNode.ToString();

            toString.Should().Contain("WARNING! INFINITE RECURSION!");
        }

        [Fact]
        public void BackwardAliasReferenceWorks()
        {
            var stream = new YamlStream();
            stream.Load(Yaml.ReaderFrom("backwards-alias.yaml"));

            Assert.Equal(1, stream.Documents.Count);
            Assert.IsType<YamlSequenceNode>(stream.Documents[0].RootNode);

            var sequence = (YamlSequenceNode)stream.Documents[0].RootNode;
            Assert.Equal(3, sequence.Children.Count);

            Assert.Equal("a scalar", ((YamlScalarNode)sequence.Children[0]).Value);
            Assert.Equal("another scalar", ((YamlScalarNode)sequence.Children[1]).Value);
            Assert.Equal("a scalar", ((YamlScalarNode)sequence.Children[2]).Value);
            Assert.Same(sequence.Children[0], sequence.Children[2]);
        }

        [Fact]
        public void ForwardAliasReferenceWorks()
        {
            var stream = new YamlStream();
            stream.Load(Yaml.ReaderFrom("forward-alias.yaml"));

            Assert.Equal(1, stream.Documents.Count);
            Assert.IsType<YamlSequenceNode>(stream.Documents[0].RootNode);

            var sequence = (YamlSequenceNode)stream.Documents[0].RootNode;
            Assert.Equal(3, sequence.Children.Count);

            Assert.Equal("a scalar", ((YamlScalarNode)sequence.Children[0]).Value);
            Assert.Equal("another scalar", ((YamlScalarNode)sequence.Children[1]).Value);
            Assert.Equal("a scalar", ((YamlScalarNode)sequence.Children[2]).Value);
            Assert.Same(sequence.Children[0], sequence.Children[2]);
        }

        [Theory]
        [InlineData("B: !!null ", "", ScalarStyle.Plain, false)]
        [InlineData("B: ", "", ScalarStyle.Plain, true)]
        [InlineData("B: abc", "abc", ScalarStyle.Plain, true)]
        [InlineData("B: ~", "~", ScalarStyle.Plain, true)]
        [InlineData("B: Null", "Null", ScalarStyle.Plain, true)]
        [InlineData("B: ''", "", ScalarStyle.SingleQuoted, true)]
        [InlineData("B: 'Null'", "Null", ScalarStyle.SingleQuoted, true)]
        public void ImplicitNullRoundtrips(string yaml, string value, ScalarStyle style, bool implicitPlain)
        {
            //load
            var stream = new YamlStream();
            stream.Load(new StringReader(yaml));
            var mapping = (YamlMappingNode)stream.Documents[0].RootNode;
            var map = mapping.Children[0];

            var yamlValue = (YamlScalarNode)map.Value;
            Assert.Equal(value, yamlValue.Value);

            var emitter = new RoundTripNullTestEmitter(implicitPlain, style);
            yamlValue.Emit(emitter, null);

            var stringWriter = new StringWriter();
            new YamlStream(stream.Documents).Save(stringWriter);
            Assert.Equal($@"{yaml}
...".NormalizeNewLines(),
stringWriter.ToString().NormalizeNewLines().TrimNewLines());
        }

        [Fact]
        public void EmptyScalarsAreEmptySingleQuoted()
        {
            var stringWriter = new StringWriter();
            var rootNode = new YamlMappingNode();
            rootNode.Children.Add(new YamlScalarNode("test"), new YamlScalarNode(""));
            var document = new YamlDocument(rootNode);
            var yamlStream = new YamlStream(document);
            yamlStream.Save(stringWriter);
            var actual = stringWriter.ToString().NormalizeNewLines();
            var expected = "test: ''\r\n...\r\n".NormalizeNewLines();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NullScalarsAreEmptyPlain()
        {
            var stringWriter = new StringWriter();
            var rootNode = new YamlMappingNode();
            rootNode.Children.Add(new YamlScalarNode("test"), new YamlScalarNode(null));
            var document = new YamlDocument(rootNode);
            var yamlStream = new YamlStream(document);
            yamlStream.Save(stringWriter);
            var actual = stringWriter.ToString().NormalizeNewLines();
            var expected = "test: \r\n...\r\n".NormalizeNewLines();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RoundtripExample1()
        {
            RoundtripTest("01-directives.yaml");
        }

        [Fact]
        public void RoundtripExample2()
        {
            RoundtripTest("02-scalar-in-imp-doc.yaml");
        }

        [Fact]
        public void RoundtripExample3()
        {
            RoundtripTest("03-scalar-in-exp-doc.yaml");
        }

        [Fact]
        public void RoundtripExample4()
        {
            RoundtripTest("04-scalars-in-multi-docs.yaml");
        }

        [Fact]
        public void RoundtripExample5()
        {
            RoundtripTest("06-float-tag.yaml");
        }

        [Fact]
        public void RoundtripExample6()
        {
            RoundtripTest("06-float-tag.yaml");
        }

        [Fact]
        public void RoundtripExample7()
        {
            RoundtripTest("07-scalar-styles.yaml");
        }

        [Fact]
        public void RoundtripExample8()
        {
            RoundtripTest("08-flow-sequence.yaml");
        }

        [Fact]
        public void RoundtripExample9()
        {
            RoundtripTest("09-flow-mapping.yaml");
        }

        [Fact]
        public void RoundtripExample10()
        {
            RoundtripTest("10-mixed-nodes-in-sequence.yaml");
        }

        [Fact]
        public void RoundtripExample11()
        {
            RoundtripTest("11-mixed-nodes-in-mapping.yaml");
        }

        [Fact]
        public void RoundtripExample12()
        {
            RoundtripTest("12-compact-sequence.yaml");
        }

        [Fact]
        public void RoundtripExample13()
        {
            RoundtripTest("13-compact-mapping.yaml");
        }

        [Fact]
        public void RoundtripExample14()
        {
            RoundtripTest("14-mapping-wo-indent.yaml");
        }

        [Fact]
        public void RoundtripBackreference()
        {
            RoundtripTest("backreference.yaml");
        }

        [Fact]
        public void FailBackreference()
        {
            RoundtripTest("fail-backreference.yaml");
        }

        [Fact]
        public void Roundtrip32BitsUnicodeEscape()
        {
            RoundtripTest("unicode-32bits-escape.yaml");
        }

        [Fact]
        public void AnchorsOverwriting()
        {
            RoundtripTest("anchors-overwriting.yaml");
        }

        [Fact]
        public void AllAliasesMustBeResolved()
        {
            var original = new YamlStream();
            Assert.Throws<AnchorNotFoundException>(() => original.Load(Yaml.ReaderFrom("invalid-reference.yaml")));
        }

        [Fact]
        public void CanReadValueWithQuotes()
        {
            var yamlStream = new YamlStream();
            yamlStream.Load(Yaml.ParserForText("description: -\"\" should be zero"));
            var nodes = yamlStream.Documents.Single().AllNodes.ToArray();
            Assert.Equal("description", nodes[1].ToString());
            Assert.Equal("-\"\" should be zero", nodes[2].ToString());
        }

        private void RoundtripTest(string yamlFileName)
        {
            var original = new YamlStream();
            original.Load(Yaml.ReaderFrom(yamlFileName));

            var buffer = new StringBuilder();
            original.Save(new StringWriter(buffer));

            var final = new YamlStream();
            final.Load(new StringReader(buffer.ToString()));

            var originalBuilder = new YamlDocumentStructureBuilder();
            original.Accept(originalBuilder);

            var finalBuilder = new YamlDocumentStructureBuilder();
            final.Accept(finalBuilder);

            Assert.Equal(originalBuilder.Events.Count, finalBuilder.Events.Count);

            for (var i = 0; i < originalBuilder.Events.Count; ++i)
            {
                var originalEvent = originalBuilder.Events[i];
                var finalEvent = finalBuilder.Events[i];

                Assert.Equal(originalEvent.Type, finalEvent.Type);
                Assert.Equal(originalEvent.Value, finalEvent.Value);
            }
        }

        private class YamlDocumentStructureBuilder : YamlVisitorBase
        {
            private readonly List<YamlNodeEvent> events = new List<YamlNodeEvent>();

            public IList<YamlNodeEvent> Events
            {
                get
                {
                    return events;
                }
            }

            public override void Visit(YamlScalarNode scalar)
            {
                events.Add(new YamlNodeEvent(YamlNodeEventType.Scalar, scalar.Anchor, scalar.Tag, scalar.Value));
            }

            public override void Visit(YamlSequenceNode sequence)
            {
                events.Add(new YamlNodeEvent(YamlNodeEventType.SequenceStart, sequence.Anchor, sequence.Tag, null));
                base.Visit(sequence);
                events.Add(new YamlNodeEvent(YamlNodeEventType.SequenceEnd, sequence.Anchor, sequence.Tag, null));
            }

            public override void Visit(YamlMappingNode mapping)
            {
                events.Add(new YamlNodeEvent(YamlNodeEventType.MappingStart, mapping.Anchor, mapping.Tag, null));
                base.Visit(mapping);
                events.Add(new YamlNodeEvent(YamlNodeEventType.MappingEnd, mapping.Anchor, mapping.Tag, null));
            }
        }

        private class YamlNodeEvent
        {
            public YamlNodeEventType Type { get; }
            public AnchorName Anchor { get; }
            public TagName Tag { get; }
            public string Value { get; }

            public YamlNodeEvent(YamlNodeEventType type, AnchorName anchor, TagName tag, string value)
            {
                Type = type;
                Anchor = anchor;
                Tag = tag;
                Value = value;
            }
        }

        private enum YamlNodeEventType
        {
            SequenceStart,
            SequenceEnd,
            MappingStart,
            MappingEnd,
            Scalar,
        }

        private class RoundTripNullTestEmitter : IEmitter
        {
            private readonly bool enforceImplicit;
            private readonly ScalarStyle style;

            public RoundTripNullTestEmitter(bool enforceImplicit, ScalarStyle style)
            {
                this.enforceImplicit = enforceImplicit;
                this.style = style;
            }

            public void Emit(ParsingEvent @event)
            {
                Assert.Equal(EventType.Scalar, @event.Type);
                Assert.IsType<Scalar>(@event);
                var scalar = (Scalar)@event;

                Assert.Equal(style, scalar.Style);
                if (enforceImplicit)
                {
                    Assert.True(scalar.IsPlainImplicit);
                }
                else
                {
                    Assert.False(scalar.IsPlainImplicit);
                }

                Assert.False(scalar.IsQuotedImplicit);
            }
        }
    }
}
