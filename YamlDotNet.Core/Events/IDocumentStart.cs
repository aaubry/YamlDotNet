using YamlDotNet.Core.Tokens;

namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Represents a document start event.
	/// </summary>
	public interface IDocumentStart : IParsingEvent
	{
		/// <summary>
		/// Gets the tags.
		/// </summary>
		/// <value>The tags.</value>
		TagDirectiveCollection Tags
		{
			get;
		}

		/// <summary>
		/// Gets the version.
		/// </summary>
		/// <value>The version.</value>
		VersionDirective Version
		{
			get;
		}

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