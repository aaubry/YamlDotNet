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

using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using YamlDotNet.Serialization;

namespace YamlDotNet.Configuration
{
	/// <summary>
	/// YAML-based configuration section. Allows to declare configuration sections that obtain their content
	/// by deserializing a YAML stream.
	/// </summary>
	/// <remarks>
	/// By default, the configuration section loads the content of the element. The type of that content must
	/// be defined in the stream itself. Alterantively, the <code>type</code> attribute may be used to specify
	/// the type of the section. The configuration element can have the <code>file</code> attribute, which
	/// specifies a file to be loaded instead of the content of the element. In that case, the element's content
	/// is ignored.
	/// </remarks>
	/// <example>
	/// App.config:
	/// 
	/// <code>
	/// &lt;?xml version="1.0" encoding="utf-8" ?&gt;
	/// &lt;configuration&gt;
	/// 	&lt;configSections&gt;
	/// 		&lt;section name="MyConfiguration1" type="YamlDotNet.Configuration.YamlConfigurationSection, YamlDotNet.Configuration" /&gt;
	/// 		&lt;section name="MyConfiguration2" type="YamlDotNet.Configuration.YamlConfigurationSection, YamlDotNet.Configuration" /&gt;
	/// 		&lt;section name="MyConfiguration3" type="YamlDotNet.Configuration.YamlConfigurationSection, YamlDotNet.Configuration" /&gt;
	/// 		&lt;section name="MyConfiguration4" type="YamlDotNet.Configuration.YamlConfigurationSection, YamlDotNet.Configuration" /&gt;
	/// 	&lt;/configSections&gt;
	/// 
	/// 	&lt;MyConfiguration1&gt;
	/// 		&lt;![CDATA[
	/// 			!&lt;!MyConfiguration,MyAssembly&gt; {
	/// 				myString: a string,
	/// 				myInt: 1
	/// 			}
	/// 		]]&gt;
	/// 	&lt;/MyConfiguration1&gt;
	/// 
	/// 	&lt;MyConfiguration2 type="MyConfiguration, MyAssembly"&gt;
	/// 		&lt;![CDATA[
	/// 			myString: a string
	/// 			myInt: 2
	/// 		]]&gt;
	/// 	&lt;/MyConfiguration2&gt;
	/// 
	/// 	&lt;MyConfiguration3 file="explicit.yaml" /&gt;
	/// 
	/// 	&lt;MyConfiguration4 type="MyConfiguration, MyAssembly" file="implicit.yaml" /&gt;
	/// &lt;/configuration&gt;
	/// </code>
	/// 
	/// explicit.yaml:
	/// 
	/// <code>
	/// !&lt;!MyConfiguration,MyAssembly&gt; {
	/// 	myString: a string,
	/// 	myInt: 3
	/// }
	/// </code>
	/// 
	/// implicit.yaml:
	/// 
	/// <code>
	/// myString: a string
	/// myInt: 4
	/// </code>
	/// </example>
	public sealed class YamlConfigurationSection : IConfigurationSectionHandler
	{
		private static readonly Regex indentParser = new Regex(@"^\s*", RegexOptions.Compiled);

		#region IConfigurationSectionHandler Members
		object IConfigurationSectionHandler.Create(object parent, object configContext, XmlNode section)
		{
			TextReader yaml;
			if (section.Attributes["file"] != null)
			{
				string fileName = section.Attributes["file"].Value;
				if (!File.Exists(fileName))
				{
					string configPath = Path.GetDirectoryName(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath);
					fileName = Path.Combine(configPath, fileName);
				}

				yaml = File.OpenText(fileName);
			}
			else
			{
				yaml = GetYamlContent(section);
			}

			var sectionType = typeof(object);
			if (section.Attributes["type"] != null)
			{
				sectionType = Type.GetType(section.Attributes["type"].Value, true);
			}

			var deserializer = new Serializer();
			return deserializer.Deserialize(yaml, sectionType);
		}
		#endregion

		#region Implementation details
		private static TextReader GetYamlContent(XmlNode section)
		{
			string[] lines = section.InnerText.Split('\n');

			int firstNonEmptyLineIndex = -1;
			int lastNonEmptyLineIndex = -2;

			for (int i = 0; i < lines.Length; ++i)
			{
				lines[i] = lines[i].TrimEnd('\r', '\t', ' ');
				if (lines[i].Length != 0)
				{
					if (firstNonEmptyLineIndex == -1)
					{
						firstNonEmptyLineIndex = i;
					}
					lastNonEmptyLineIndex = i;
				}
			}

			StringBuilder yamlText = new StringBuilder();
			if (firstNonEmptyLineIndex <= lastNonEmptyLineIndex)
			{
				int indent = indentParser.Match(lines[firstNonEmptyLineIndex]).Length;

				for (int i = firstNonEmptyLineIndex; i <= lastNonEmptyLineIndex; ++i)
				{
					yamlText.AppendLine(lines[i].Substring(indent));
				}
			}

			return new StringReader(yamlText.ToString());
		}
		#endregion
	}
}
