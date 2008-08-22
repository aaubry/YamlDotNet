using System;

namespace YamlDotNet.Core.Events
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
