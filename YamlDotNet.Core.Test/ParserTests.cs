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

using System;
using System.Collections;
using Xunit;
using YamlDotNet.Events;
using VersionDirective = YamlDotNet.Tokens.VersionDirective;
using TagDirective = YamlDotNet.Tokens.TagDirective;

namespace YamlDotNet.Test
{
	public class ParserTests : YamlTest
	{
		private static TagDirectiveCollection defaultDirectives = new TagDirectiveCollection(new[] {
			new TagDirective("!", "!"),
			new TagDirective("!!", "tag:yaml.org,2002:")
		});

		[Fact]
		public void EmptyDocument()
		{
			var parser = CreateParser("empty.yaml");

			AssertNext(parser, new StreamStart());
			AssertNext(parser, new StreamEnd());
			AssertDoesNotHaveNext(parser);
		}

		[Fact]
		public void VerifyEventsOnExample1()
		{
			var parser = CreateParser("test1.yaml");
			
			AssertNext(parser, new StreamStart());
			AssertNext(parser, new DocumentStart(new VersionDirective(new Version(1, 1)), new TagDirectiveCollection(new[] {
				new TagDirective("!", "!foo"),
				new TagDirective("!yaml!", "tag:yaml.org,2002:"),
				new TagDirective("!!", "tag:yaml.org,2002:")
			}), false));
			AssertNext(parser, new Scalar(null, null, string.Empty, ScalarStyle.Plain, true, false));
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new StreamEnd());
			AssertDoesNotHaveNext(parser);
		}
		
		[Fact]
		public void VerifyTokensOnExample2()
		{
			var parser = CreateParser("test2.yaml");
			
			AssertNext(parser, new StreamStart());
			AssertNext(parser, new DocumentStart(null, defaultDirectives, true));
			AssertNext(parser, new Scalar(null, null, "a scalar", ScalarStyle.SingleQuoted, false, true));
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new StreamEnd());
			AssertDoesNotHaveNext(parser);
		}
		
		[Fact]
		public void VerifyTokensOnExample3()
		{
			var parser = CreateParser("test3.yaml");
			
			AssertNext(parser, new StreamStart());
			AssertNext(parser, new DocumentStart(null, defaultDirectives, false));
			AssertNext(parser, new Scalar(null, null, "a scalar", ScalarStyle.SingleQuoted, false, true));
			AssertNext(parser, new DocumentEnd(false));
			AssertNext(parser, new StreamEnd());
			AssertDoesNotHaveNext(parser);
		}		
		
		[Fact]
		public void VerifyTokensOnExample4()
		{
			var parser = CreateParser("test4.yaml");
			
			AssertNext(parser, new StreamStart());
			AssertNext(parser, new DocumentStart(null, defaultDirectives, true));
			AssertNext(parser, new Scalar(null, null, "a scalar", ScalarStyle.SingleQuoted, false, true));
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new DocumentStart(null, defaultDirectives, false));
			AssertNext(parser, new Scalar(null, null, "another scalar", ScalarStyle.SingleQuoted, false, true));
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new DocumentStart(null, defaultDirectives, false));
			AssertNext(parser, new Scalar(null, null, "yet another scalar", ScalarStyle.SingleQuoted, false, true));
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new StreamEnd());
			AssertDoesNotHaveNext(parser);
		}		
		
		[Fact]
		public void VerifyTokensOnExample5()
		{
			var parser = CreateParser("test5.yaml");
			
			AssertNext(parser, new StreamStart());
			AssertNext(parser, new DocumentStart(null, defaultDirectives, true));
			AssertNext(parser, new SequenceStart("A", null, true, SequenceStyle.Flow));
			AssertNext(parser, new AnchorAlias("A"));
			AssertNext(parser, new SequenceEnd());
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new StreamEnd());
			AssertDoesNotHaveNext(parser);
		}
		
		[Fact]
		public void VerifyTokensOnExample6()
		{
			var parser = CreateParser("test6.yaml");
			
			AssertNext(parser, new StreamStart());
			AssertNext(parser, new DocumentStart(null, defaultDirectives, true));
			AssertNext(parser, new Scalar(null, "tag:yaml.org,2002:float", "3.14", ScalarStyle.DoubleQuoted, false, false));
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new StreamEnd());
			AssertDoesNotHaveNext(parser);
		}
		
		[Fact]
		public void VerifyTokensOnExample7()
		{
			var parser = CreateParser("test7.yaml");
			
			AssertNext(parser, new StreamStart());
			AssertNext(parser, new DocumentStart(null, defaultDirectives, false));
			AssertNext(parser, new Scalar(null, null, "", ScalarStyle.Plain, true, false));
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new DocumentStart(null, defaultDirectives, false));
			AssertNext(parser, new Scalar(null, null, "a plain scalar", ScalarStyle.Plain, true, false));
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new DocumentStart(null, defaultDirectives, false));
			AssertNext(parser, new Scalar(null, null, "a single-quoted scalar", ScalarStyle.SingleQuoted, false, true));
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new DocumentStart(null, defaultDirectives, false));
			AssertNext(parser, new Scalar(null, null, "a double-quoted scalar", ScalarStyle.DoubleQuoted, false, true));
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new DocumentStart(null, defaultDirectives, false));
			AssertNext(parser, new Scalar(null, null, "a literal scalar", ScalarStyle.Literal, false, true));
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new DocumentStart(null, defaultDirectives, false));
			AssertNext(parser, new Scalar(null, null, "a folded scalar", ScalarStyle.Folded, false, true));
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new StreamEnd());
			AssertDoesNotHaveNext(parser);
		}
		
		[Fact]
		public void VerifyTokensOnExample8()
		{
			var parser = CreateParser("test8.yaml");
			
			AssertNext(parser, new StreamStart());
			AssertNext(parser, new DocumentStart(null, defaultDirectives, true));
			AssertNext(parser, new SequenceStart(null, null, true, SequenceStyle.Flow));
			AssertNext(parser, new Scalar(null, null, "item 1", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "item 2", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "item 3", ScalarStyle.Plain, true, false));
			AssertNext(parser, new SequenceEnd());
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new StreamEnd());
			AssertDoesNotHaveNext(parser);
		}

		[Fact]
		public void VerifyTokensOnExample9()
		{
			var parser = CreateParser("test9.yaml");
			
			AssertNext(parser, new StreamStart());
			AssertNext(parser, new DocumentStart(null, defaultDirectives, true));
			AssertNext(parser, new MappingStart(null, null, true, MappingStyle.Flow));
			AssertNext(parser, new Scalar(null, null, "a simple key", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "a value", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "a complex key", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "another value", ScalarStyle.Plain, true, false));
			AssertNext(parser, new MappingEnd());
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new StreamEnd());
			AssertDoesNotHaveNext(parser);
		}

		[Fact]
		public void VerifyTokensOnExample10()
		{
			var parser = CreateParser("test10.yaml");
			
			AssertNext(parser, new StreamStart());
			AssertNext(parser, new DocumentStart(null, defaultDirectives, true));
			AssertNext(parser, new SequenceStart(null, null, true, SequenceStyle.Block));
			AssertNext(parser, new Scalar(null, null, "item 1", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "item 2", ScalarStyle.Plain, true, false));
			AssertNext(parser, new SequenceStart(null, null, true, SequenceStyle.Block));
			AssertNext(parser, new Scalar(null, null, "item 3.1", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "item 3.2", ScalarStyle.Plain, true, false));
			AssertNext(parser, new SequenceEnd());
			AssertNext(parser, new MappingStart(null, null, true, MappingStyle.Block));
			AssertNext(parser, new Scalar(null, null, "key 1", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "value 1", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "key 2", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "value 2", ScalarStyle.Plain, true, false));
			AssertNext(parser, new MappingEnd());
			AssertNext(parser, new SequenceEnd());
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new StreamEnd());
			AssertDoesNotHaveNext(parser);
		}
	
		[Fact]
		public void VerifyTokensOnExample11()
		{
			var parser = CreateParser("test11.yaml");
			
			AssertNext(parser, new StreamStart());
			AssertNext(parser, new DocumentStart(null, defaultDirectives, true));
			AssertNext(parser, new MappingStart(null, null, true, MappingStyle.Block));
			AssertNext(parser, new Scalar(null, null, "a simple key", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "a value", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "a complex key", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "another value", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "a mapping", ScalarStyle.Plain, true, false));
			AssertNext(parser, new MappingStart(null, null, true, MappingStyle.Block));
			AssertNext(parser, new Scalar(null, null, "key 1", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "value 1", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "key 2", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "value 2", ScalarStyle.Plain, true, false));
			AssertNext(parser, new MappingEnd());
			AssertNext(parser, new Scalar(null, null, "a sequence", ScalarStyle.Plain, true, false));
			AssertNext(parser, new SequenceStart(null, null, true, SequenceStyle.Block));
			AssertNext(parser, new Scalar(null, null, "item 1", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "item 2", ScalarStyle.Plain, true, false));
			AssertNext(parser, new SequenceEnd());
			AssertNext(parser, new MappingEnd());
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new StreamEnd());
			AssertDoesNotHaveNext(parser);
		}
	
		[Fact]
		public void VerifyTokensOnExample12()
		{
			var parser = CreateParser("test12.yaml");
			
			AssertNext(parser, new StreamStart());
			AssertNext(parser, new DocumentStart(null, defaultDirectives, true));
			AssertNext(parser, new SequenceStart(null, null, true, SequenceStyle.Block));
			AssertNext(parser, new SequenceStart(null, null, true, SequenceStyle.Block));
			AssertNext(parser, new Scalar(null, null, "item 1", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "item 2", ScalarStyle.Plain, true, false));
			AssertNext(parser, new SequenceEnd());
			AssertNext(parser, new MappingStart(null, null, true, MappingStyle.Block));
			AssertNext(parser, new Scalar(null, null, "key 1", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "value 1", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "key 2", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "value 2", ScalarStyle.Plain, true, false));
			AssertNext(parser, new MappingEnd());
			AssertNext(parser, new MappingStart(null, null, true, MappingStyle.Block));
			AssertNext(parser, new Scalar(null, null, "complex key", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "complex value", ScalarStyle.Plain, true, false));
			AssertNext(parser, new MappingEnd());
			AssertNext(parser, new SequenceEnd());
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new StreamEnd());
			AssertDoesNotHaveNext(parser);
		}
			
		[Fact]
		public void VerifyTokensOnExample13()
		{
			var parser = CreateParser("test13.yaml");
			
			AssertNext(parser, new StreamStart());
			AssertNext(parser, new DocumentStart(null, defaultDirectives, true));
			AssertNext(parser, new MappingStart(null, null, true, MappingStyle.Block));
			AssertNext(parser, new Scalar(null, null, "a sequence", ScalarStyle.Plain, true, false));
			AssertNext(parser, new SequenceStart(null, null, true, SequenceStyle.Block));
			AssertNext(parser, new Scalar(null, null, "item 1", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "item 2", ScalarStyle.Plain, true, false));
			AssertNext(parser, new SequenceEnd());
			AssertNext(parser, new Scalar(null, null, "a mapping", ScalarStyle.Plain, true, false));
			AssertNext(parser, new MappingStart(null, null, true, MappingStyle.Block));
			AssertNext(parser, new Scalar(null, null, "key 1", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "value 1", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "key 2", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "value 2", ScalarStyle.Plain, true, false));
			AssertNext(parser, new MappingEnd());
			AssertNext(parser, new MappingEnd());
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new StreamEnd());
			AssertDoesNotHaveNext(parser);
		}
			
		[Fact]
		public void VerifyTokensOnExample14()
		{
			var parser = CreateParser("test14.yaml");
			
			AssertNext(parser, new StreamStart());
			AssertNext(parser, new DocumentStart(null, defaultDirectives, true));
			AssertNext(parser, new MappingStart(null, null, true, MappingStyle.Block));
			AssertNext(parser, new Scalar(null, null, "key", ScalarStyle.Plain, true, false));
			AssertNext(parser, new SequenceStart(null, null, true, SequenceStyle.Block));
			AssertNext(parser, new Scalar(null, null, "item 1", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "item 2", ScalarStyle.Plain, true, false));
			AssertNext(parser, new SequenceEnd());
			AssertNext(parser, new MappingEnd());
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new StreamEnd());
			AssertDoesNotHaveNext(parser);
		}

		[Fact]
		public void VerifyTokenWithLocalTags()
		{
			var parser = CreateParser("local-tags.yaml");

			AssertNext(parser, new StreamStart());
			AssertNext(parser, new DocumentStart(null, defaultDirectives, false));
			AssertNext(parser, new MappingStart(null, "!MyObject", false, MappingStyle.Block));
			AssertNext(parser, new Scalar(null, null, "a", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "1.0", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "b", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "42", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "c", ScalarStyle.Plain, true, false));
			AssertNext(parser, new Scalar(null, null, "-7", ScalarStyle.Plain, true, false));
			AssertNext(parser, new MappingEnd());
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new StreamEnd());
			AssertDoesNotHaveNext(parser);
		}

		private IParser CreateParser(string name)
		{
			return new Parser(YamlFile(name));
		}

		private void AssertNext(IParser parser, ParsingEvent expected) {
			AssertHasNext(parser);
			AssertCurrent(parser, expected);
		}

		private void AssertHasNext(IParser parser) {
			Assert.True(parser.MoveNext());
		}

		private void AssertDoesNotHaveNext(IParser parser) {
			Assert.False(parser.MoveNext());
		}

		private void AssertCurrent(IParser parser, ParsingEvent expected) {
			Dump.WriteLine(expected.GetType().Name);
			Assert.True(expected.GetType().IsAssignableFrom(parser.Current.GetType()),
				string.Format("The event is not of the expected type. Exprected: {0}, Actual: {1}", expected.GetType().Name, parser.Current.GetType().Name));

			foreach (var property in expected.GetType().GetProperties()) {
				if (property.PropertyType != typeof(Mark) && property.CanRead) {
					var value = property.GetValue(parser.Current, null);
					var expectedValue = property.GetValue(expected, null);
					// Todo: what does GetTypeCode do and is it necessary?
					if (expectedValue != null && Type.GetTypeCode(expectedValue.GetType()) == TypeCode.Object && expectedValue is IEnumerable) {
						Dump.Write("\t{0} = {{", property.Name);
						var isFirst = true;
						foreach (var item in (IEnumerable)value) {
							if (isFirst) {
								isFirst = false;
							} else {
								Dump.Write(", ");
							}
							Dump.Write(item);
						}
						Dump.WriteLine("}");

						if (expectedValue is ICollection && value is ICollection) {
							Assert.Equal(((ICollection)expectedValue).Count, ((ICollection)value).Count);
						}

						var values = ((IEnumerable)value).GetEnumerator();
						var expectedValues = ((IEnumerable)expectedValue).GetEnumerator();
						while (expectedValues.MoveNext()) {
							Assert.True(values.MoveNext());
							Assert.Equal(expectedValues.Current, values.Current);
						}

						Assert.False(values.MoveNext());
					} else {
						Dump.WriteLine("\t{0} = {1}", property.Name, value);
						Assert.Equal(expectedValue, value);
					}
				}
			}
		}
	}
}