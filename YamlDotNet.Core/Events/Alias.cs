using System;

namespace YamlDotNet.Core.Events
{
    public class Alias : Event
	{				
		public Alias(Mark start, Mark end)
			: base(start, end)
		{
		}
		
		public Alias() {
		}
	}
}
