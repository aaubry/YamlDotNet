
using System;

namespace YamlDotNet.CoreCs.Tokens
{
	public class FlowSequenceEnd : Token {
		public FlowSequenceEnd()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		public FlowSequenceEnd(Mark start, Mark end)
			: base(start, end)
		{
		}
	}
}