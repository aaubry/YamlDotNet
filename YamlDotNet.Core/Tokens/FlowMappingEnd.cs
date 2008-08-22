
using System;

namespace YamlDotNet.Core.Tokens
{
	public class FlowMappingEnd : Token {
		public FlowMappingEnd()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		public FlowMappingEnd(Mark start, Mark end)
			: base(start, end)
		{
		}
	}
}