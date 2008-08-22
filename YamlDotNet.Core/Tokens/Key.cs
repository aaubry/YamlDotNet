
using System;

namespace YamlDotNet.Core.Tokens
{
	public class Key : Token {
		public Key()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		public Key(Mark start, Mark end)
			: base(start, end)
		{
		}
	}
}