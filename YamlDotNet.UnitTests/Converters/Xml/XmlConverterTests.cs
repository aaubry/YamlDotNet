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