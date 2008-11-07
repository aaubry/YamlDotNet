namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Base interface for parsing events.
	/// </summary>
	public interface IParsingEvent
	{
		/// <summary>
		/// Gets the position in the input stream where the event start.
		/// </summary>
		Mark Start
		{
			get;
		}

		/// <summary>
		/// Gets the position in the input stream where the event ends.
		/// </summary>
		Mark End
		{
			get;
		}
	}
}