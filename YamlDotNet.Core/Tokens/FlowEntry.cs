
using System;

namespace YamlDotNet.CoreCs.Tokens
{
	public class FlowEntry : Token {
		public FlowEntry()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		public FlowEntry(Mark start, Mark end)
			: base(start, end)
		{
		}
	}
}