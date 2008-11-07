namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Defines the behavior that is common between node events.
	/// </summary>
	public interface INodeEvent : IParsingEvent
	{
		/// <summary>
		/// Gets the anchor.
		/// </summary>
		string Anchor
		{
			get;
		}

		/// <summary>
		/// Gets the tag.
		/// </summary>
		string Tag
		{
			get;
		}

		/// <summary>
		/// Gets a value indicating whether this instance is canonical.
		/// </summary>
		bool IsCanonical
		{
			get;
		}
	}
}