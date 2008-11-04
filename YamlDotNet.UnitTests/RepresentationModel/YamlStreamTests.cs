using System;
using NUnit.Framework;
using YamlDotNet.RepresentationModel;

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
	}
}