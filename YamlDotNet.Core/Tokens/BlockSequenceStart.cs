
using System;

namespace YamlDotNet.Core.Tokens
{
	/// <summary>
	/// Represents a block sequence start token.
	/// </summary>
	public class BlockSequenceStart : Token
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BlockSequenceStart"/> class.
		/// </summary>
		public BlockSequenceStart()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BlockSequenceStart"/> class.
		/// </summary>
		/// <param name="start">The start position of the token.</param>
		/// <param name="end">The end position of the token.</param>
		public BlockSequenceStart(Mark start, Mark end)
			: base(start, end)
		{
		}
	}
}