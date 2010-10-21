using System;
using System.Collections.Generic;
using NUnit.Framework;
using YamlDotNet.RepresentationModel;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace YamlDotNet.UnitTests.RepresentationModel
{
	[TestFixture]
	public class YamlStreamTests : YamlTest
	{
		[Test]
		public void LoadSimpleDocument() {
			YamlStream stream = new YamlStream();
			stream.Load(YamlFile("test2.yaml"));
			
			Assert.AreEqual(1, stream.Documents.Count, "The stream should contain exactly one document.");
			Assert.IsInstanceOfType(typeof(YamlScalarNode), stream.Documents[0].RootNode, "The document should contain a scalar.");
			Assert.AreEqual("a scalar", ((YamlScalarNode)stream.Documents[0].RootNode).Value, "The value of the node is incorrect.");
		}
		
		[Test]
		public void BackwardAliasReferenceWorks() {
			YamlStream stream = new YamlStream();
			stream.Load(YamlFile("backwardsAlias.yaml"));
			
			Assert.AreEqual(1, stream.Documents.Count, "The stream should contain exactly one document.");
			Assert.IsInstanceOfType(typeof(YamlSequenceNode), stream.Documents[0].RootNode, "The document should contain a sequence.");

			YamlSequenceNode sequence = (YamlSequenceNode)stream.Documents[0].RootNode;
			Assert.AreEqual(3, sequence.Children.Count, "The sequence does not contain the correct number of children.");

			Assert.AreEqual("a scalar", ((YamlScalarNode)sequence.Children[0]).Value, "The value of the first node is incorrect.");
			Assert.AreEqual("another scalar", ((YamlScalarNode)sequence.Children[1]).Value, "The value of the second node is incorrect.");
			Assert.AreEqual("a scalar", ((YamlScalarNode)sequence.Children[2]).Value, "The value of the third node is incorrect.");
			Assert.AreSame(sequence.Children[0], sequence.Children[2], "The first and third element should be the same.");
		}
		
		[Test]
		public void ForwardAliasReferenceWorks() {
			YamlStream stream = new YamlStream();
			stream.Load(YamlFile("forwardAlias.yaml"));
			
			Assert.AreEqual(1, stream.Documents.Count, "The stream should contain exactly one document.");
			Assert.IsInstanceOfType(typeof(YamlSequenceNode), stream.Documents[0].RootNode, "The document should contain a sequence.");

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

		[Test]
		public void RoundtripExample1()
		{
			RoundtripTest("test1.yaml");
		}

		[Test]
		public void RoundtripExample2()
		{
			RoundtripTest("test2.yaml");
		}

		[Test]
		public void RoundtripExample3()
		{
			RoundtripTest("test3.yaml");
		}

		[Test]
		public void RoundtripExample4()
		{
			RoundtripTest("test4.yaml");
		}

		[Test]
		public void RoundtripExample5()
		{
			RoundtripTest("test6.yaml");
		}

		[Test]
		public void RoundtripExample6()
		{
			RoundtripTest("test6.yaml");
		}

		[Test]
		public void RoundtripExample7()
		{
			RoundtripTest("test7.yaml");
		}

		[Test]
		public void RoundtripExample8()
		{
			RoundtripTest("test8.yaml");
		}

		[Test]
		public void RoundtripExample9()
		{
			RoundtripTest("test9.yaml");
		}

		[Test]
		public void RoundtripExample10()
		{
			RoundtripTest("test10.yaml");
		}

		[Test]
		public void RoundtripExample11()
		{
			RoundtripTest("test11.yaml");
		}

		[Test]
		public void RoundtripExample12()
		{
			RoundtripTest("test12.yaml");
		}

		[Test]
		public void RoundtripExample13()
		{
			RoundtripTest("test13.yaml");
		}

		[Test]
		public void RoundtripExample14()
		{
			RoundtripTest("test14.yaml");
		}

		[Test]
		public void RoundtripBackreference()
		{
			RoundtripTest("backreference.yaml");
		}

		[Test]
		public void RoundtripSample()
		{
			YamlStream original = new YamlStream();
			original.Load(YamlFile("sample.yaml"));

			original.Accept(new TracingVisitor());
			
			//RoundtripTest("sample.yaml");
		}
	}
}