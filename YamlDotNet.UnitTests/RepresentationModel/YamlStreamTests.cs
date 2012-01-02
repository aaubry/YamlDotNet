//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2008, 2009, 2010, 2011 Antoine Aubry
    
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YamlDotNet.RepresentationModel;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace YamlDotNet.UnitTests.RepresentationModel
{
	[TestClass]
	public class YamlStreamTests : YamlTest
	{
		public TestContext TestContext { get; set; }

		[TestMethod]
		public void LoadSimpleDocument() {
			YamlStream stream = new YamlStream();
			stream.Load(YamlFile("test2.yaml"));
			
			Assert.AreEqual(1, stream.Documents.Count, "The stream should contain exactly one document.");
			Assert.IsInstanceOfType(stream.Documents[0].RootNode, typeof(YamlScalarNode), "The document should contain a scalar.");
			Assert.AreEqual("a scalar", ((YamlScalarNode)stream.Documents[0].RootNode).Value, "The value of the node is incorrect.");
		}
		
		[TestMethod]
		public void BackwardAliasReferenceWorks() {
			YamlStream stream = new YamlStream();
			stream.Load(YamlFile("backwardsAlias.yaml"));
			
			Assert.AreEqual(1, stream.Documents.Count, "The stream should contain exactly one document.");
			Assert.IsInstanceOfType(stream.Documents[0].RootNode, typeof(YamlSequenceNode), "The document should contain a sequence.");

			YamlSequenceNode sequence = (YamlSequenceNode)stream.Documents[0].RootNode;
			Assert.AreEqual(3, sequence.Children.Count, "The sequence does not contain the correct number of children.");

			Assert.AreEqual("a scalar", ((YamlScalarNode)sequence.Children[0]).Value, "The value of the first node is incorrect.");
			Assert.AreEqual("another scalar", ((YamlScalarNode)sequence.Children[1]).Value, "The value of the second node is incorrect.");
			Assert.AreEqual("a scalar", ((YamlScalarNode)sequence.Children[2]).Value, "The value of the third node is incorrect.");
			Assert.AreSame(sequence.Children[0], sequence.Children[2], "The first and third element should be the same.");
		}
		
		[TestMethod]
		public void ForwardAliasReferenceWorks() {
			YamlStream stream = new YamlStream();
			stream.Load(YamlFile("forwardAlias.yaml"));
			
			Assert.AreEqual(1, stream.Documents.Count, "The stream should contain exactly one document.");
			Assert.IsInstanceOfType(stream.Documents[0].RootNode, typeof(YamlSequenceNode), "The document should contain a sequence.");

			YamlSequenceNode sequence = (YamlSequenceNode)stream.Documents[0].RootNode;
			Assert.AreEqual(3, sequence.Children.Count, "The sequence does not contain the correct number of children.");

			Assert.AreEqual("a scalar", ((YamlScalarNode)sequence.Children[0]).Value, "The value of the first node is incorrect.");
			Assert.AreEqual("another scalar", ((YamlScalarNode)sequence.Children[1]).Value, "The value of the second node is incorrect.");
			Assert.AreEqual("a scalar", ((YamlScalarNode)sequence.Children[2]).Value, "The value of the third node is incorrect.");
			Assert.AreSame(sequence.Children[0], sequence.Children[2], "The first and third element should be the same.");
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

			TestContext.WriteLine(buffer.ToString());

			YamlStream final = new YamlStream();
			final.Load(new StringReader(buffer.ToString()));

			YamlDocumentStructureBuilder originalBuilder = new YamlDocumentStructureBuilder();
			original.Accept(originalBuilder);

			YamlDocumentStructureBuilder finalBuilder = new YamlDocumentStructureBuilder();
			final.Accept(finalBuilder);

			Console.WriteLine("The original document produced {0} events.", originalBuilder.Events.Count);
			Console.WriteLine("The final document produced {0} events.", finalBuilder.Events.Count);
			Assert.AreEqual(originalBuilder.Events.Count, finalBuilder.Events.Count, "The two documents should have the same structure.");

			for (int i = 0; i < originalBuilder.Events.Count; ++i)
			{
				YamlNodeEvent originalEvent = originalBuilder.Events[i];
				YamlNodeEvent finalEvent = finalBuilder.Events[i];

				Assert.AreEqual(originalEvent.Type, finalEvent.Type, "The type of the events should be the same.");
				//Assert.AreEqual(originalEvent.Tag, finalEvent.Tag, "The tag of the events should be the same.");
				//Assert.AreEqual(originalEvent.Anchor, finalEvent.Anchor, "The anchor of the events should be the same.");
				Assert.AreEqual(originalEvent.Value, finalEvent.Value, "The value of the events should be the same.");
			}
		}

		[TestMethod]
		public void RoundtripExample1()
		{
			RoundtripTest("test1.yaml");
		}

		[TestMethod]
		public void RoundtripExample2()
		{
			RoundtripTest("test2.yaml");
		}

		[TestMethod]
		public void RoundtripExample3()
		{
			RoundtripTest("test3.yaml");
		}

		[TestMethod]
		public void RoundtripExample4()
		{
			RoundtripTest("test4.yaml");
		}

		[TestMethod]
		public void RoundtripExample5()
		{
			RoundtripTest("test6.yaml");
		}

		[TestMethod]
		public void RoundtripExample6()
		{
			RoundtripTest("test6.yaml");
		}

		[TestMethod]
		public void RoundtripExample7()
		{
			RoundtripTest("test7.yaml");
		}

		[TestMethod]
		public void RoundtripExample8()
		{
			RoundtripTest("test8.yaml");
		}

		[TestMethod]
		public void RoundtripExample9()
		{
			RoundtripTest("test9.yaml");
		}

		[TestMethod]
		public void RoundtripExample10()
		{
			RoundtripTest("test10.yaml");
		}

		[TestMethod]
		public void RoundtripExample11()
		{
			RoundtripTest("test11.yaml");
		}

		[TestMethod]
		public void RoundtripExample12()
		{
			RoundtripTest("test12.yaml");
		}

		[TestMethod]
		public void RoundtripExample13()
		{
			RoundtripTest("test13.yaml");
		}

		[TestMethod]
		public void RoundtripExample14()
		{
			RoundtripTest("test14.yaml");
		}

		[TestMethod]
		public void RoundtripBackreference()
		{
			RoundtripTest("backreference.yaml");
		}

		[TestMethod]
		public void RoundtripSample()
		{
			YamlStream original = new YamlStream();
			original.Load(YamlFile("sample.yaml"));

			original.Accept(new TracingVisitor());
			
			//RoundtripTest("sample.yaml");
		}

		[TestMethod]
		public void FailBackreference()
		{
			//YamlStream original = new YamlStream();
			//original.Load(YamlFile("fail-backreference.yaml"));
			RoundtripTest("fail-backreference.yaml");
		}

		[TestMethod]
		[ExpectedException(typeof(AnchorNotFoundException))]
		public void AllAliasesMustBeResolved()
		{
			YamlStream original = new YamlStream();
			original.Load(YamlFile("invalid-reference.yaml"));

			var visitor = new AliasFindingVisitor();

			try
			{
				original.Accept(visitor);
			}
			catch (NotSupportedException)
			{
				foreach (var node in visitor.CurrentPath)
				{
					TestContext.WriteLine(node.ToString());
				}

				Assert.Fail();
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