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
using System.Xml;
using YamlDotNet.RepresentationModel;
using System.Globalization;
using System.Diagnostics;

namespace YamlDotNet.Converters.Xml
{
	/// <summary>
	/// Converts between <see cref="YamlDocument"/> and <see cref="XmlDocument"/>.
	/// </summary>
	public class XmlConverter
	{
		#region YamlToXmlDocumentVisitor
		private class YamlToXmlDocumentVisitor : YamlVisitor
		{
			private XmlDocument myDocument;
			private XmlNode current;
			private readonly XmlConverterOptions options;

			public XmlDocument Document
			{
				get
				{
					return myDocument;
				}
			}

			private void PushNode(string elementName)
			{
				XmlNode newNode = myDocument.CreateElement(elementName);
				current.AppendChild(newNode);
				current = newNode;
			}

			private void PopNode()
			{
				current = current.ParentNode;
			}

			public YamlToXmlDocumentVisitor(XmlConverterOptions options)
			{
				this.options = options;
			}

			protected override void Visit(YamlDocument document)
			{
				myDocument = new XmlDocument();
				current = myDocument;
				PushNode(options.RootElementName);
			}

			protected override void Visit(YamlScalarNode scalar)
			{
				current.AppendChild(myDocument.CreateTextNode(scalar.Value));
			}

			protected override void Visit(YamlSequenceNode sequence)
			{
				PushNode(options.SequenceElementName);
			}

			protected override void Visited(YamlSequenceNode sequence)
			{
				PopNode();
			}

			protected override void VisitChildren(YamlSequenceNode sequence)
			{
				foreach (var item in sequence.Children)
				{
					PushNode(options.SequenceItemElementName);
					item.Accept(this);
					PopNode();
				}
			}

			protected override void Visit(YamlMappingNode mapping)
			{
				PushNode(options.MappingElementName);
			}

			protected override void Visited(YamlMappingNode mapping)
			{
				PopNode();
			}

			protected override void VisitChildren(YamlMappingNode mapping)
			{
				foreach (var pair in mapping.Children)
				{
					PushNode(options.MappingEntryElementName);

					PushNode(options.MappingKeyElementName);
					pair.Key.Accept(this);
					PopNode();

					PushNode(options.MappingValueElementName);
					pair.Value.Accept(this);
					PopNode();

					PopNode();
				}
			}
		}
		#endregion

		#region XmlToYamlConverter
		private class XmlToYamlConverter
		{
			private struct ExitCaller : IDisposable
			{
				private readonly XmlToYamlConverter owner;

				public ExitCaller(XmlToYamlConverter owner)
				{
					this.owner = owner;
				}

				#region IDisposable Members
				public void Dispose()
				{
					owner.Exit();
				}
				#endregion
			}

			private readonly XmlConverterOptions options;
			private readonly XmlDocument document;
			private XmlNode current;
			private XmlNode currentParent;

			public XmlToYamlConverter(XmlDocument document, XmlConverterOptions options)
			{
				this.document = document;
				this.options = options;
			}

			public YamlDocument ParseDocument()
			{
				currentParent = document;
				current = document.DocumentElement;
				using (ExpectElement(options.RootElementName))
				{
					return new YamlDocument(ParseNode());
				}
			}

			private YamlNode ParseNode()
			{
				if(AcceptText())
				{
					return ParseScalar();
				}

				if (AcceptElement(options.SequenceElementName))
				{
					return ParseSequence();
				}

				if (AcceptElement(options.MappingElementName))
				{
					return ParseMapping();
				}

				throw new InvalidOperationException("Expected sequence, mapping or scalar.");
			}

			private YamlNode ParseMapping()
			{
				using(ExpectElement(options.MappingElementName))
				{
					YamlMappingNode mapping = new YamlMappingNode();
					while (AcceptElement(options.MappingEntryElementName))
					{
						using(ExpectElement(options.MappingEntryElementName))
						{
							YamlNode key;
							using(ExpectElement(options.MappingKeyElementName))
							{
								key = ParseNode();
							}

							YamlNode value;
							using(ExpectElement(options.MappingValueElementName))
							{
								value = ParseNode();
							}
							
							mapping.Children.Add(key, value);
						}
					}
					return mapping;
				}
			}

			private YamlNode ParseSequence()
			{
				using (ExpectElement(options.SequenceElementName))
				{
					YamlSequenceNode sequence = new YamlSequenceNode();
					while (AcceptElement(options.SequenceItemElementName))
					{
						using(ExpectElement(options.SequenceItemElementName))
						{
							sequence.Children.Add(ParseNode());
						}
					}
					return sequence;
				}
			}

			private YamlNode ParseScalar()
			{
				string text = ExpectText();
				Exit();
				return new YamlScalarNode(text);
			}

			#region Navigation methods
			private bool Accept(XmlNodeType nodeType, string elementName)
			{
				if (current == null)
				{
					return false;
				}

				if (current.NodeType != nodeType)
				{
					return false;
				}

				if (nodeType == XmlNodeType.Element)
				{
					if (current.LocalName != elementName)
					{
						return false;
					}
				}

				return true;
			}

			private bool AcceptText()
			{
				return Accept(XmlNodeType.Text, null);
			}

			private bool AcceptElement(string elementName)
			{
				return Accept(XmlNodeType.Element, elementName);
			}

			private XmlNode Expect(XmlNodeType nodeType, string elementName)
			{
				if(!Accept(nodeType, elementName))
				{
					if (nodeType == XmlNodeType.Text)
					{
						throw new InvalidOperationException(string.Format(
							CultureInfo.InvariantCulture,
							"Expected node type '{0}', got '{1}'.",
							nodeType,
							current.NodeType
						));
					}
					else
					{
						throw new InvalidOperationException(string.Format(
							CultureInfo.InvariantCulture,
							"Expected element '{0}', got '{1}'.",
							elementName,
							current.LocalName
						));
					}
				}

				currentParent = current;
				current = current.FirstChild;
				return currentParent;
			}

			private IDisposable ExpectElement(string elementName)
			{
				Expect(XmlNodeType.Element, elementName);
				return new ExitCaller(this);
			}

			private string ExpectText()
			{
				return Expect(XmlNodeType.Text, null).Value;
			}

			private void Exit()
			{
				Debug.Assert(current == null);
				current = currentParent.NextSibling;
				currentParent = currentParent.ParentNode;
			}
			#endregion
		}
		#endregion

		private readonly XmlConverterOptions options;

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlConverter"/> class.
		/// </summary>
		/// <param name="options">The options.</param>
		public XmlConverter(XmlConverterOptions options)
		{
			this.options = options.IsReadOnly ? options : options.AsReadOnly();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlConverter"/> class.
		/// </summary>
		public XmlConverter()
			: this(XmlConverterOptions.Default)
		{
		}

		/// <summary>
		/// Converts a <see cref="YamlDocument"/> to <see cref="XmlDocument"/>.
		/// </summary>
		/// <param name="document">The YAML document to convert.</param>
		/// <returns></returns>
		public XmlDocument ToXml(YamlDocument document)
		{
			YamlToXmlDocumentVisitor visitor = new YamlToXmlDocumentVisitor(options);
			document.Accept(visitor);
			return visitor.Document;
		}

		/// <summary>
		/// Converts a <see cref="XmlDocument"/> to <see cref="YamlDocument"/>.
		/// </summary>
		/// <param name="document">The XML document to convert.</param>
		/// <returns></returns>
		public YamlDocument FromXml(XmlDocument document)
		{
			XmlToYamlConverter converter = new XmlToYamlConverter(document, options);
			return converter.ParseDocument();
		}
	}
}