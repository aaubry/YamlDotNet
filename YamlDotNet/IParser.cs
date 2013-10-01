using YamlDotNet.Core.Events;

namespace YamlDotNet.Core
{
	/// <summary>
	/// Represents a YAML stream paser.
	/// </summary>
	public interface IParser
	{
		/// <summary>
		/// Gets the current event.
		/// </summary>
		ParsingEvent Current { get; }

		/// <summary>
		/// Moves to the next event.
		/// </summary>
		/// <returns>Returns true if there are more events available, otherwise returns false.</returns>
		bool MoveNext();
	}
}