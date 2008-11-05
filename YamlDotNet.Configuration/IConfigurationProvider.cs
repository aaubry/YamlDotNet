using System;

namespace YamlDotNet.Configuration
{
	/// <summary>
	/// Defines an interface for obtaining configuration sections.
	/// </summary>
	public interface IConfigurationProvider
	{
		/// <summary>
		/// Gets the configuration section with the specified name.
		/// </summary>
		/// <param name="name">The name of the configuration section.</param>
		/// <returns></returns>
		object GetSection(string name);
	}
}