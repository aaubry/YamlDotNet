using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;

namespace YamlDotNet.Configuration
{
	/// <summary>
	/// Loads configuration from various providers.
	/// </summary>
	/// <remarks>
	/// ConfigurationLoader is always initialized with a single provider: <see cref="DotNetConfigurationProvider"/>.
	/// </remarks>
	public class ConfigurationLoader
	{
		private ConfigurationLoader()
		{
		}

		/// <summary>
		/// Gets the only instance of <see cref="ConfigurationLoader" />.
		/// </summary>
		public static readonly ConfigurationLoader Instance = new ConfigurationLoader();

		private readonly HashSet<IConfigurationProvider> providers = new HashSet<IConfigurationProvider>
		{
			new DotNetConfigurationProvider()                                     	
		};

		/// <summary>
		/// Registers a new configuration provider.
		/// </summary>
		/// <param name="provider">The configuration provider.</param>
		public void Register(IConfigurationProvider provider)
		{
			if (provider == null)
			{
				throw new ArgumentNullException("provider");
			}

			providers.Add(provider);
		}

		/// <summary>
		/// Gets the configuration section with the specified name.
		/// </summary>
		/// <param name="name">The name of the configuration section.</param>
		/// <returns></returns>
		/// <exception cref="ConfigurationErrorsException">The specified section does not exist.</exception>
		public object GetSection(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}

			foreach (var provider in providers)
			{
				object section = provider.GetSection(name);
				if (section != null)
				{
					return section;
				}
			}

			throw new ConfigurationErrorsException(string.Format(CultureInfo.InvariantCulture, "Configuration section '{0}' does not exist.", name));
		}
	}
}
