using System;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Instructs the <see cref="Deserializer"/> to use a different field name for serialization.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public class YamlAliasAttribute : Attribute
	{
		/// <summary>
		/// Gets or sets the alias name.
		/// </summary>
		public string Alias { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlAliasAttribute" /> class.
		/// </summary>
		/// <param name="alias">The alias to use for this field.</param>
		public YamlAliasAttribute(string alias)
		{
			Alias = alias;
		}
	}
}
