using System;

namespace YamlDotNet.CoreCs.Events
{
    public class SequenceEnd : Event
	{
		public SequenceEnd(Mark start, Mark end)
			: base(start, end)
		{
		}
			
		public SequenceEnd() {
		}
	}
}
