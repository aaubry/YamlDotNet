namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Represents an alias event.
	/// </summary>
	public interface IAnchorAlias : IParsingEvent
	{
		/// <summary>
		/// Gets the value of the alias.
		/// </summary>
		string Value
		{
			get;
		}
	}
}