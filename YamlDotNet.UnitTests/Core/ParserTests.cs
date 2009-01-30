using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using VersionDirective = YamlDotNet.Core.Tokens.VersionDirective;
using TagDirective = YamlDotNet.Core.Tokens.TagDirective;

namespace YamlDotNet.UnitTests
{
	[TestFixture]
	public class ParserTests : YamlTest
	{	
		private static Parser CreateParser(string name) {
			return new Parser(YamlFile(name));
		}
			
		private void AssertHasNext(Parser parser) {
			Assert.IsTrue(parser.MoveNext(), "The parser does not contain more events.");
		}
		
		private void AssertDoesNotHaveNext(Parser parser) {
			Assert.IsFalse(parser.MoveNext(), "The parser should not contain more events.");
		}
	
		private void AssertCurrent(Parser parser, ParsingEvent expected) {
			Console.WriteLine(expected.GetType().Name);
			Assert.IsTrue(expected.GetType().IsAssignableFrom(parser.Current.GetType()), "The event is not of the expected type.");
			
			foreach (var property in expected.GetType().GetProperties()) {
				if(property.PropertyType != typeof(Mark) && property.CanRead) {
					object value = property.GetValue(parser.Current, null);
					object expectedValue = property.GetValue(expected, null);
					if(expectedValue != null && Type.GetTypeCode(expectedValue.GetType()) == TypeCode.Object && expectedValue is IEnumerable) {
						Console.Write("\t{0} = {{", property.Name);
						bool isFirst = true;
						foreach(var item in (IEnumerable)value) {
							if(isFirst) {
								isFirst = false;
							} else {
								Console.Write(", ");
							}
							Console.Write(item);
						}
						Console.WriteLine("}");
						
						if(expectedValue is ICollection && value is ICollection) {
							Assert.AreEqual(((ICollection)expectedValue).Count, ((ICollection)value).Count, "The collection does not contain the correct number of items.");
						}
						
						IEnumerator values = ((IEnumerable)value).GetEnumerator();
						IEnumerator expectedValues = ((IEnumerable)expectedValue).GetEnumerator();
						while(expectedValues.MoveNext()) {
							Assert.IsTrue(values.MoveNext(), "The property does not contain enough items.");
							Assert.AreEqual(expectedValues.Current, values.Current, string.Format("The property '{0}' is incorrect.", property.Name));
						}
						
						Assert.IsFalse(values.MoveNext(), "The property contains too many items.");
					} else {
						Console.WriteLine("\t{0} = {1}", property.Name, value);
						Assert.AreEqual(expectedValue, value, string.Format("The property '{0}' is incorrect.", property.Name));
					}
				}
			}
		}
	
		private void AssertNext(Parser parser, ParsingEvent expected) {
			AssertHasNext(parser);
			AssertCurrent(parser, expected);
		}

		[Test]
		public void EmptyDocument()
		{
			Parser parser = CreateParser("empty.yaml");

			AssertNext(parser, new StreamStart());
			AssertNext(parser, new StreamEnd());
			AssertDoesNotHaveNext(parser);
		}
		
		private static TagDirectiveCollection defaultDirectives = new TagDirectiveCollection(new TagDirective[] {
			new TagDirective("!", "!"),
			new TagDirective("!!", "tag:yaml.org,2002:"),
		});
		
		[Test]
		public void VerifyEventsOnExample1()
		{
			Parser parser = CreateParser("test1.yaml");
			
			AssertNext(parser, new StreamStart());
			AssertNext(parser, new DocumentStart(new VersionDirective(new Core.Version(1, 1)), new TagDirectiveCollection(new TagDirective[] {
				new TagDirective("!", "!foo"),
				new TagDirective("!yaml!", "tag:yaml.org,2002:"),
				new TagDirective("!!", "tag:yaml.org,2002:"),
			}), false));
			AssertNext(parser, new Scalar(null, null, string.Empty, ScalarStyle.Plain, true, false));
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new StreamEnd());
			AssertDoesNotHaveNext(parser);
		}
		
		[Test]
		public void VerifyTokensOnExample2()
		{
			Parser parser = CreateParser("test2.yaml");
			
			AssertNext(parser, new StreamStart());
			AssertNext(parser, new DocumentStart(null, defaultDirectives, true));
			AssertNext(parser, new Scalar(null, null, "a scalar", ScalarStyle.SingleQuoted, false, true));
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new StreamEnd());
			AssertDoesNotHaveNext(parser);
		}
		
		[Test]
		public void VerifyTokensOnExample3()
		{
			Parser parser = CreateParser("test3.yaml");
			
			AssertNext(parser, new StreamStart());
			AssertNext(parser, new DocumentStart(null, defaultDirectives, false));
			AssertNext(parser, new Scalar(null, null, "a scalar", ScalarStyle.SingleQuoted, false, true));
			AssertNext(parser, new DocumentEnd(false));
			AssertNext(parser, new StreamEnd());
			AssertDoesNotHaveNext(parser);
		}		
 		
		[Test]
		public void VerifyTokensOnExample4()
		{
			Parser parser = CreateParser("test4.yaml");
			
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
 		
		[Test]
		public void VerifyTokensOnExample5()
		{
			Parser parser = CreateParser("test5.yaml");
			
			AssertNext(parser, new StreamStart());
			AssertNext(parser, new DocumentStart(null, defaultDirectives, true));
			AssertNext(parser, new SequenceStart("A", null, true, SequenceStyle.Flow));
			AssertNext(parser, new AnchorAlias("A"));
			AssertNext(parser, new SequenceEnd());
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new StreamEnd());
			AssertDoesNotHaveNext(parser);
		}
 		
		[Test]
		public void VerifyTokensOnExample6()
		{
			Parser parser = CreateParser("test6.yaml");
			
			AssertNext(parser, new StreamStart());
			AssertNext(parser, new DocumentStart(null, defaultDirectives, true));
			AssertNext(parser, new Scalar(null, "tag:yaml.org,2002:float", "3.14", ScalarStyle.DoubleQuoted, false, false));
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new StreamEnd());
			AssertDoesNotHaveNext(parser);
		}
		
		[Test]
		public void VerifyTokensOnExample7()
		{
			Parser parser = CreateParser("test7.yaml");
			
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
		
		[Test]
		public void VerifyTokensOnExample8()
		{
			Parser parser = CreateParser("test8.yaml");
			
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

		[Test]
		public void VerifyTokensOnExample9()
		{
			Parser parser = CreateParser("test9.yaml");
			
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

		[Test]
		public void VerifyTokensOnExample10()
		{
			Parser parser = CreateParser("test10.yaml");
			
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
	
		[Test]
		public void VerifyTokensOnExample11()
		{
			Parser parser = CreateParser("test11.yaml");
			
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
	
		[Test]
		public void VerifyTokensOnExample12()
		{
			Parser parser = CreateParser("test12.yaml");
			
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
			
		[Test]
		public void VerifyTokensOnExample13()
		{
			Parser parser = CreateParser("test13.yaml");
			
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
			
		[Test]
		public void VerifyTokensOnExample14()
		{
			Parser parser = CreateParser("test14.yaml");
			
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
	}
}