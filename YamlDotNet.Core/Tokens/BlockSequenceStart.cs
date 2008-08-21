
using System;

namespace YamlDotNet.CoreCs.Tokens
{
	public class BlockSequenceStart : Token {
		public BlockSequenceStart()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		public BlockSequenceStart(Mark start, Mark end)
			: base(start, end)
		{
		}
	}
}