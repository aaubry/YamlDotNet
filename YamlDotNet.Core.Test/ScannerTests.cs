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
using Xunit;
using YamlDotNet.Tokens;

namespace YamlDotNet.Test
{
	public class ScannerTests : YamlTest
	{
		[Fact]
		public void VerifyTokensOnExample1()
		{
			Scanner scanner = CreateScanner("test1.yaml");
			
			AssertNext(scanner, new StreamStart());
			AssertNext(scanner, new VersionDirective(new Version(1, 1)));
			AssertNext(scanner, new TagDirective("!", "!foo"));
			AssertNext(scanner, new TagDirective("!yaml!", "tag:yaml.org,2002:"));
			AssertNext(scanner, new DocumentStart());
			AssertNext(scanner, new StreamEnd());
			AssertDoesNotHaveNext(scanner);
		}
		
		[Fact]
		public void VerifyTokensOnExample2()
		{
			Scanner scanner = CreateScanner("test2.yaml");
			
			AssertNext(scanner, new StreamStart());
			AssertNext(scanner, new Scalar("a scalar", ScalarStyle.SingleQuoted));
			AssertNext(scanner, new StreamEnd());
			AssertDoesNotHaveNext(scanner);
		}
		
		[Fact]
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
 		
		[Fact]
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
 		
		[Fact]
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
 		
		[Fact]
		public void VerifyTokensOnExample6()
		{
			Scanner scanner = CreateScanner("test6.yaml");
			
			AssertNext(scanner, new StreamStart());
			AssertNext(scanner, new Tag("!!", "float"));
			AssertNext(scanner, new Scalar("3.14", ScalarStyle.DoubleQuoted));
			AssertNext(scanner, new StreamEnd());
			AssertDoesNotHaveNext(scanner);
		}
		
		[Fact]
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
		
		[Fact]
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

		[Fact]
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

		[Fact]
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
	
		[Fact]
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
	
		[Fact]
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
			
		[Fact]
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
			
		[Fact]
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

		private static Scanner CreateScanner(string name) {
			return new Scanner(YamlFile(name));
		}

		private void AssertNext(Scanner scanner, Token expected) {
			AssertHasNext(scanner);
			AssertCurrent(scanner, expected);
		}

		private void AssertHasNext(Scanner scanner) {
			Assert.True(scanner.MoveNext());
		}

		private void AssertDoesNotHaveNext(Scanner scanner) {
			Assert.False(scanner.MoveNext());
		}

		private void AssertCurrent(Scanner scanner, Token expected) {
			Dump.WriteLine(expected.GetType().Name);
			Assert.NotNull(scanner.Current);
			Assert.True(expected.GetType().IsAssignableFrom(scanner.Current.GetType()));

			foreach (var property in expected.GetType().GetProperties()) {
				if (property.PropertyType != typeof(Mark) && property.CanRead) {
					var value = property.GetValue(scanner.Current, null);
					Dump.WriteLine("\t{0} = {1}", property.Name, value);
					Assert.Equal(property.GetValue(expected, null), value);
				}
			}
		}
	}
}