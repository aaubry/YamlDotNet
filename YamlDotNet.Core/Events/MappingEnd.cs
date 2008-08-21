using System;

namespace YamlDotNet.CoreCs.Events
{
    public class MappingEnd : Event
	{
		public MappingEnd(Mark start, Mark end)
			: base(start, end)
		{
		}
			
		public MappingEnd() {
		}
	}
}
