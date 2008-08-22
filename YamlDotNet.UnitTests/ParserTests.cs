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
					if(Type.GetTypeCode(expectedValue.GetType()) == TypeCode.Object && expectedValue is IEnumerable) {
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
			AssertNext(parser, new DocumentStart(new VersionDirective(new Core.Version(1, 1)), new TagDirective[] { new TagDirective("!", "!foo"), new TagDirective("!yaml!", "tag:yaml.org,2002:") }));
			AssertNext(parser, new Scalar(string.Empty, string.Empty));
			AssertNext(parser, new DocumentEnd());
			AssertNext(parser, new StreamEnd());
			AssertDoesNotHaveNext(parser);
		}
	}
}