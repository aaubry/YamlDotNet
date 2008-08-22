
using System;

namespace YamlDotNet.Core.Tokens
{
	public class BlockEnd : Token {
		public BlockEnd()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		public BlockEnd(Mark start, Mark end)
			: base(start, end)
		{
		}
	}
}