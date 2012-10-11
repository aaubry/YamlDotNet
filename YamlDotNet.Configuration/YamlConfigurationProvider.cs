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

﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel.Serialization;

namespace YamlDotNet.Configuration
{
	/// <summary>
	/// Configuration provider that loads a YAML stream.
	/// </summary>
	public sealed class YamlConfigurationProvider : IConfigurationProvider
	{
		private IDictionary<string, object> sections;

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlConfigurationProvider"/> class.
		/// </summary>
		public YamlConfigurationProvider()
			: this(BuildDefaultFilename())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlConfigurationProvider"/> class.
		/// </summary>
		/// <param name="fileName">The fileName.</param>
		public YamlConfigurationProvider(string fileName)
		{
			using (TextReader yaml = File.OpenText(fileName))
			{
				LoadSections(yaml);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlConfigurationProvider"/> class.
		/// </summary>
		/// <param name="yaml">The yaml.</param>
		public YamlConfigurationProvider(TextReader yaml)
		{
			LoadSections(yaml);
		}

		private static string BuildDefaultFilename()
		{
			return Regex.Replace(
				ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath,
				@"\.config$",
				".yaml",
				RegexOptions.IgnoreCase
			);
		}

		private void LoadSections(TextReader yaml)
		{
			YamlSerializer serializer = new YamlSerializer(typeof(Dictionary<string, object>));
			sections = (IDictionary<string, object>)serializer.Deserialize(yaml);
		}

		#region IConfigurationProvider Members
		object IConfigurationProvider.GetSection(string name)
		{
			object section;
			if (sections.TryGetValue(name, out section))
			{
				return section;
			}
			return null;
		}
		#endregion
	}

}
