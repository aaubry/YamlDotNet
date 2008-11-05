using System;
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
