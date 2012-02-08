//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2008, 2009, 2010, 2011, 2012 Antoine Aubry
    
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
using Xunit;
using YamlDotNet.RepresentationModel;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace YamlDotNet.UnitTests.RepresentationModel
{
	public class YamlStreamTests : YamlTest
	{

		[Fact]
		public void LoadSimpleDocument() {
			YamlStream stream = new YamlStream();
			stream.Load(YamlFile("test2.yaml"));
			
			Assert.Equal(1, stream.Documents.Count);
			Assert.IsType<YamlScalarNode>(stream.Documents[0].RootNode);
			Assert.Equal("a scalar", ((YamlScalarNode)stream.Documents[0].RootNode).Value);
		}
		
		[Fact]
		public void BackwardAliasReferenceWorks() {
			YamlStream stream = new YamlStream();
			stream.Load(YamlFile("backwardsAlias.yaml"));
			
			Assert.Equal(1, stream.Documents.Count);
			Assert.IsType<YamlSequenceNode>(stream.Documents[0].RootNode);

			YamlSequenceNode sequence = (YamlSequenceNode)stream.Documents[0].RootNode;
			Assert.Equal(3, sequence.Children.Count);

			Assert.Equal("a scalar", ((YamlScalarNode)sequence.Children[0]).Value);
			Assert.Equal("another scalar", ((YamlScalarNode)sequence.Children[1]).Value);
			Assert.Equal("a scalar", ((YamlScalarNode)sequence.Children[2]).Value);
			Assert.Same(sequence.Children[0], sequence.Children[2]);
		}
		
		[Fact]
		public void ForwardAliasReferenceWorks() {
			YamlStream stream = new YamlStream();
			stream.Load(YamlFile("forwardAlias.yaml"));
			
			Assert.Equal(1, stream.Documents.Count);
			Assert.IsType<YamlSequenceNode>(stream.Documents[0].RootNode);

			YamlSequenceNode sequence = (YamlSequenceNode)stream.Documents[0].RootNode;
			Assert.Equal(3, sequence.Children.Count);

			Assert.Equal("a scalar", ((YamlScalarNode)sequence.Children[0]).Value);
			Assert.Equal("another scalar", ((YamlScalarNode)sequence.Children[1]).Value);
			Assert.Equal("a scalar", ((YamlScalarNode)sequence.Children[2]).Value);
			Assert.Same(sequence.Children[0], sequence.Children[2]);
		}

		private enum YamlNodeEventType
		{
			SequenceStart,
			SequenceEnd,
			MappingStart,
			MappingEnd,
			Scalar,
		}

		private class YamlNodeEvent
		{
			public YamlNodeEventType Type
			{
				get;
				private set;
			}

			public string Anchor
			{
				get;
				private set;
			}

			public string Tag
			{
				get;
				private set;
			}

			public string Value
			{
				get;
				private set;
			}

			public YamlNodeEvent(YamlNodeEventType type, string anchor, string tag, string value)
			{
				Type = type;
				Anchor = anchor;
				Tag = tag;
				Value = value;
			}
		}

		private class YamlDocumentStructureBuilder : YamlVisitor
		{
			private readonly List<YamlNodeEvent> events = new List<YamlNodeEvent>();

			public IList<YamlNodeEvent> Events
			{
				get
				{
					return events;
				}
			}

			protected override void Visit(YamlScalarNode scalar)
			{
				events.Add(new YamlNodeEvent(YamlNodeEventType.Scalar, scalar.Anchor, scalar.Tag, scalar.Value));
			}

			protected override void Visit(YamlSequenceNode sequence)
			{
				events.Add(new YamlNodeEvent(YamlNodeEventType.SequenceStart, sequence.Anchor, sequence.Tag, null));
			}

			protected override void Visited(YamlSequenceNode sequence)
			{
				events.Add(new YamlNodeEvent(YamlNodeEventType.SequenceEnd, sequence.Anchor, sequence.Tag, null));
			}

			protected override void Visit(YamlMappingNode mapping)
			{
				events.Add(new YamlNodeEvent(YamlNodeEventType.MappingStart, mapping.Anchor, mapping.Tag, null));
			}

			protected override void Visited(YamlMappingNode mapping)
			{
				events.Add(new YamlNodeEvent(YamlNodeEventType.MappingEnd, mapping.Anchor, mapping.Tag, null));
			}
		}

		private void RoundtripTest(string yamlFileName)
		{
			YamlStream original = new YamlStream();
			original.Load(YamlFile(yamlFileName));

			StringBuilder buffer = new StringBuilder();
			original.Save(new StringWriter(buffer));

			Console.WriteLine(buffer.ToString());

			YamlStream final = new YamlStream();
			final.Load(new StringReader(buffer.ToString()));

			YamlDocumentStructureBuilder originalBuilder = new YamlDocumentStructureBuilder();
			original.Accept(originalBuilder);

			YamlDocumentStructureBuilder finalBuilder = new YamlDocumentStructureBuilder();
			final.Accept(finalBuilder);

			Console.WriteLine("The original document produced {0} events.", originalBuilder.Events.Count);
			Console.WriteLine("The final document produced {0} events.", finalBuilder.Events.Count);
			Assert.Equal(originalBuilder.Events.Count, finalBuilder.Events.Count);

			for (int i = 0; i < originalBuilder.Events.Count; ++i)
			{
				YamlNodeEvent originalEvent = originalBuilder.Events[i];
				YamlNodeEvent finalEvent = finalBuilder.Events[i];

				Assert.Equal(originalEvent.Type, finalEvent.Type);
				//Assert.Equal(originalEvent.Tag, finalEvent.Tag);
				//Assert.Equal(originalEvent.Anchor, finalEvent.Anchor);
				Assert.Equal(originalEvent.Value, finalEvent.Value);
			}
		}

		[Fact]
		public void RoundtripExample1()
		{
			RoundtripTest("test1.yaml");
		}

		[Fact]
		public void RoundtripExample2()
		{
			RoundtripTest("test2.yaml");
		}

		[Fact]
		public void RoundtripExample3()
		{
			RoundtripTest("test3.yaml");
		}

		[Fact]
		public void RoundtripExample4()
		{
			RoundtripTest("test4.yaml");
		}

		[Fact]
		public void RoundtripExample5()
		{
			RoundtripTest("test6.yaml");
		}

		[Fact]
		public void RoundtripExample6()
		{
			RoundtripTest("test6.yaml");
		}

		[Fact]
		public void RoundtripExample7()
		{
			RoundtripTest("test7.yaml");
		}

		[Fact]
		public void RoundtripExample8()
		{
			RoundtripTest("test8.yaml");
		}

		[Fact]
		public void RoundtripExample9()
		{
			RoundtripTest("test9.yaml");
		}

		[Fact]
		public void RoundtripExample10()
		{
			RoundtripTest("test10.yaml");
		}

		[Fact]
		public void RoundtripExample11()
		{
			RoundtripTest("test11.yaml");
		}

		[Fact]
		public void RoundtripExample12()
		{
			RoundtripTest("test12.yaml");
		}

		[Fact]
		public void RoundtripExample13()
		{
			RoundtripTest("test13.yaml");
		}

		[Fact]
		public void RoundtripExample14()
		{
			RoundtripTest("test14.yaml");
		}

		[Fact]
		public void RoundtripBackreference()
		{
			RoundtripTest("backreference.yaml");
		}

		[Fact]
		public void RoundtripSample()
		{
			YamlStream original = new YamlStream();
			original.Load(YamlFile("sample.yaml"));

			original.Accept(new TracingVisitor());
			
			//RoundtripTest("sample.yaml");
		}

		[Fact]
		public void FailBackreference()
		{
			//YamlStream original = new YamlStream();
			//original.Load(YamlFile("fail-backreference.yaml"));
			RoundtripTest("fail-backreference.yaml");
		}

		[Fact]
		public void AllAliasesMustBeResolved()
		{
			YamlStream original = new YamlStream();
			original.Load(YamlFile("invalid-reference.yaml"));

			var visitor = new AliasFindingVisitor();

			try
			{
				Assert.Throws<AnchorNotFoundException>(() => original.Accept(visitor));
			}
			catch (NotSupportedException)
			{
				foreach (var node in visitor.CurrentPath)
				{
					Console.WriteLine(node.ToString());
				}

				Assert.True(false);
			}
		}

		private class AliasFindingVisitor : YamlVisitor
		{
			private readonly Stack<string> _currentPath = new Stack<string>();

			protected override void Visit(YamlMappingNode mapping)
			{
				base.Visit(mapping);
			}

			protected override void VisitChildren(YamlSequenceNode sequence)
			{
				int index = 0;
				foreach (var child in sequence.Children)
				{
					_currentPath.Push(string.Format("seq[{0}]", index.ToString()));
					child.Accept(this);
					_currentPath.Pop();
				}
			}

			protected override void VisitChildren(YamlMappingNode mapping)
			{
				foreach (var child in mapping.Children)
				{
					_currentPath.Push(string.Format("map[{0}]", child.Key.ToString()));
					child.Value.Accept(this);
					_currentPath.Pop();
				}
			}

			public IEnumerable<string> CurrentPath { get { return _currentPath; } }
		}

	}
}