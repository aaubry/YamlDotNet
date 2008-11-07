namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Represents a mapping start event.
	/// </summary>
	public interface IMappingStart : INodeEvent
	{
		/// <summary>
		/// Gets a value indicating whether this instance is implicit.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is implicit; otherwise, <c>false</c>.
		/// </value>
		bool IsImplicit
		{
			get;
		}

		/// <summary>
		/// Gets the style of the mapping.
		/// </summary>
		MappingStyle Style
		{
			get;
		}
	}
}