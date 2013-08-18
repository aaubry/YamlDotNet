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
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Xunit.Extensions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;

namespace YamlDotNet.UnitTests
{
	public class EmitterTests : YamlTest
	{
		private void ParseAndEmit(string name) {
			string testText = YamlFile(name).ReadToEnd();

			Parser parser = new Parser(new StringReader(testText));
			using(StringWriter output = new StringWriter()) {
				Emitter emitter = new Emitter(output, 2, int.MaxValue, false);
				while(parser.MoveNext()) {
					//Console.WriteLine(parser.Current.GetType().Name);
					Console.Error.WriteLine(parser.Current);
					emitter.Emit(parser.Current);
				}
				
				string result = output.ToString();
				
				Console.WriteLine();
				Console.WriteLine("------------------------------");
				Console.WriteLine();
				Console.WriteLine(testText);
				Console.WriteLine();
				Console.WriteLine("------------------------------");
				Console.WriteLine();
				Console.WriteLine(result);
				Console.WriteLine();
				Console.WriteLine("------------------------------");
				Console.WriteLine();

				/*
				Parser resultParser = new Parser(new StringReader(result));
				while(resultParser.MoveNext()) {
					Console.WriteLine(resultParser.Current.GetType().Name);
				}
				*/
				/*
				
				if(testText != result) {
					Console.WriteLine();
					Console.WriteLine("------------------------------");
					Console.WriteLine();
					Console.WriteLine("Expected:");
					Console.WriteLine();
					Console.WriteLine(testText);
					Console.WriteLine();
					Console.WriteLine("------------------------------");
					Console.WriteLine();
					Console.WriteLine("Result:");
					Console.WriteLine();
					Console.WriteLine(result);
					Console.WriteLine();
					Console.WriteLine("------------------------------");
					Console.WriteLine();
				}
				
				Assert.Equal(testText, result);
				*/
			}
		}
		
		[Fact]
		public void EmitExample1()
		{
			ParseAndEmit("test1.yaml");
		}
		
		[Fact]
		public void EmitExample2()
		{
			ParseAndEmit("test2.yaml");
		}
		
		[Fact]
		public void EmitExample3()
		{
			ParseAndEmit("test3.yaml");
		}
		
		[Fact]
		public void EmitExample4()
		{
			ParseAndEmit("test4.yaml");
		}
		
		[Fact]
		public void EmitExample5()
		{
			ParseAndEmit("test5.yaml");
		}
		
		[Fact]
		public void EmitExample6()
		{
			ParseAndEmit("test6.yaml");
		}
		
		[Fact]
		public void EmitExample7()
		{
			ParseAndEmit("test7.yaml");
		}
		
		[Fact]
		public void EmitExample8()
		{
			ParseAndEmit("test8.yaml");
		}
		
		[Fact]
		public void EmitExample9()
		{
			ParseAndEmit("test9.yaml");
		}
		
		[Fact]
		public void EmitExample10()
		{
			ParseAndEmit("test10.yaml");
		}
		
		[Fact]
		public void EmitExample11()
		{
			ParseAndEmit("test11.yaml");
		}
		
		[Fact]
		public void EmitExample12()
		{
			ParseAndEmit("test12.yaml");
		}
		
		[Fact]
		public void EmitExample13()
		{
			ParseAndEmit("test13.yaml");
		}
		
		[Fact]
		public void EmitExample14()
		{
			ParseAndEmit("test14.yaml");
		}

		private string EmitScalar(Scalar scalar)
		{
			return Emit(
				new SequenceStart(null, null, false, SequenceStyle.Block),
				scalar,
				new SequenceEnd()
			);
		}

		private string Emit(params ParsingEvent[] events)
		{
			var buffer = new StringWriter();
			var emitter = new Emitter(buffer);
			emitter.Emit(new StreamStart());
			emitter.Emit(new DocumentStart(null, null, true));

			foreach (var evt in events)
			{
				emitter.Emit(evt);
			}

			emitter.Emit(new DocumentEnd(true));
			emitter.Emit(new StreamEnd());

			return buffer.ToString();
		}

		[Theory]
		[InlineData("LF hello\nworld")]
		[InlineData("CRLF hello\r\nworld")]
		public void FoldedStyleDoesNotLooseCharacters(string text)
		{
			var yaml = Emit(new Scalar(null, null, text, ScalarStyle.Folded, true, false));
			Console.WriteLine(yaml);
			Assert.True(yaml.Contains("world"));
		}

		[Fact]
		public void FoldedStyleIsSelectedWhenNewLinesAreFoundInLiteral()
		{
			var yaml = Emit(new Scalar(null, null, "hello\nworld", ScalarStyle.Any, true, false));
			Console.WriteLine(yaml);
			Assert.True(yaml.Contains(">"));
		}

		[Fact]
		public void FoldedStyleDoesNotGenerateExtraLineBreaks()
		{
			var yaml = Emit(new Scalar(null, null, "hello\nworld", ScalarStyle.Folded, true, false));
			Console.WriteLine(yaml);
			Assert.False(yaml.Contains("\r\n\r\n"));
		}

		[Fact]
		public void FoldedStyleDoesNotCollapseLineBreaks()
		{
			var yaml = Emit(new Scalar(null, null, ">+\n", ScalarStyle.Folded, true, false));
			Console.WriteLine(yaml);
			Assert.True(yaml.Contains("\r\n\r\n"));
		}

		[Fact]
		public void FoldedStylePreservesNewLines()
		{
			var input = "id: 0\nPayload:\n  X: 5\n  Y: 6\n";

			var yaml = Emit(
				new MappingStart(),
				new Scalar("Payload"),
				new Scalar(null, null, input, ScalarStyle.Folded, true, false),
				new MappingEnd()
			);
			Console.WriteLine(yaml);

			var stream = new YamlStream();
			stream.Load(new StringReader(yaml));

			var mapping = (YamlMappingNode)stream.Documents[0].RootNode;
			var value = (YamlScalarNode)mapping.Children.First().Value;

			Console.WriteLine(value.Value);

			Assert.Equal(input, value.Value);
		}
	}
}