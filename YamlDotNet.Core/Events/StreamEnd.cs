using System;

namespace YamlDotNet.CoreCs.Events
{
    public class StreamEnd : Event
	{
		public StreamEnd(Mark start, Mark end)
			: base(start, end)
		{
		}
		
		public StreamEnd() {
		}
	}
}
