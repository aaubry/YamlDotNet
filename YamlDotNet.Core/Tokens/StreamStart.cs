using System;
using System.Text;

namespace YamlDotNet.Core.Tokens
{
	public class StreamStart : Token {
		public StreamStart()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		public StreamStart(Mark start, Mark end)
			: base(start, end)
		{
		}
	}
}