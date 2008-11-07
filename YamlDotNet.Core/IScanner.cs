using YamlDotNet.Core.Tokens;

namespace YamlDotNet.Core
{
	/// <summary>
	/// Defines the interface for a stand-alone YAML scanner that
	/// converts a sequence of characters into a sequence of YAML tokens.
	/// </summary>
	public interface IScanner
	{
		/// <summary>
		/// Gets the current position inside the input stream.
		/// </summary>
		/// <value>The current position.</value>
		Mark CurrentPosition
		{
			get;
		}

		/// <summary>
		/// Gets the current token.
		/// </summary>
		Token Current
		{
			get;
		}

		/// <summary>
		/// Moves to the next token.
		/// </summary>
		/// <returns></returns>
		bool MoveNext();

		/// <summary>
		/// Moves to the next token.
		/// </summary>
		/// <returns></returns>
		bool ParserMoveNext();

		/// <summary>
		/// Consumes the current token and increments the parsed token count
		/// </summary>
		void ConsumeCurrent();
	}
}