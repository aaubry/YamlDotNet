using System;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using YamlDotNet.Core;
using YamlDotNet.Core.Tokens;

namespace YamlDotNet.UnitTests
{
	[TestFixture]
	public class ScannerTests : YamlTest
	{
		private static Scanner CreateScanner(string name) {
			return new Scanner(YamlFile(name));
		}
		
		private void AssertHasNext(Scanner scanner) {
			Assert.IsTrue(scanner.MoveNext(), "The scanner does not contain more tokens.");
		}
		
		private void AssertDoesNotHaveNext(Scanner scanner) {
			Assert.IsFalse(scanner.MoveNext(), "The scanner should not contain more tokens.");
		}

		private void AssertCurrent(Scanner scanner, Token expected) {
			Console.WriteLine(expected.GetType().Name);
			Assert.IsNotNull(scanner.Current, "The current token is null.");
			Assert.IsInstanceOfType(expected.GetType(), scanner.Current, "The token is not of the expected type.");
			
			foreach (var property in expected.GetType().GetProperties()) {
				if(property.PropertyType != typeof(Mark) && property.CanRead) {
					object value = property.GetValue(scanner.Current, null);
					Console.WriteLine("\t{0} = {1}", property.Name, value);
					Assert.AreEqual(property.GetValue(expected, null), value, string.Format("The property '{0}' is incorrect.", property.Name));
				}
			}
		}
		
		private void AssertNext(Scanner scanner, Token expected) {
			AssertHasNext(scanner);
			AssertCurrent(scanner, expected);
		}
		
		[Test]
		public void VerifyTokensOnExample1()
		{
			Scanner scanner = CreateScanner("test1.yaml");
			
			AssertNext(scanner, new StreamStart());
			AssertNext(scanner, new VersionDirective(new Core.Version(1, 1)));
			AssertNext(scanner, new TagDirective("!", "!foo"));
			AssertNext(scanner, new TagDirective("!yaml!", "tag:yaml.org,2002:"));
			AssertNext(scanner, new DocumentStart());
			AssertNext(scanner, new StreamEnd());
			AssertDoesNotHaveNext(scanner);
		}
		
		[Test]
		public void VerifyTokensOnExample2()
		{
			Scanner scanner = CreateScanner("test2.yaml");
			
			AssertNext(scanner, new StreamStart());
			AssertNext(scanner, new Scalar("a scalar", ScalarStyle.SingleQuoted));
			AssertNext(scanner, new StreamEnd());
			AssertDoesNotHaveNext(scanner);
		}
		
		[Test]
		public void VerifyTokensOnExample3()
		{
			Scanner scanner = CreateScanner("test3.yaml");
			
			AssertNext(scanner, new StreamStart());
			AssertNext(scanner, new DocumentStart());
			AssertNext(scanner, new Scalar("a scalar", ScalarStyle.SingleQuoted));
			AssertNext(scanner, new DocumentEnd());
			AssertNext(scanner, new StreamEnd());
			AssertDoesNotHaveNext(scanner);
		}		
 		
		[Test]
		public void VerifyTokensOnExample4()
		{
			Scanner scanner = CreateScanner("test4.yaml");
			
			AssertNext(scanner, new StreamStart());
			AssertNext(scanner, new Scalar("a scalar", ScalarStyle.SingleQuoted));
			AssertNext(scanner, new DocumentStart());
			AssertNext(scanner, new Scalar("another scalar", ScalarStyle.SingleQuoted));
			AssertNext(scanner, new DocumentStart());
			AssertNext(scanner, new Scalar("yet another scalar", ScalarStyle.SingleQuoted));
			AssertNext(scanner, new StreamEnd());
			AssertDoesNotHaveNext(scanner);
		}		
 		
		[Test]
		public void VerifyTokensOnExample5()
		{
			Scanner scanner = CreateScanner("test5.yaml");
			
			AssertNext(scanner, new StreamStart());
			AssertNext(scanner, new Anchor("A"));
			AssertNext(scanner, new FlowSequenceStart());
			AssertNext(scanner, new AnchorAlias("A"));
			AssertNext(scanner, new FlowSequenceEnd());
			AssertNext(scanner, new StreamEnd());
			AssertDoesNotHaveNext(scanner);
		}
 		
		[Test]
		public void VerifyTokensOnExample6()
		{
			Scanner scanner = CreateScanner("test6.yaml");
			
			AssertNext(scanner, new StreamStart());
			AssertNext(scanner, new Tag("!!", "float"));
			AssertNext(scanner, new Scalar("3.14", ScalarStyle.DoubleQuoted));
			AssertNext(scanner, new StreamEnd());
			AssertDoesNotHaveNext(scanner);
		}
		
		[Test]
		public void VerifyTokensOnExample7()
		{
			Scanner scanner = CreateScanner("test7.yaml");
			
			AssertNext(scanner, new StreamStart());
			AssertNext(scanner, new DocumentStart());
			AssertNext(scanner, new DocumentStart());
			AssertNext(scanner, new Scalar("a plain scalar", ScalarStyle.Plain));
			AssertNext(scanner, new DocumentStart());
			AssertNext(scanner, new Scalar("a single-quoted scalar", ScalarStyle.SingleQuoted));
			AssertNext(scanner, new DocumentStart());
			AssertNext(scanner, new Scalar("a double-quoted scalar", ScalarStyle.DoubleQuoted));
			AssertNext(scanner, new DocumentStart());
			AssertNext(scanner, new Scalar("a literal scalar", ScalarStyle.Literal));
			AssertNext(scanner, new DocumentStart());
			AssertNext(scanner, new Scalar("a folded scalar", ScalarStyle.Folded));
			AssertNext(scanner, new StreamEnd());
			AssertDoesNotHaveNext(scanner);
		}
		
		[Test]
		public void VerifyTokensOnExample8()
		{
			Scanner scanner = CreateScanner("test8.yaml");
			
			AssertNext(scanner, new StreamStart());
			AssertNext(scanner, new FlowSequenceStart());
			AssertNext(scanner, new Scalar("item 1", ScalarStyle.Plain));
			AssertNext(scanner, new FlowEntry());
			AssertNext(scanner, new Scalar("item 2", ScalarStyle.Plain));
			AssertNext(scanner, new FlowEntry());
			AssertNext(scanner, new Scalar("item 3", ScalarStyle.Plain));
			AssertNext(scanner, new FlowSequenceEnd());
			AssertNext(scanner, new StreamEnd());
			AssertDoesNotHaveNext(scanner);
		}

		[Test]
		public void VerifyTokensOnExample9()
		{
			Scanner scanner = CreateScanner("test9.yaml");
			
			AssertNext(scanner, new StreamStart());
			AssertNext(scanner, new FlowMappingStart());
			AssertNext(scanner, new Key());
			AssertNext(scanner, new Scalar("a simple key", ScalarStyle.Plain));
			AssertNext(scanner, new Value());
			AssertNext(scanner, new Scalar("a value", ScalarStyle.Plain));
			AssertNext(scanner, new FlowEntry());
			AssertNext(scanner, new Key());
			AssertNext(scanner, new Scalar("a complex key", ScalarStyle.Plain));
			AssertNext(scanner, new Value());
			AssertNext(scanner, new Scalar("another value", ScalarStyle.Plain));
			AssertNext(scanner, new FlowEntry());
			AssertNext(scanner, new FlowMappingEnd());
			AssertNext(scanner, new StreamEnd());
			AssertDoesNotHaveNext(scanner);
		}

		[Test]
		public void VerifyTokensOnExample10()
		{
			Scanner scanner = CreateScanner("test10.yaml");
			
			AssertNext(scanner, new StreamStart());
			AssertNext(scanner, new BlockSequenceStart());
			AssertNext(scanner, new BlockEntry());
			AssertNext(scanner, new Scalar("item 1", ScalarStyle.Plain));
			AssertNext(scanner, new BlockEntry());
			AssertNext(scanner, new Scalar("item 2", ScalarStyle.Plain));
			AssertNext(scanner, new BlockEntry());
			AssertNext(scanner, new BlockSequenceStart());
			AssertNext(scanner, new BlockEntry());
			AssertNext(scanner, new Scalar("item 3.1", ScalarStyle.Plain));
			AssertNext(scanner, new BlockEntry());
			AssertNext(scanner, new Scalar("item 3.2", ScalarStyle.Plain));
			AssertNext(scanner, new BlockEnd());
			AssertNext(scanner, new BlockEntry());
			AssertNext(scanner, new BlockMappingStart());
			AssertNext(scanner, new Key());
			AssertNext(scanner, new Scalar("key 1", ScalarStyle.Plain));
			AssertNext(scanner, new Value());
			AssertNext(scanner, new Scalar("value 1", ScalarStyle.Plain));
			AssertNext(scanner, new Key());
			AssertNext(scanner, new Scalar("key 2", ScalarStyle.Plain));
			AssertNext(scanner, new Value());
			AssertNext(scanner, new Scalar("value 2", ScalarStyle.Plain));
			AssertNext(scanner, new BlockEnd());
			AssertNext(scanner, new BlockEnd());
			AssertNext(scanner, new StreamEnd());
			AssertDoesNotHaveNext(scanner);
		}
	
		[Test]
		public void VerifyTokensOnExample11()
		{
			Scanner scanner = CreateScanner("test11.yaml");
			
			AssertNext(scanner, new StreamStart());
			AssertNext(scanner, new BlockMappingStart());
			AssertNext(scanner, new Key());
			AssertNext(scanner, new Scalar("a simple key", ScalarStyle.Plain));
			AssertNext(scanner, new Value());
			AssertNext(scanner, new Scalar("a value", ScalarStyle.Plain));
			AssertNext(scanner, new Key());
			AssertNext(scanner, new Scalar("a complex key", ScalarStyle.Plain));
			AssertNext(scanner, new Value());
			AssertNext(scanner, new Scalar("another value", ScalarStyle.Plain));
			AssertNext(scanner, new Key());
			AssertNext(scanner, new Scalar("a mapping", ScalarStyle.Plain));
			AssertNext(scanner, new Value());
			AssertNext(scanner, new BlockMappingStart());
			AssertNext(scanner, new Key());
			AssertNext(scanner, new Scalar("key 1", ScalarStyle.Plain));
			AssertNext(scanner, new Value());
			AssertNext(scanner, new Scalar("value 1", ScalarStyle.Plain));
			AssertNext(scanner, new Key());
			AssertNext(scanner, new Scalar("key 2", ScalarStyle.Plain));
			AssertNext(scanner, new Value());
			AssertNext(scanner, new Scalar("value 2", ScalarStyle.Plain));
			AssertNext(scanner, new BlockEnd());
			AssertNext(scanner, new Key());
			AssertNext(scanner, new Scalar("a sequence", ScalarStyle.Plain));
			AssertNext(scanner, new Value());
			AssertNext(scanner, new BlockSequenceStart());
			AssertNext(scanner, new BlockEntry());
			AssertNext(scanner, new Scalar("item 1", ScalarStyle.Plain));
			AssertNext(scanner, new BlockEntry());
			AssertNext(scanner, new Scalar("item 2", ScalarStyle.Plain));
			AssertNext(scanner, new BlockEnd());
			AssertNext(scanner, new BlockEnd());
			AssertNext(scanner, new StreamEnd());
			AssertDoesNotHaveNext(scanner);
		}
	
		[Test]
		public void VerifyTokensOnExample12()
		{
			Scanner scanner = CreateScanner("test12.yaml");
			
			AssertNext(scanner, new StreamStart());
			AssertNext(scanner, new BlockSequenceStart());
			AssertNext(scanner, new BlockEntry());
			AssertNext(scanner, new BlockSequenceStart());
			AssertNext(scanner, new BlockEntry());
			AssertNext(scanner, new Scalar("item 1", ScalarStyle.Plain));
			AssertNext(scanner, new BlockEntry());
			AssertNext(scanner, new Scalar("item 2", ScalarStyle.Plain));
			AssertNext(scanner, new BlockEnd());
			AssertNext(scanner, new BlockEntry());
			AssertNext(scanner, new BlockMappingStart());
			AssertNext(scanner, new Key());
			AssertNext(scanner, new Scalar("key 1", ScalarStyle.Plain));
			AssertNext(scanner, new Value());
			AssertNext(scanner, new Scalar("value 1", ScalarStyle.Plain));
			AssertNext(scanner, new Key());
			AssertNext(scanner, new Scalar("key 2", ScalarStyle.Plain));
			AssertNext(scanner, new Value());
			AssertNext(scanner, new Scalar("value 2", ScalarStyle.Plain));
			AssertNext(scanner, new BlockEnd());
			AssertNext(scanner, new BlockEntry());
			AssertNext(scanner, new BlockMappingStart());
			AssertNext(scanner, new Key());
			AssertNext(scanner, new Scalar("complex key", ScalarStyle.Plain));
			AssertNext(scanner, new Value());
			AssertNext(scanner, new Scalar("complex value", ScalarStyle.Plain));
			AssertNext(scanner, new BlockEnd());
			AssertNext(scanner, new BlockEnd());
			AssertNext(scanner, new StreamEnd());
			AssertDoesNotHaveNext(scanner);
		}
			
		[Test]
		public void VerifyTokensOnExample13()
		{
			Scanner scanner = CreateScanner("test13.yaml");
			
			AssertNext(scanner, new StreamStart());
			AssertNext(scanner, new BlockMappingStart());
			AssertNext(scanner, new Key());
			AssertNext(scanner, new Scalar("a sequence", ScalarStyle.Plain));
			AssertNext(scanner, new Value());
			AssertNext(scanner, new BlockSequenceStart());
			AssertNext(scanner, new BlockEntry());
			AssertNext(scanner, new Scalar("item 1", ScalarStyle.Plain));
			AssertNext(scanner, new BlockEntry());
			AssertNext(scanner, new Scalar("item 2", ScalarStyle.Plain));
			AssertNext(scanner, new BlockEnd());
			AssertNext(scanner, new Key());
			AssertNext(scanner, new Scalar("a mapping", ScalarStyle.Plain));
			AssertNext(scanner, new Value());
			AssertNext(scanner, new BlockMappingStart());
			AssertNext(scanner, new Key());
			AssertNext(scanner, new Scalar("key 1", ScalarStyle.Plain));
			AssertNext(scanner, new Value());
			AssertNext(scanner, new Scalar("value 1", ScalarStyle.Plain));
			AssertNext(scanner, new Key());
			AssertNext(scanner, new Scalar("key 2", ScalarStyle.Plain));
			AssertNext(scanner, new Value());
			AssertNext(scanner, new Scalar("value 2", ScalarStyle.Plain));
			AssertNext(scanner, new BlockEnd());
			AssertNext(scanner, new BlockEnd());
			AssertNext(scanner, new StreamEnd());
			AssertDoesNotHaveNext(scanner);
		}
			
		[Test]
		public void VerifyTokensOnExample14()
		{
			Scanner scanner = CreateScanner("test14.yaml");
			
			AssertNext(scanner, new StreamStart());
			AssertNext(scanner, new BlockMappingStart());
			AssertNext(scanner, new Key());
			AssertNext(scanner, new Scalar("key", ScalarStyle.Plain));
			AssertNext(scanner, new Value());
			AssertNext(scanner, new BlockEntry());
			AssertNext(scanner, new Scalar("item 1", ScalarStyle.Plain));
			AssertNext(scanner, new BlockEntry());
			AssertNext(scanner, new Scalar("item 2", ScalarStyle.Plain));
			AssertNext(scanner, new BlockEnd());
			AssertNext(scanner, new StreamEnd());
			AssertDoesNotHaveNext(scanner);
		}
	}
}