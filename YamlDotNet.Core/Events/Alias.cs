using System;

namespace YamlDotNet.CoreCs.Events
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
