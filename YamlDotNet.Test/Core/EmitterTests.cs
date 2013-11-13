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

using System.Collections.Generic;
using System.Linq;
using System.IO;
using FluentAssertions;
using Xunit;
using Xunit.Extensions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;

namespace YamlDotNet.Test.Core
{
	public class EmitterTests
	{
		[Theory]
		[InlineData("01-directives.yaml")]
		[InlineData("02-scalar-in-imp-doc.yaml")]
		[InlineData("03-scalar-in-exp-doc.yaml")]
		[InlineData("04-scalars-in-multi-docs.yaml")]
		[InlineData("05-circular-sequence.yaml")]
		[InlineData("06-float-tag.yaml")]
		[InlineData("07-scalar-styles.yaml")]
		[InlineData("08-flow-sequence.yaml")]
		[InlineData("09-flow-mapping.yaml")]
		[InlineData("10-mixed-nodes-in-sequence.yaml")]
		[InlineData("11-mixed-nodes-in-mapping.yaml")]
		[InlineData("12-compact-sequence.yaml")]
		[InlineData("13-compact-mapping.yaml")]
		[InlineData("14-mapping-wo-indent.yaml")]
		public void CompareOriginalAndEmittedText(string filename)
		{
			var stream = Yaml.StreamFrom(filename);

			var originalEvents = ParsingEventsOf(stream.ReadToEnd());
			var emittedText = EmittedTextFrom(originalEvents);
			var emittedEvents = ParsingEventsOf(emittedText);

			Dump.WriteLine(emittedText);
			emittedEvents.Should().Equal(originalEvents);
		}

		private IList<IParsingEvent> ParsingEventsOf(string text)
		{
			IParser parser = new Parser(new StringReader(text));
			return EnumerationOf(parser).ToList();
		}

		private IEnumerable<IParsingEvent> EnumerationOf(IParser parser)
		{
			while (parser.MoveNext())
			{
				yield return parser.Current;
			}
		}

		private static string EmittedTextFrom(IEnumerable<IParsingEvent> events)
		{
			var output = new StringWriter();
			var emitter = new Emitter(output, 2, int.MaxValue, false);

			events
				.Do(evt => Dump.WriteLine(evt))
				.Run(emitter.Emit);

			return output.ToString();
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

			foreach (var evt in events) {
				emitter.Emit(evt);
			}

			emitter.Emit(new DocumentEnd(true));
			emitter.Emit(new StreamEnd());

			return buffer.ToString();
		}
	}
}