using YamlDotNet.Events;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// A deserialized value used by <see cref="IYamlSerializable.ReadYaml"/> that
	/// can be a direct value or an alias. This is used to handle for forward alias.
	/// If an alias is found in a <see cref="ValueResult"/>, the caller usually
	/// register a late binding instruction through <see cref="SerializerContext.AddAliasBinding"/>
	/// that will be called once the whole document has been parsed, in order to 
	/// resolve all remaining aliases.
	/// </summary>
	public struct ValueResult
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ValueResult"/> struct that contains a value.
		/// </summary>
		/// <param name="value">The value.</param>
		public ValueResult(object value) : this()
		{
			Value = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ValueResult" /> struct that contains an <see cref="AnchorAlias"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		public ValueResult(AnchorAlias value)
		{
			Value = value;
		}

		/// <summary>
		/// The returned value or null if no value.
		/// </summary>
		public readonly object Value;

		/// <summary>
		/// True if this value result is an alias.
		/// </summary>
		public bool IsAlias
		{
			get { return Value is AnchorAlias; }
		}

		/// <summary>
		/// Gets the alias, only valid if <see cref="IsAlias"/> is true, null otherwise.
		/// </summary>
		/// <value>The alias.</value>
		public AnchorAlias Alias
		{
			get { return Value as AnchorAlias; }
		}
	}
}