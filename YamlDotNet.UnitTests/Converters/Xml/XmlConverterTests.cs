using System;
using System.Xml;
using NUnit.Framework;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Converters.Xml;
using YamlDotNet.Converters.Xml.Extensions;
using System.IO;

namespace YamlDotNet.UnitTests
{
	[TestFixture]
	public class XmlConverterTests : YamlTest
	{
		private class TracingVisitor : YamlVisitor {
			private int indent = 0;
			
			private void WriteIndent() {
				for(int i = 0; i < indent; ++i) {
					Console.Write("  ");
				}
			}
			
			protected override void Visit (YamlDocument document) {
				WriteIndent();				
				Console.WriteLine("Visit(YamlDocument)");
				++indent;
			}

			protected override void Visit (YamlMappingNode mapping)
			{
				WriteIndent();				
				Console.WriteLine("Visit(YamlMapping)");
				++indent;
			}
			
			protected override void Visit (YamlScalarNode scalar)
			{
				WriteIndent();				
				Console.WriteLine("Visit(YamlScalarNode) - {0}", scalar.Value);
				++indent;
			}

			protected override void Visit (YamlSequenceNode sequence)
			{
				WriteIndent();				
				Console.WriteLine("Visit(YamlSequenceNode)");
				++indent;
			}

			protected override void Visit (YamlStream stream)
			{
				WriteIndent();				
				Console.WriteLine("Visit(YamlStream)");
				++indent;
			}

			protected override void Visited (YamlDocument document)
			{
				--indent;
				WriteIndent();				
				Console.WriteLine("Visited(YamlDocument)");
			}
			
			protected override void Visited (YamlMappingNode mapping)
			{
				--indent;
				WriteIndent();				
				Console.WriteLine("Visited(YamlMappingNode)");
			}

			protected override void Visited (YamlScalarNode scalar)
			{
				--indent;
				WriteIndent();				
				Console.WriteLine("Visited(YamlScalarNode)");
			}

			protected override void Visited (YamlSequenceNode sequence)
			{
				--indent;
				WriteIndent();				
				Console.WriteLine("Visited(YamlSequenceNode)");
			}

			protected override void Visited (YamlStream stream)
			{
				--indent;
				WriteIndent();				
				Console.WriteLine("Visited(YamlStream)");
			}
		}
		
		private static YamlDocument GetDocument(string name) {
			YamlStream stream = new YamlStream();
			stream.Load(YamlFile(name));
			Assert.IsTrue(stream.Documents.Count > 0);
			return stream.Documents[0];
		}
		
		[Test]
		public void ScalarToXml() {
			YamlDocument yaml = GetDocument("test2.yaml");
			
			XmlConverter converter = new XmlConverter();
			XmlDocument xml = converter.ToXml(yaml);
			
			xml.Save(Console.Out);
		}			
		
		[Test]
		public void SequenceOfScalarsToXml() {
			YamlDocument yaml = GetDocument("test8.yaml");
			
			XmlConverter converter = new XmlConverter();
			XmlDocument xml = converter.ToXml(yaml);
			
			xml.Save(Console.Out);
		}			
		
		[Test]
		public void MappingOfScalarsToXml() {
			YamlDocument yaml = GetDocument("test9.yaml");

			XmlConverter converter = new XmlConverter();
			XmlDocument xml = converter.ToXml(yaml);
			
			xml.Save(Console.Out);
		}			
		
		[Test]
		public void SequenceOfMappingAndSequencesToXml() {
			YamlDocument yaml = GetDocument("test10.yaml");
			
			XmlConverter converter = new XmlConverter();
			XmlDocument xml = converter.ToXml(yaml);
			
			xml.Save(Console.Out);
		}			
		
		[Test]
		public void ToXmlUsingExtension() {
			YamlDocument yaml = GetDocument("test10.yaml");
			XmlDocument xml = yaml.ToXml();
			xml.Save(Console.Out);
		}			

		[Test]
		public void Roundtrip()
		{
			YamlDocument yaml = GetDocument("test10.yaml");

			XmlConverter converter = new XmlConverter();
			XmlDocument xml = converter.ToXml(yaml);

			StringWriter firstBuffer = new StringWriter();
			xml.Save(firstBuffer);
			Console.Out.Write(firstBuffer.ToString());

			YamlDocument intermediate = converter.FromXml(xml);
			XmlDocument final = converter.ToXml(intermediate);

			StringWriter secondBuffer = new StringWriter();
			final.Save(secondBuffer);
			Console.Error.Write(secondBuffer.ToString());

			Assert.AreEqual(firstBuffer.ToString(), secondBuffer.ToString(), "The first and second XML are different.");
		}
	}
}