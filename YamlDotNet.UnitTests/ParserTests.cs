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
		private static Parser CreateParser(string content) {
			return new Parser(YamlFile(content));
		}
			
		private void AssertHasNext(Parser parser) {
			Assert.IsTrue(parser.MoveNext(), "The parser does not contain more events.");
		}
		
		private void AssertDoesNotHaveNext(Parser parser) {
			Assert.IsFalse(parser.MoveNext(), "The parser should not contain more events.");
		}
	
		private void AssertCurrent(Parser parser, ParsingEvent expected) {
			Console.WriteLine(expected.GetType().Name);
			Assert.IsInstanceOfType(expected.GetType(), parser.Current, "The event is not of the expected type.");
			
			foreach (PropertyInfo property in expected.GetType().GetProperties()) {
				if(property.PropertyType != typeof(Mark) && property.CanRead) {
					object value = property.GetValue(parser.Current, null);
					object expectedValue = property.GetValue(expected, null);
					if(expectedValue != null && Type.GetTypeCode(expectedValue.GetType()) == TypeCode.Object && expectedValue is IEnumerable) {
						Console.Write("\t{0} = {{", property.Name);
						bool isFirst = true;
						foreach(object item in (IEnumerable)value) {
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
			Parser parser = CreateParser(@"");

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
			Parser parser = CreateParser(@"
				%YAML   1.1
				%TAG    !   !foo
				%TAG    !yaml!  tag:yaml.org,2002:
				---
			");
			
			AssertNext(parser, new StreamStart());
			AssertNext(parser, new DocumentStart(new VersionDirective(new Core.Version(1, 1)), new TagDirectiveCollection(new TagDirective[] {
				new TagDirective("!", "!foo"),
				new TagDirective("!yaml!", "tag:yaml.org,2002:"),
				new TagDirective("!!", "tag:yaml.org,2002:"),
			}), false));
			AssertNext(parser, new Scalar(string.Empty, string.Empty, string.Empty, ScalarStyle.Plain, true, false));
			AssertNext(parser, new DocumentEnd(true));
			AssertNext(parser, new StreamEnd());
			AssertDoesNotHaveNext(parser);
		}
		
		[Test]
		public void VerifyTokensOnExample2()
		{
			Parser parser = CreateParser(@"
				'a scalar'
			");
			
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
			Parser parser = CreateParser(@"
				---
				'a scalar'
				...
			");
			
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
			Parser parser = CreateParser(@"
				'a scalar'
				---
				'another scalar'
				---
				'yet another scalar'
			");
			
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
			Parser parser = CreateParser(@"
				&A [ *A ]
			");
			
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
			Parser parser = CreateParser(@"
				!!float ""3.14""  # A good approximation.
			");
			
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
			Parser parser = CreateParser(@"
				--- # Implicit empty plain scalars do not produce tokens.
				--- a plain scalar
				--- 'a single-quoted scalar'
				--- ""a double-quoted scalar""
				--- |-
				  a literal scalar
				--- >-
				  a folded
				  scalar
			");
			
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
			Parser parser = CreateParser(@"
				[item 1, item 2, item 3]
			");
			
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
			Parser parser = CreateParser(@"
				{
					a simple key: a value,  # Note that the KEY token is produced.
					? a complex key: another value,
				}
			");
			
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
			Parser parser = CreateParser(@"
				- item 1
				- item 2
				-
				  - item 3.1
				  - item 3.2
				-
				  key 1: value 1
				  key 2: value 2
			");
			
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
		[Ignore("The original C code also fails this test")]
		public void VerifyTokensOnExample11()
		{
			Parser parser = CreateParser(@"
				a simple key: a value   # The KEY token is produced here.
				? a complex key
				: another value
				a mapping:
				  key 1: value 1
				  key 2: value 2
				a sequence:
				  - item 1
				  - item 2
			");
			
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
//	
//		[Test]
//		public void VerifyTokensOnExample12()
//		{
//			Parser parser = CreateParser(@"
//				- - item 1
//				  - item 2
//				- key 1: value 1
//				  key 2: value 2
//				- ? complex key
//				  : complex value
//			");
//			
//			AssertNext(parser, new StreamStart());
//			AssertNext(parser, new BlockSequenceStart());
//			AssertNext(parser, new BlockEntry());
//			AssertNext(parser, new BlockSequenceStart());
//			AssertNext(parser, new BlockEntry());
//			AssertNext(parser, new Scalar("item 1", ScalarStyle.Plain));
//			AssertNext(parser, new BlockEntry());
//			AssertNext(parser, new Scalar("item 2", ScalarStyle.Plain));
//			AssertNext(parser, new BlockEnd());
//			AssertNext(parser, new BlockEntry());
//			AssertNext(parser, new BlockMappingStart());
//			AssertNext(parser, new Key());
//			AssertNext(parser, new Scalar("key 1", ScalarStyle.Plain));
//			AssertNext(parser, new Value());
//			AssertNext(parser, new Scalar("value 1", ScalarStyle.Plain));
//			AssertNext(parser, new Key());
//			AssertNext(parser, new Scalar("key 2", ScalarStyle.Plain));
//			AssertNext(parser, new Value());
//			AssertNext(parser, new Scalar("value 2", ScalarStyle.Plain));
//			AssertNext(parser, new BlockEnd());
//			AssertNext(parser, new BlockEntry());
//			AssertNext(parser, new BlockMappingStart());
//			AssertNext(parser, new Key());
//			AssertNext(parser, new Scalar("complex key", ScalarStyle.Plain));
//			AssertNext(parser, new Value());
//			AssertNext(parser, new Scalar("complex value", ScalarStyle.Plain));
//			AssertNext(parser, new BlockEnd());
//			AssertNext(parser, new BlockEnd());
//			AssertNext(parser, new StreamEnd());
//			AssertDoesNotHaveNext(parser);
//		}
//			
//		[Test]
//		public void VerifyTokensOnExample13()
//		{
//			Parser parser = CreateParser(@"
//				? a sequence
//				: - item 1
//				  - item 2
//				? a mapping
//				: key 1: value 1
//				  key 2: value 2
//			");
//			
//			AssertNext(parser, new StreamStart());
//			AssertNext(parser, new BlockMappingStart());
//			AssertNext(parser, new Key());
//			AssertNext(parser, new Scalar("a sequence", ScalarStyle.Plain));
//			AssertNext(parser, new Value());
//			AssertNext(parser, new BlockSequenceStart());
//			AssertNext(parser, new BlockEntry());
//			AssertNext(parser, new Scalar("item 1", ScalarStyle.Plain));
//			AssertNext(parser, new BlockEntry());
//			AssertNext(parser, new Scalar("item 2", ScalarStyle.Plain));
//			AssertNext(parser, new BlockEnd());
//			AssertNext(parser, new Key());
//			AssertNext(parser, new Scalar("a mapping", ScalarStyle.Plain));
//			AssertNext(parser, new Value());
//			AssertNext(parser, new BlockMappingStart());
//			AssertNext(parser, new Key());
//			AssertNext(parser, new Scalar("key 1", ScalarStyle.Plain));
//			AssertNext(parser, new Value());
//			AssertNext(parser, new Scalar("value 1", ScalarStyle.Plain));
//			AssertNext(parser, new Key());
//			AssertNext(parser, new Scalar("key 2", ScalarStyle.Plain));
//			AssertNext(parser, new Value());
//			AssertNext(parser, new Scalar("value 2", ScalarStyle.Plain));
//			AssertNext(parser, new BlockEnd());
//			AssertNext(parser, new BlockEnd());
//			AssertNext(parser, new StreamEnd());
//			AssertDoesNotHaveNext(parser);
//		}
//			
//		[Test]
//		[Ignore("The original C code also fails this test")]
//		public void VerifyTokensOnExample14()
//		{
//			Parser parser = CreateParser(@"
//				key:
//				- item 1    # BLOCK-SEQUENCE-START is NOT produced here.
//				- item 2
//			");
//			
//			AssertNext(parser, new StreamStart());
//			AssertNext(parser, new BlockMappingStart());
//			AssertNext(parser, new Key());
//			AssertNext(parser, new Scalar("key", ScalarStyle.Plain));
//			AssertNext(parser, new Value());
//			AssertNext(parser, new BlockEntry());
//			AssertNext(parser, new Scalar("item 1", ScalarStyle.Plain));
//			AssertNext(parser, new BlockEntry());
//			AssertNext(parser, new Scalar("item 2", ScalarStyle.Plain));
//			AssertNext(parser, new BlockEnd());
//			AssertNext(parser, new StreamEnd());
//			AssertDoesNotHaveNext(parser);
//		}
	}
}