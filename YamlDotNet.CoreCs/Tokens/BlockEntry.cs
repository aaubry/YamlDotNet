
using System;

namespace YamlDotNet.CoreCs.Tokens
{
	public class BlockEntry : Token {
		public BlockEntry()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		public BlockEntry(Mark start, Mark end)
			: base(start, end)
		{
		}
	}
}