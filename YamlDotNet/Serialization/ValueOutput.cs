using YamlDotNet.Events;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// A deserialized value used by <see cref="IYamlSerializable.ReadYaml"/> that
	/// can be a direct value or an alias. This is used to handle for forward alias.
	/// If an alias is found in a <see cref="ValueOutput"/>, the caller usually
	/// register a late binding instruction through <see cref="SerializerContext.AddAliasBinding"/>
	/// that will be called once the whole document has been parsed, in order to 
	/// resolve all remaining aliases.
	/// </summary>
	public struct ValueOutput
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ValueOutput"/> struct that contains a value.
		/// </summary>
		/// <param name="value">The value.</param>
		public ValueOutput(object value) : this()
		{
			Value = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ValueOutput" /> struct that contains an <see cref="AnchorAlias"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		public ValueOutput(AnchorAlias value)
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

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>A <see cref="System.String" /> that represents this instance.</returns>
		public override string ToString()
		{
			return string.Format("{0}", Value);
		}
	}
}