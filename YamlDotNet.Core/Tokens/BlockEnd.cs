
using System;

namespace YamlDotNet.Core.Tokens
{
	/// <summary>
	/// Represents a block end token.
	/// </summary>
	public class BlockEnd : Token
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BlockEnd"/> class.
		/// </summary>
		public BlockEnd()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BlockEnd"/> class.
		/// </summary>
		/// <param name="start">The start position of the token.</param>
		/// <param name="end">The end position of the token.</param>
		public BlockEnd(Mark start, Mark end)
			: base(start, end)
		{
		}
	}
}