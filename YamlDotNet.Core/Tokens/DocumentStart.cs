
using System;

namespace YamlDotNet.Core.Tokens
{
	public class DocumentStart : Token {
		public DocumentStart()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		public DocumentStart(Mark start, Mark end)
			: base(start, end)
		{
		}
	}
}