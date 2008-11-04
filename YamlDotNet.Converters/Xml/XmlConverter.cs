using System;
using System.Collections.Generic;
using System.Xml;
using YamlDotNet.RepresentationModel;

namespace YamlDotNet.Converters.Xml
{
	public class XmlConverter
	{
		private class YamlToXmlDocumentVisitor : YamlVisitor {
			private XmlDocument myDocument;
			private XmlNode current;
			private readonly Stack<string> states = new Stack<string>();
			
			public XmlDocument Document {
				get {
					return myDocument;
				}
			}
			
			private XmlNode AddNode() {
				XmlNode newNode = myDocument.CreateElement(states.Peek());
				current.AppendChild(newNode);
				return newNode;
			}
			
			private void PushNode(string childElementName) {
				current = AddNode();
				states.Push(childElementName);
			}
			
			private void PopNode() {
				current = current.ParentNode;
				states.Pop();
			}
			
			public YamlToXmlDocumentVisitor(string rootElementName) {
				states.Push(rootElementName);
			}
			
			protected override void Visit(YamlDocument document)
			{
				myDocument = new XmlDocument();
				current = myDocument; 
			}

			protected override void Visit(YamlScalarNode scalar)
			{
				XmlNode scalarNode = AddNode();
				scalarNode.AppendChild(myDocument.CreateTextNode(scalar.Value));
			}
			
			protected override void Visit(YamlSequenceNode sequence)
			{
				PushNode("sequence"); 
			}
			
			protected override void Visited(YamlSequenceNode sequence)
			{
				PopNode();
			}
			
			protected override void Visit(YamlMappingNode mapping)
			{
				PushNode("mapping");
			}
			
			protected override void Visited(YamlMappingNode mapping)
			{
				PopNode();
			}
			
			protected override void VisitChildren (YamlMappingNode mapping)
			{
				foreach (var pair in mapping.Children) {
					PushNode("key");
					pair.Key.Accept(this);
					states.Pop();
					states.Push("value");
					pair.Value.Accept(this);
					PopNode();
				}
			}
		}
		
		public XmlDocument ToXml(YamlDocument document, string rootElementName) {
			YamlToXmlDocumentVisitor visitor = new YamlToXmlDocumentVisitor(rootElementName);
			document.Accept(visitor);
			return visitor.Document;
		}
		
		public YamlDocument FromXml(XmlDocument document) {
			return null;
		}
	}
}