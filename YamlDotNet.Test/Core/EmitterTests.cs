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
	public class EmitterTests : EventsHelper
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
			originalEvents.Run(x => Dump.WriteLine(x));
			var emittedText = EmittedTextFrom(originalEvents);
			Dump.WriteLine(emittedText);
			var emittedEvents = ParsingEventsOf(emittedText);

			emittedEvents.Should().Equal(originalEvents);
		}

		private IList<ParsingEvent> ParsingEventsOf(string text)
		{
			var parser = new Parser(new StringReader(text));
			return EnumerationOf(parser).ToList();
		}

		private IEnumerable<ParsingEvent> EnumerationOf(IParser parser)
		{
			while (parser.MoveNext())
			{
				yield return parser.Current;
			}
		}

		private string EmittedTextFrom(IEnumerable<ParsingEvent> events)
		{
			return Emit(events, EmitterWithIndentCreator);
		}

		private Func<TextWriter, Emitter> EmitterWithIndentCreator
		{
			get { return writer => new Emitter(writer, 2, int.MaxValue, false); }
		}

		[Theory]
		[InlineData("LF hello\nworld")]
		[InlineData("CRLF hello\r\nworld")]
		public void FoldedStyleDoesNotLooseCharacters(string text)
		{
			var events = SequenceWith(FoldedScalar(text).ExplicitQuoted);

			var yaml = Emit(StreamedDocumentWith(events));

			Dump.WriteLine(yaml);
			yaml.Should().Contain("world");
		}

		[Fact]
		public void FoldedStyleIsSelectedWhenNewLinesAreFoundInLiteral()
		{
			var events = SequenceWith(Scalar("hello\nworld").ExplicitQuoted);

			var yaml = Emit(StreamedDocumentWith(events));

			Dump.WriteLine(yaml);
			yaml.Should().Contain(">");
		}

		[Fact]
		public void FoldedStyleDoesNotGenerateExtraLineBreaks()
		{
			var events = SequenceWith(FoldedScalar("hello\nworld").ExplicitQuoted);

			var yaml = Emit(StreamedDocumentWith(events));

			// Todo: Why involve the rep. model when testing the Emitter? Can we match using a regex?
			var stream = new YamlStream();
			stream.Load(new StringReader(yaml));
			var sequence = (YamlSequenceNode)stream.Documents[0].RootNode;
			var scalar = (YamlScalarNode)sequence.Children[0];

			Dump.WriteLine(yaml);
			scalar.Value.Should().Be("hello\nworld");
		}

		[Fact]
		public void FoldedStyleDoesNotCollapseLineBreaks()
		{
			var events = SequenceWith(FoldedScalar(">+\n").ExplicitQuoted);

			var yaml = Emit(StreamedDocumentWith(events));

			var stream = new YamlStream();
			stream.Load(new StringReader(yaml));
			var sequence = (YamlSequenceNode)stream.Documents[0].RootNode;
			var scalar = (YamlScalarNode)sequence.Children[0];

			Dump.WriteLine("${0}$", yaml);
			scalar.Value.Should().Be(">+\n");
		}

		[Fact]
		[Trait("motive", "issue #39")]
		public void FoldedStylePreservesNewLines()
		{
			var input = "id: 0\nPayload:\n  X: 5\n  Y: 6\n";
			var events = MappingWith(
				Scalar("Payload"),
				FoldedScalar(input).ExplicitQuoted);

			var yaml = Emit(StreamedDocumentWith(events));
			Dump.WriteLine(yaml);

			var stream = new YamlStream();
			stream.Load(new StringReader(yaml));

			var mapping = (YamlMappingNode)stream.Documents[0].RootNode;
			var value = (YamlScalarNode)mapping.Children.First().Value;

			Dump.WriteLine(value.Value);
			value.Value.Should().Be(input);
		}

		private string Emit(IEnumerable<ParsingEvent> events)
		{
			return Emit(events, x => new Emitter(x));
		}

		private string Emit(IEnumerable<ParsingEvent> events, Func<TextWriter, Emitter> createEmitter)
		{
			var writer = new StringWriter();
			var emitter = createEmitter(writer);
			events.Run(emitter.Emit);
			return writer.ToString();
		}

		private IEnumerable<ParsingEvent> StreamedDocumentWith(IEnumerable<ParsingEvent> events)
		{
			return Wrap(
				Wrap(events, DocumentStart(Implicit), DocumentEnd(Implicit)),
				StreamStart, StreamEnd);
		}

		private IEnumerable<ParsingEvent> SequenceWith(params ParsingEvent[] events)
		{
			return Wrap(events, BlockSequenceStart.Explicit, SequenceEnd);
		}

		private IEnumerable<ParsingEvent> MappingWith(params ParsingEvent[] events)
		{
			return Wrap(events, MappingStart, MappingEnd);
		}

		private IEnumerable<ParsingEvent> Wrap(IEnumerable<ParsingEvent> events, ParsingEvent start, ParsingEvent end)
		{
			yield return start;
			foreach (var @event in events)
			{
				yield return @event;
			}
			yield return end;
		}
	}
}