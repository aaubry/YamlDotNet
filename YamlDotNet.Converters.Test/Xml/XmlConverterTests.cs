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

using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Converters.Xml;
using YamlDotNet.Converters.Xml.Extensions;
using YamlDotNet.Test;
using System.IO;

namespace YamlDotNet.Converters.Test.Xml
{
	public class XmlConverterTests : YamlTest
	{
		[Fact]
		public void ScalarToXml() {
			var writer = new StringWriter();
			var yaml = GetDocument("test2.yaml");

			var xml = new XmlConverter().ToXml(yaml);

			xml.Save(writer);
			Dump.Write(writer);
		}

		[Fact]
		public void SequenceOfScalarsToXml() {
			var writer = new StringWriter();
			var yaml = GetDocument("test8.yaml");

			var xml = new XmlConverter().ToXml(yaml);

			xml.Save(writer);
			Dump.Write(writer);
		}

		[Fact]
		public void MappingOfScalarsToXml() {
			var writer = new StringWriter();
			var yaml = GetDocument("test9.yaml");

			var xml = new XmlConverter().ToXml(yaml);

			xml.Save(writer);
			Dump.Write(writer);
		}

		[Fact]
		public void SequenceOfMappingAndSequencesToXml() {
			var writer = new StringWriter();
			var yaml = GetDocument("test10.yaml");

			var xml = new XmlConverter().ToXml(yaml);

			xml.Save(writer);
			Dump.Write(writer);
		}

		[Fact]
		public void ToXmlUsingExtension() {
			var writer = new StringWriter();
			var yaml = GetDocument("test10.yaml");

			var xml = yaml.ToXml();

			xml.Save(writer);
			Dump.Write(writer);
		}

		[Fact]
		public void Roundtrip()
		{
			var yaml = GetDocument("test10.yaml");

			var converter = new XmlConverter();
			var xml = converter.ToXml(yaml);

			var firstBuffer = new StringWriter();
			xml.Save(firstBuffer);
			Dump.Write(firstBuffer);

			var intermediate = converter.FromXml(xml);
			var final = converter.ToXml(intermediate);

			var secondBuffer = new StringWriter();
			final.Save(secondBuffer);
			Dump.Write(secondBuffer);

			Assert.Equal(firstBuffer.ToString(), secondBuffer.ToString());
		}

		private static YamlDocument GetDocument(string name)
		{
			var stream = new YamlStream();
			stream.Load(YamlFile(name));
			Assert.True(stream.Documents.Count > 0, "The file [" + name + "] did not contain any Yaml documents");
			return stream.Documents[0];
		}
	}
}