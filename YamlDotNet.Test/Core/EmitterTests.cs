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

using System.Linq;
using System.IO;
using Xunit;
using Xunit.Extensions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;

namespace YamlDotNet.Test.Core
{
	public class EmitterTests
	{
		[Fact]
		public void EmitExample1()
		{
			ParseAndEmit("01-directives.yaml");
		}

		[Fact]
		public void EmitExample2()
		{
			ParseAndEmit("02-scalar-in-imp-doc.yaml");
		}

		[Fact]
		public void EmitExample3()
		{
			ParseAndEmit("03-scalar-in-exp-doc.yaml");
		}

		[Fact]
		public void EmitExample4()
		{
			ParseAndEmit("04-scalars-in-multi-docs.yaml");
		}

		[Fact]
		public void EmitExample5()
		{
			ParseAndEmit("05-circular-sequence.yaml");
		}

		[Fact]
		public void EmitExample6()
		{
			ParseAndEmit("06-float-tag.yaml");
		}

		[Fact]
		public void EmitExample7()
		{
			ParseAndEmit("07-scalar-styles.yaml");
		}

		[Fact]
		public void EmitExample8()
		{
			ParseAndEmit("08-flow-sequence.yaml");
		}

		[Fact]
		public void EmitExample9()
		{
			ParseAndEmit("09-flow-mapping.yaml");
		}

		[Fact]
		public void EmitExample10()
		{
			ParseAndEmit("10-mixed-nodes-in-sequence.yaml");
		}

		[Fact]
		public void EmitExample11()
		{
			ParseAndEmit("11-mixed-nodes-in-mapping.yaml");
		}

		[Fact]
		public void EmitExample12()
		{
			ParseAndEmit("12-compact-sequence.yaml");
		}

		[Fact]
		public void EmitExample13()
		{
			ParseAndEmit("13-compact-mapping.yaml");
		}

		[Fact]
		public void EmitExample14()
		{
			ParseAndEmit("14-mapping-wo-indent.yaml");
		}

		private void ParseAndEmit(string filename)
		{
			var testText = Yaml.StreamFrom(filename).ReadToEnd();

			var output = new StringWriter();
			IParser parser = new Parser(new StringReader(testText));
			IEmitter emitter = new Emitter(output, 2, int.MaxValue, false);
			Dump.WriteLine("= Parse and emit yaml file ["+ filename + "] =");
			while (parser.MoveNext())
			{
				Dump.WriteLine(parser.Current);
				emitter.Emit(parser.Current);
			}
			Dump.WriteLine();

			Dump.WriteLine("= Original =");
			Dump.WriteLine(testText);
			Dump.WriteLine();

			Dump.WriteLine("= Result =");
			Dump.WriteLine(output);
			Dump.WriteLine();

			// Todo: figure out how (if?) to assert
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
			var yaml = EmitScalar(new Scalar(null, null, text, ScalarStyle.Folded, true, false));
			Dump.WriteLine(yaml);
			Assert.True(yaml.Contains("world"));
		}

		[Fact]
		public void FoldedStyleIsSelectedWhenNewLinesAreFoundInLiteral()
		{
			var yaml = EmitScalar(new Scalar(null, null, "hello\nworld", ScalarStyle.Any, true, false));
			Dump.WriteLine(yaml);
			Assert.True(yaml.Contains(">"));
		}

		[Fact]
		public void FoldedStyleDoesNotGenerateExtraLineBreaks()
		{
			var yaml = EmitScalar(new Scalar(null, null, "hello\nworld", ScalarStyle.Folded, true, false));
			Dump.WriteLine(yaml);

			// Todo: Why involve the rep. model when testing the Emitter? Can we match using a regex?
			var stream = new YamlStream();
			stream.Load(new StringReader(yaml));
			var sequence = (YamlSequenceNode)stream.Documents[0].RootNode;
			var scalar = (YamlScalarNode)sequence.Children[0];

			Assert.Equal("hello\nworld", scalar.Value);
		}

		[Fact]
		public void FoldedStyleDoesNotCollapseLineBreaks()
		{
			var yaml = EmitScalar(new Scalar(null, null, ">+\n", ScalarStyle.Folded, true, false));
			Dump.WriteLine("${0}$", yaml);

			var stream = new YamlStream();
			stream.Load(new StringReader(yaml));
			var sequence = (YamlSequenceNode)stream.Documents[0].RootNode;
			var scalar = (YamlScalarNode)sequence.Children[0];

			Assert.Equal(">+\n", scalar.Value);
		}

		[Fact]
		[Trait("motive", "issue #39")]
		public void FoldedStylePreservesNewLines()
		{
			var input = "id: 0\nPayload:\n  X: 5\n  Y: 6\n";

			var yaml = Emit(
				new MappingStart(),
				new Scalar("Payload"),
				new Scalar(null, null, input, ScalarStyle.Folded, true, false),
				new MappingEnd()
			);
			Dump.WriteLine(yaml);

			var stream = new YamlStream();
			stream.Load(new StringReader(yaml));

			var mapping = (YamlMappingNode)stream.Documents[0].RootNode;
			var value = (YamlScalarNode)mapping.Children.First().Value;

			var output = value.Value;
			Dump.WriteLine(output);
			Assert.Equal(input, output);
		}
	}
}