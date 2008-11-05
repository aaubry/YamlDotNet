using System;
using System.Configuration;

namespace YamlDotNet.Configuration
{
	/// <summary>
	/// Provider for the standard configuration system.
	/// </summary>
	public sealed class DotNetConfigurationProvider : IConfigurationProvider
	{
		#region IConfigurationProvider Members
		object IConfigurationProvider.GetSection(string name)
		{
			return ConfigurationManager.GetSection(name);
		}
		#endregion
	}
}