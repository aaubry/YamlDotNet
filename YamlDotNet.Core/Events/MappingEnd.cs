using System;

namespace YamlDotNet.Core.Events
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
