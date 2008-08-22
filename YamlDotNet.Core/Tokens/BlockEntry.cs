
using System;

namespace YamlDotNet.Core.Tokens
{
	/// <summary>
	/// Represents a block entry event.
	/// </summary>
	public class BlockEntry : Token
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BlockEntry"/> class.
		/// </summary>
		public BlockEntry()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BlockEntry"/> class.
		/// </summary>
		/// <param name="start">The start position of the token.</param>
		/// <param name="end">The end position of the token.</param>
		public BlockEntry(Mark start, Mark end)
			: base(start, end)
		{
		}
	}
}