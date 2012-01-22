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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Converters.Xml;
using YamlDotNet.Converters.Xml.Extensions;
using System.IO;

namespace YamlDotNet.UnitTests
{
	[TestClass]
	public class XmlConverterTests : YamlTest
	{
		private static YamlDocument GetDocument(string name) {
			YamlStream stream = new YamlStream();
			stream.Load(YamlFile(name));
			Assert.IsTrue(stream.Documents.Count > 0);
			return stream.Documents[0];
		}
		
		[TestMethod]
		public void ScalarToXml() {
			YamlDocument yaml = GetDocument("test2.yaml");
			
			XmlConverter converter = new XmlConverter();
			XmlDocument xml = converter.ToXml(yaml);
			
			xml.Save(Console.Out);
		}			
		
		[TestMethod]
		public void SequenceOfScalarsToXml() {
			YamlDocument yaml = GetDocument("test8.yaml");
			
			XmlConverter converter = new XmlConverter();
			XmlDocument xml = converter.ToXml(yaml);
			
			xml.Save(Console.Out);
		}			
		
		[TestMethod]
		public void MappingOfScalarsToXml() {
			YamlDocument yaml = GetDocument("test9.yaml");

			XmlConverter converter = new XmlConverter();
			XmlDocument xml = converter.ToXml(yaml);
			
			xml.Save(Console.Out);
		}			
		
		[TestMethod]
		public void SequenceOfMappingAndSequencesToXml() {
			YamlDocument yaml = GetDocument("test10.yaml");
			
			XmlConverter converter = new XmlConverter();
			XmlDocument xml = converter.ToXml(yaml);
			
			xml.Save(Console.Out);
		}			
		
		[TestMethod]
		public void ToXmlUsingExtension() {
			YamlDocument yaml = GetDocument("test10.yaml");
			XmlDocument xml = yaml.ToXml();
			xml.Save(Console.Out);
		}			

		[TestMethod]
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