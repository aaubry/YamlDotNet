//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2008, 2009, 2010, 2011, 2012, 2013 Antoine Aubry
	
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

using System.Collections;
using System.IO;
using FluentAssertions;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Test.Core
{
	public class ParserTests : EventsHelper
	{
		[Fact]
		public void EmptyDocument()
		{
			AssertSequenceOfEventsFrom(ParserForEmptyContent(),
				StreamStart,
				StreamEnd);
		}

		[Fact]
		public void VerifyEventsOnExample1()
		{
			AssertSequenceOfEventsFrom(ParserFor("01-directives.yaml"),
				StreamStart,
				DocumentStart(Explicit, Version(1, 1),
					TagDirective("!", "!foo"),
					TagDirective("!yaml!", TagYaml),
					TagDirective("!!", TagYaml)),
				PlainScalar(string.Empty).ExplicitQuoted,
				DocumentEnd(Implicit),
				StreamEnd);
		}

		[Fact]
		public void VerifyTokensOnExample2()
		{
			AssertSequenceOfEventsFrom(ParserFor("02-scalar-in-imp-doc.yaml"),
				StreamStart,
				DocumentStart(Implicit),
				SingleQuotedScalar("a scalar").ExplicitPlain,
				DocumentEnd(Implicit),
				StreamEnd);
		}

		[Fact]
		public void VerifyTokensOnExample3()
		{
			AssertSequenceOfEventsFrom(ParserFor("03-scalar-in-exp-doc.yaml"),
				StreamStart,
				DocumentStart(Explicit),
				SingleQuotedScalar("a scalar").ExplicitPlain,
				DocumentEnd(Explicit),
				StreamEnd);
		}

		[Fact]
		public void VerifyTokensOnExample4()
		{
			AssertSequenceOfEventsFrom(ParserFor("04-scalars-in-multi-docs.yaml"),
				StreamStart,
				DocumentStart(Implicit),
				SingleQuotedScalar("a scalar").ExplicitPlain,
				DocumentEnd(Implicit),
				DocumentStart(Explicit),
				SingleQuotedScalar("another scalar").ExplicitPlain,
				DocumentEnd(Implicit),
				DocumentStart(Explicit),
				SingleQuotedScalar("yet another scalar").ExplicitPlain,
				DocumentEnd(Implicit),
				StreamEnd);
		}

		[Fact]
		public void VerifyTokensOnExample5()
		{
			AssertSequenceOfEventsFrom(ParserFor("05-circular-sequence.yaml"),
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
			var parser = ParserFor("06-float-tag.yaml");
			AssertSequenceOfEventsFrom(parser,
				StreamStart,
				DocumentStart(Implicit),
				DoubleQuotedScalar("3.14").T(TagYaml + "float").Explicit,
				DocumentEnd(Implicit),
				StreamEnd);
		}

		[Fact]
		public void VerifyTokensOnExample7()
		{
			AssertSequenceOfEventsFrom(ParserFor("07-scalar-styles.yaml"),
				StreamStart,
				DocumentStart(Explicit),
				PlainScalar(string.Empty).ExplicitQuoted,
				DocumentEnd(Implicit),
				DocumentStart(Explicit),
				PlainScalar("a plain scalar").ExplicitQuoted,
				DocumentEnd(Implicit),
				DocumentStart(Explicit),
				SingleQuotedScalar("a single-quoted scalar").ExplicitPlain,
				DocumentEnd(Implicit),
				DocumentStart(Explicit),
				DoubleQuotedScalar("a double-quoted scalar").ExplicitPlain,
				DocumentEnd(Implicit),
				DocumentStart(Explicit),
				LiteralScalar("a literal scalar").ExplicitPlain,
				DocumentEnd(Implicit),
				DocumentStart(Explicit),
				FoldedScalar("a folded scalar").ExplicitPlain,
				DocumentEnd(Implicit),
				StreamEnd);
		}

		[Fact]
		public void VerifyTokensOnExample8()
		{
			AssertSequenceOfEventsFrom(ParserFor("08-flow-sequence.yaml"),
				StreamStart,
				DocumentStart(Implicit),
				FlowSequenceStart,
				PlainScalar("item 1").ExplicitQuoted,
				PlainScalar("item 2").ExplicitQuoted,
				PlainScalar("item 3").ExplicitQuoted,
				SequenceEnd,
				DocumentEnd(Implicit),
				StreamEnd);
		}

		[Fact]
		public void VerifyTokensOnExample9()
		{
			AssertSequenceOfEventsFrom(ParserFor("09-flow-mapping.yaml"),
				StreamStart,
				DocumentStart(Implicit),
				FlowMappingStart,
				PlainScalar("a simple key").ExplicitQuoted,
				PlainScalar("a value").ExplicitQuoted,
				PlainScalar("a complex key").ExplicitQuoted,
				PlainScalar("another value").ExplicitQuoted,
				MappingEnd,
				DocumentEnd(Implicit),
				StreamEnd);
		}

		[Fact]
		public void VerifyTokensOnExample10()
		{
			AssertSequenceOfEventsFrom(ParserFor("10-mixed-nodes-in-sequence.yaml"),
				StreamStart,
				DocumentStart(Implicit),
				BlockSequenceStart,
				PlainScalar("item 1").ExplicitQuoted,
				PlainScalar("item 2").ExplicitQuoted,
				BlockSequenceStart,
				PlainScalar("item 3.1").ExplicitQuoted,
				PlainScalar("item 3.2").ExplicitQuoted,
				SequenceEnd,
				BlockMappingStart,
				PlainScalar("key 1").ExplicitQuoted,
				PlainScalar("value 1").ExplicitQuoted,
				PlainScalar("key 2").ExplicitQuoted,
				PlainScalar("value 2").ExplicitQuoted,
				MappingEnd,
				SequenceEnd,
				DocumentEnd(Implicit),
				StreamEnd);
		}

		[Fact]
		public void VerifyTokensOnExample11()
		{
			AssertSequenceOfEventsFrom(ParserFor("11-mixed-nodes-in-mapping.yaml"),
				StreamStart,
				DocumentStart(Implicit),
				BlockMappingStart,
				PlainScalar("a simple key").ExplicitQuoted,
				PlainScalar("a value").ExplicitQuoted,
				PlainScalar("a complex key").ExplicitQuoted,
				PlainScalar("another value").ExplicitQuoted,
				PlainScalar("a mapping").ExplicitQuoted,
				BlockMappingStart,
				PlainScalar("key 1").ExplicitQuoted,
				PlainScalar("value 1").ExplicitQuoted,
				PlainScalar("key 2").ExplicitQuoted,
				PlainScalar("value 2").ExplicitQuoted,
				MappingEnd,
				PlainScalar("a sequence").ExplicitQuoted,
				BlockSequenceStart,
				PlainScalar("item 1").ExplicitQuoted,
				PlainScalar("item 2").ExplicitQuoted,
				SequenceEnd,
				MappingEnd,
				DocumentEnd(Implicit),
				StreamEnd);
		}

		[Fact]
		public void VerifyTokensOnExample12()
		{
			AssertSequenceOfEventsFrom(ParserFor("12-compact-sequence.yaml"),
				StreamStart,
				DocumentStart(Implicit),
				BlockSequenceStart,
				BlockSequenceStart,
				PlainScalar("item 1").ExplicitQuoted,
				PlainScalar("item 2").ExplicitQuoted,
				SequenceEnd,
				BlockMappingStart,
				PlainScalar("key 1").ExplicitQuoted,
				PlainScalar("value 1").ExplicitQuoted,
				PlainScalar("key 2").ExplicitQuoted,
				PlainScalar("value 2").ExplicitQuoted,
				MappingEnd,
				BlockMappingStart,
				PlainScalar("complex key").ExplicitQuoted,
				PlainScalar("complex value").ExplicitQuoted,
				MappingEnd,
				SequenceEnd,
				DocumentEnd(Implicit),
				StreamEnd);
		}

		[Fact]
		public void VerifyTokensOnExample13()
		{
			AssertSequenceOfEventsFrom(ParserFor("13-compact-mapping.yaml"),
				StreamStart,
				DocumentStart(Implicit),
				BlockMappingStart,
				PlainScalar("a sequence").ExplicitQuoted,
				BlockSequenceStart,
				PlainScalar("item 1").ExplicitQuoted,
				PlainScalar("item 2").ExplicitQuoted,
				SequenceEnd,
				PlainScalar("a mapping").ExplicitQuoted,
				BlockMappingStart,
				PlainScalar("key 1").ExplicitQuoted,
				PlainScalar("value 1").ExplicitQuoted,
				PlainScalar("key 2").ExplicitQuoted,
				PlainScalar("value 2").ExplicitQuoted,
				MappingEnd,
				MappingEnd,
				DocumentEnd(Implicit),
				StreamEnd);
		}

		[Fact]
		public void VerifyTokensOnExample14()
		{
			AssertSequenceOfEventsFrom(ParserFor("14-mapping-wo-indent.yaml"),
				StreamStart,
				DocumentStart(Implicit),
				BlockMappingStart,
				PlainScalar("key").ExplicitQuoted,
				BlockSequenceStart,
				PlainScalar("item 1").ExplicitQuoted,
				PlainScalar("item 2").ExplicitQuoted,
				SequenceEnd,
				MappingEnd,
				DocumentEnd(Implicit),
				StreamEnd);
		}

		[Fact]
		public void VerifyTokenWithLocalTags()
		{
			AssertSequenceOfEventsFrom(ParserFor("local-tags.yaml"),
				StreamStart,
				DocumentStart(Explicit),
				BlockMappingStart.T("!MyObject").Explicit,
				PlainScalar("a").ExplicitQuoted,
				PlainScalar("1.0").ExplicitQuoted,
				PlainScalar("b").ExplicitQuoted,
				PlainScalar("42").ExplicitQuoted,
				PlainScalar("c").ExplicitQuoted,
				PlainScalar("-7").ExplicitQuoted,
				MappingEnd,
				DocumentEnd(Implicit),
				StreamEnd);
		}

		[Fact]
		public void LineAndColumnNumbersAreCorrectlyCalculated()
		{
			var reader = new EventReader(ParserFor("list.yaml"));
			reader.Expect<StreamStart>();
			reader.Expect<DocumentStart>();
			var sut = reader.Expect<SequenceStart>();

			sut.Start.Line.Should().Be(1, "The sequence should start on line 1");
			sut.Start.Column.Should().Be(1, "The sequence should start on column 1");
		}

		private IParser ParserForEmptyContent()
		{
			return new Parser(new StringReader(string.Empty));
		}

		private IParser ParserFor(string name)
		{
			return new Parser(Yaml.StreamFrom(name));
		}

		private void AssertSequenceOfEventsFrom(IParser parser, params ParsingEvent[] events)
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

		private void AssertEvent(ParsingEvent expected, ParsingEvent actual, int eventNumber)
		{
			actual.GetType().Should().Be(expected.GetType(), "Parse event {0} is not of the expected type.", eventNumber);

			foreach (var property in expected.GetType().GetProperties())
			{
				if (property.PropertyType == typeof(Mark) || !property.CanRead)
				{
					continue;
				}

				var value = property.GetValue(actual, null);
				var expectedValue = property.GetValue(expected, null);
				if (expectedValue is IEnumerable && !(expectedValue is string))
				{
					Dump.Write("\t{0} = {{", property.Name);
					Dump.Write(string.Join(", ", (IEnumerable)value));
					Dump.WriteLine("}");

					if (expectedValue is ICollection && value is ICollection)
					{
						var expectedCount = ((ICollection)expectedValue).Count;
						var valueCount = ((ICollection)value).Count;
						valueCount.Should().Be(expectedCount, "Compared size of collections in property {0} in parse event {1}",
							property.Name, eventNumber);
					}

					var values = ((IEnumerable)value).GetEnumerator();
					var expectedValues = ((IEnumerable)expectedValue).GetEnumerator();
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
					Dump.WriteLine("\t{0} = {1}", property.Name, value);
					value.Should().Be(expectedValue, "Compared property {0} in parse event {1}", property.Name, eventNumber);
				}
			}
		}
	}
}