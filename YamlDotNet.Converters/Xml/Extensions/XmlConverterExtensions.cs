using System;
using System.Xml;
using YamlDotNet.Converters.Xml;
using YamlDotNet.RepresentationModel;

namespace YamlDotNet.Converters.Xml.Extensions {
	/// <summary>
	/// Defines extension methods for calling the methods of <see cref="XmlConverter"/>.
	/// </summary>
	public static class XmlConverterExtensions
	{
		public static XmlDocument ToXml(this YamlDocument document, string rootElementName) {
			XmlConverter converter = new XmlConverter();
			return converter.ToXml(document, rootElementName);
		}

		public static XmlDocument ToXml(this YamlDocument document) {
			return document.ToXml("root");
		}
	}
}