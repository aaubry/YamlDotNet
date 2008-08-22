
using System;

namespace YamlDotNet.Core.Tokens
{
	public class Value : Token {
		public Value()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		public Value(Mark start, Mark end)
			: base(start, end)
		{
		}
	}
}