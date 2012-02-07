//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2008, 2009, 2010, 2011, 2012 Antoine Aubry
    
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
using YamlDotNet.Converters.Xml;
using YamlDotNet.RepresentationModel;

namespace YamlDotNet.Converters.Xml.Extensions {
	/// <summary>
	/// Defines extension methods for calling the methods of <see cref="XmlConverter"/>.
	/// </summary>
	public static class XmlConverterExtensions
	{
		/// <summary>
		/// Invokes <see cref="XmlConverter.ToXml"/>.
		/// </summary>
		public static XmlDocument ToXml(this YamlDocument document, XmlConverterOptions options)
		{
			XmlConverter converter = new XmlConverter(options);
			return converter.ToXml(document);
		}

		/// <summary>
		/// Invokes <see cref="XmlConverter.ToXml"/>.
		/// </summary>
		public static XmlDocument ToXml(this YamlDocument document)
		{
			XmlConverter converter = new XmlConverter();
			return converter.ToXml(document);
		}
	}
}