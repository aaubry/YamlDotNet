using System;

namespace YamlDotNet.Core.Events
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
