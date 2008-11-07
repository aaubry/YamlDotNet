namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Represents a document end event.
	/// </summary>
	public interface IDocumentEnd : IParsingEvent
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
	}
}