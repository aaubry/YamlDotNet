
using System;

namespace YamlDotNet.CoreCs.Tokens
{
	public class FlowMappingStart : Token {
		public FlowMappingStart()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		public FlowMappingStart(Mark start, Mark end)
			: base(start, end)
		{
		}
	}
}