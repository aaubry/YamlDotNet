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

using System.Collections;
using System.Reflection;
using FluentAssertions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using TagDirective = YamlDotNet.Core.Tokens.TagDirective;
using VersionDirective = YamlDotNet.Core.Tokens.VersionDirective;

// ReSharper disable MemberHidesStaticFromOuterClass
namespace YamlDotNet.Test.Core
{
    public class EventsHelper
    {
        protected const bool Explicit = false;
        protected const bool Implicit = true;
        protected const string TagYaml = "tag:yaml.org,2002:";

        protected static readonly TagDirective[] DefaultTags =
        {
            new TagDirective("!", "!"),
            new TagDirective("!!", TagYaml)
        };

        protected static StreamStart StreamStart
        {
            get { return new StreamStart(); }
        }

        protected static StreamEnd StreamEnd
        {
            get { return new StreamEnd(); }
        }

        protected DocumentStart DocumentStart(bool isImplicit)
        {
            return DocumentStart(isImplicit, null, DefaultTags);
        }

        protected DocumentStart DocumentStart(bool isImplicit, VersionDirective version, params TagDirective[] tags)
        {
            return new DocumentStart(version, new TagDirectiveCollection(tags), isImplicit);
        }

        protected VersionDirective Version(int major, int minor)
        {
            return new VersionDirective(new Version(major, minor));
        }

        protected TagDirective TagDirective(string handle, string prefix)
        {
            return new TagDirective(handle, prefix);
        }

        protected DocumentEnd DocumentEnd(bool isImplicit)
        {
            return new DocumentEnd(isImplicit);
        }

        protected ScalarBuilder Scalar(string text)
        {
            return new ScalarBuilder(text, ScalarStyle.Any);
        }

        protected ScalarBuilder PlainScalar(string text)
        {
            return new ScalarBuilder(text, ScalarStyle.Plain);
        }

        protected ScalarBuilder SingleQuotedScalar(string text)
        {
            return new ScalarBuilder(text, ScalarStyle.SingleQuoted);
        }

        protected ScalarBuilder DoubleQuotedScalar(string text)
        {
            return new ScalarBuilder(text, ScalarStyle.DoubleQuoted);
        }

        protected ScalarBuilder LiteralScalar(string text)
        {
            return new ScalarBuilder(text, ScalarStyle.Literal);
        }

        protected ScalarBuilder FoldedScalar(string text)
        {
            return new ScalarBuilder(text, ScalarStyle.Folded);
        }

        protected SequenceStartBuilder BlockSequenceStart
        {
            get { return new SequenceStartBuilder(SequenceStyle.Block); }
        }

        protected SequenceStartBuilder FlowSequenceStart
        {
            get { return new SequenceStartBuilder(SequenceStyle.Flow); }
        }

        protected SequenceEnd SequenceEnd
        {
            get { return new SequenceEnd(); }
        }

        protected MappingStart MappingStart
        {
            get { return new MappingStart(); }
        }

        protected MappingStartBuilder BlockMappingStart
        {
            get { return new MappingStartBuilder(MappingStyle.Block); }
        }

        protected MappingStartBuilder FlowMappingStart
        {
            get { return new MappingStartBuilder(MappingStyle.Flow); }
        }

        protected MappingEnd MappingEnd
        {
            get { return new MappingEnd(); }
        }

        protected AnchorAlias AnchorAlias(string alias)
        {
            return new AnchorAlias(new AnchorName(alias));
        }

        protected Comment StandaloneComment(string value)
        {
            return new Comment(value, false);
        }

        protected Comment InlineComment(string value)
        {
            return new Comment(value, true);
        }

        protected class ScalarBuilder
        {
            private readonly string text;
            private readonly ScalarStyle style;
            private string tag;
            private bool plainImplicit;
            private bool quotedImplicit;

            public ScalarBuilder(string text, ScalarStyle style)
            {
                this.text = text;
                this.style = style;
                plainImplicit = style == ScalarStyle.Plain;
                quotedImplicit = style != ScalarStyle.Plain &&
                                 style != ScalarStyle.Any;
            }

            public ScalarBuilder T(string tag)
            {
                this.tag = tag;
                plainImplicit = false;
                quotedImplicit = false;
                return this;
            }

            public ScalarBuilder ImplicitPlain
            {
                get
                {
                    plainImplicit = true;
                    return this;
                }
            }

            public ScalarBuilder ImplicitQuoted
            {
                get
                {
                    quotedImplicit = true;
                    return this;
                }
            }

            public static implicit operator Scalar(ScalarBuilder builder)
            {
                return new Scalar(AnchorName.Empty,
                    builder.tag,
                    builder.text,
                    builder.style,
                    builder.plainImplicit,
                    builder.quotedImplicit);
            }
        }

        protected class SequenceStartBuilder
        {
            private const bool DefaultImplicit = true;

            private readonly SequenceStyle style;
            private AnchorName anchor;
            private bool @implicit;

            public SequenceStartBuilder(SequenceStyle style)
            {
                this.style = style;
                @implicit = DefaultImplicit;
            }

            public SequenceStartBuilder A(string anchor)
            {
                this.anchor = new AnchorName(anchor);
                return this;
            }

            public SequenceStartBuilder Explicit
            {
                get
                {
                    @implicit = false;
                    return this;
                }
            }

            public static implicit operator SequenceStart(SequenceStartBuilder builder)
            {
                return new SequenceStart(builder.anchor, null, builder.@implicit, builder.style);
            }
        }

        protected class MappingStartBuilder
        {
            private const bool DefaultImplicit = true;

            private readonly MappingStyle style;
            private string tag;
            private bool @implicit;

            public MappingStartBuilder(MappingStyle style)
            {
                this.style = style;
                @implicit = DefaultImplicit;
            }

            public MappingStartBuilder T(string tag)
            {
                this.tag = tag;
                return this;
            }

            public MappingStartBuilder Explicit
            {
                get
                {
                    @implicit = false;
                    return this;
                }
            }

            public static implicit operator MappingStart(MappingStartBuilder builder)
            {
                return new MappingStart(AnchorName.Empty, builder.tag, builder.@implicit, builder.style);
            }
        }

        protected void AssertSequenceOfEventsFrom(IParser parser, params ParsingEvent[] events)
        {
            var eventNumber = 1;
            foreach (var expected in events)
            {
                parser.MoveNext().Should().BeTrue("Missing parse event number {0}", eventNumber);
                AssertEvent(expected, parser.Current, eventNumber);
                eventNumber++;
            }
            parser.MoveNext().Should().BeFalse("Found extra parse events");
        }

        protected void AssertEvent(ParsingEvent expected, ParsingEvent actual, int eventNumber)
        {
            actual.GetType().Should().Be(expected.GetType(), "Parse event {0} is not of the expected type.", eventNumber);

            foreach (var property in expected.GetType().GetTypeInfo().GetProperties())
            {
                if (property.PropertyType == typeof(Mark) || !property.CanRead)
                {
                    continue;
                }

                var value = property.GetValue(actual, null);
                var expectedValue = property.GetValue(expected, null);
                if (expectedValue is IEnumerable enumerable && !(expectedValue is string))
                {
                    if (expectedValue is ICollection expectedCollection && value is ICollection valueCollection)
                    {
                        var expectedCount = expectedCollection.Count;
                        var valueCount = valueCollection.Count;
                        valueCount.Should().Be(expectedCount, "Compared size of collections in property {0} in parse event {1}",
                            property.Name, eventNumber);
                    }

                    var values = ((IEnumerable)value).GetEnumerator();
                    var expectedValues = enumerable.GetEnumerator();
                    while (expectedValues.MoveNext())
                    {
                        values.MoveNext().Should().BeTrue("Property {0} in parse event {1} had too few elements", property.Name, eventNumber);
                        values.Current.Should().Be(expectedValues.Current,
                            "Compared element in property {0} in parse event {1}", property.Name, eventNumber);
                    }
                    values.MoveNext().Should().BeFalse("Property {0} in parse event {1} had too many elements", property.Name, eventNumber);
                }
                else
                {
                    value.Should().Be(expectedValue, "Compared property {0} in parse event {1}", property.Name, eventNumber);
                }
            }
        }
    }
}
