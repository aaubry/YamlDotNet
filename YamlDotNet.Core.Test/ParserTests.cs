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
using YamlDotNet.Core.Events;

namespace YamlDotNet.Core.Test
{
	public class ParserTests : ParserTestHelper
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
			AssertSequenceOfEventsFrom(ParserFor("test1.yaml"),
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
			AssertSequenceOfEventsFrom(ParserFor("test2.yaml"),
				StreamStart,
				DocumentStart(Implicit),
				SingleQuotedScalar("a scalar"),
				DocumentEnd(Implicit),
				StreamEnd);
		}

		[Fact]
		public void VerifyTokensOnExample3()
		{
			AssertSequenceOfEventsFrom(ParserFor("test3.yaml"),
				StreamStart,
				DocumentStart(Explicit),
				SingleQuotedScalar("a scalar"),
				DocumentEnd(Explicit),
				StreamEnd);
		}

		[Fact]
		public void VerifyTokensOnExample4()
		{
			AssertSequenceOfEventsFrom(ParserFor("test4.yaml"),
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
			AssertSequenceOfEventsFrom(ParserFor("test5.yaml"),
				StreamStart,
				DocumentStart(Implicit),
				AnchoredFlowSequenceStart("A"),
				AnchorAlias("A"),
				SequenceEnd,
				DocumentEnd(Implicit),
				StreamEnd);
		}

		[Fact]
		public void VerifyTokensOnExample6()
		{
			var parser = ParserFor("test6.yaml");
			AssertSequenceOfEventsFrom(parser,
				StreamStart,
				DocumentStart(Implicit),
				ExplicitDoubleQuotedScalar(TagYaml + "float", "3.14"),
				DocumentEnd(Implicit),
				StreamEnd);
		}

		[Fact]
		public void VerifyTokensOnExample7()
		{
			AssertSequenceOfEventsFrom(ParserFor("test7.yaml"),
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
			AssertSequenceOfEventsFrom(ParserFor("test8.yaml"),
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
			AssertSequenceOfEventsFrom(ParserFor("test9.yaml"),
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
			AssertSequenceOfEventsFrom(ParserFor("test10.yaml"),
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
			AssertSequenceOfEventsFrom(ParserFor("test11.yaml"),
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
			AssertSequenceOfEventsFrom(ParserFor("test12.yaml"),
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
			AssertSequenceOfEventsFrom(ParserFor("test13.yaml"),
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
			AssertSequenceOfEventsFrom(ParserFor("test14.yaml"),
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
			AssertSequenceOfEventsFrom(ParserFor("local-tags.yaml"),
				StreamStart,
				DocumentStart(Explicit),
				TaggedBlockMappingStart("!MyObject"),
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