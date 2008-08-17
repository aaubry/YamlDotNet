
using System;

namespace YamlDotNet.CoreCs.Tokens
{
	public class FlowSequenceStart : Token {
		public FlowSequenceStart()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		public FlowSequenceStart(Mark start, Mark end)
			: base(start, end)
		{
		}
	}
}