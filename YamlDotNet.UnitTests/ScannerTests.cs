using System;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using YamlDotNet.CoreCs;
using YamlDotNet.CoreCs.Tokens;

namespace YamlDotNet.UnitTests
{
	[TestFixture]
	public class ScannerTests
	{
		private TextReader YamlFile(string content) {
			string[] lines = content.Split('\n');
			StringBuilder buffer = new StringBuilder();
			int indent = -1;
			for(int i = 1; i < lines.Length - 1; ++i) {
				if(indent < 0) {
					indent = 0;
					while(lines[i][indent] == '\t') {
						++indent;
					}
				} else {
					buffer.Append('\n');
				}
				buffer.Append(lines[i].Substring(indent));
			}
			return new StringReader(buffer.ToString());
		}
		
		[Test]
		public void MakeSureThatYamlFileWorks() {
			TextReader file = YamlFile(@"
				%YAML   1.1
				%TAG    !   !foo
				%TAG    !yaml!  tag:yaml.org,2002:
				---
			");
			
			string expected = "%YAML   1.1\n%TAG    !   !foo\n%TAG    !yaml!  tag:yaml.org,2002:\n---";
			Assert.AreEqual(expected, file.ReadToEnd(), "The YamlFile method does not work properly.");
		}
		
		private Scanner CreateScanner(string content) {
			return new Scanner(YamlFile(content));
		}
		
		private void AssertHasNext(Scanner scanner) {
			Assert.IsTrue(scanner.MoveNext(), "The scanner does not contain more tokens.");
		}
		
		private void AssertDoesNotHaveNext(Scanner scanner) {
			Assert.IsFalse(scanner.MoveNext(), "The scanner should not contain more tokens.");
		}

		private void AssertCurrent(Scanner scanner, Token expected) {
			Console.WriteLine(expected.GetType().Name);
			Assert.IsInstanceOfType(expected.GetType(), scanner.Current, "The token is not of the expected type.");
			
			foreach (PropertyInfo property in expected.GetType().GetProperties()) {
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
			Scanner scanner = CreateScanner(@"
				%YAML   1.1
				%TAG    !   !foo
				%TAG    !yaml!  tag:yaml.org,2002:
				---
			");
			
			AssertNext(scanner, new StreamStart());
			AssertNext(scanner, new VersionDirective(new CoreCs.Version(1, 1)));
			AssertNext(scanner, new TagDirective("!", "!foo"));
			AssertNext(scanner, new TagDirective("!yaml!", "tag:yaml.org,2002:"));
			AssertNext(scanner, new DocumentStart());
			AssertNext(scanner, new StreamEnd());
			AssertDoesNotHaveNext(scanner);
		}
		
		[Test]
		public void VerifyTokensOnExample2()
		{
			Scanner scanner = CreateScanner(@"
				'a scalar'
			");
			
			AssertNext(scanner, new StreamStart());
			AssertNext(scanner, new Scalar("a scalar", ScalarStyle.SingleQuoted));
			AssertNext(scanner, new StreamEnd());
			AssertDoesNotHaveNext(scanner);
		}
		
		[Test]
		public void VerifyTokensOnExample3()
		{
			Scanner scanner = CreateScanner(@"
				---
				'a scalar'
				...
			");
			
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
			Scanner scanner = CreateScanner(@"
				'a scalar'
				---
				'another scalar'
				---
				'yet another scalar'
			");
			
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
			Scanner scanner = CreateScanner(@"
				&A [ *A ]
			");
			
			AssertNext(scanner, new StreamStart());
			AssertNext(scanner, new Anchor("A"));
			AssertNext(scanner, new FlowSequenceStart());
			AssertNext(scanner, new Alias("A"));
			AssertNext(scanner, new FlowSequenceEnd());
			AssertNext(scanner, new StreamEnd());
			AssertDoesNotHaveNext(scanner);
		}
 		
		[Test]
		public void VerifyTokensOnExample6()
		{
			Scanner scanner = CreateScanner(@"
				!!float ""3.14""  # A good approximation.
			");
			
			AssertNext(scanner, new StreamStart());
			AssertNext(scanner, new Tag("!!", "float"));
			AssertNext(scanner, new Scalar("3.14", ScalarStyle.DoubleQuoted));
			AssertNext(scanner, new StreamEnd());
			AssertDoesNotHaveNext(scanner);
		}
		
		[Test]
		public void VerifyTokensOnExample7()
		{
			Scanner scanner = CreateScanner(@"
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
			Scanner scanner = CreateScanner(@"
				[item 1, item 2, item 3]
			");
			
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
			Scanner scanner = CreateScanner(@"
				{
					a simple key: a value,  # Note that the KEY token is produced.
					? a complex key: another value,
				}
			");
			
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
			Scanner scanner = CreateScanner(@"
				- item 1
				- item 2
				-
				  - item 3.1
				  - item 3.2
				-
				  key 1: value 1
				  key 2: value 2
			");
			
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
		[Ignore("The original C code also fails this test")]
		public void VerifyTokensOnExample11()
		{
			Scanner scanner = CreateScanner(@"
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
			AssertNext(scanner, new Scalar("a mappint", ScalarStyle.Plain));
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
			Scanner scanner = CreateScanner(@"
				- - item 1
				  - item 2
				- key 1: value 1
				  key 2: value 2
				- ? complex key
				  : complex value
			");
			
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
			Scanner scanner = CreateScanner(@"
				? a sequence
				: - item 1
				  - item 2
				? a mapping
				: key 1: value 1
				  key 2: value 2
			");
			
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
		[Ignore("The original C code also fails this test")]
		public void VerifyTokensOnExample14()
		{
			Scanner scanner = CreateScanner(@"
				key:
				- item 1    # BLOCK-SEQUENCE-START is NOT produced here.
				- item 2
			");
			
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