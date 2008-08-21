using System;

namespace YamlDotNet.CoreCs.Events
{
    public class DocumentStart : Event
	{
		public DocumentStart(Mark start, Mark end)
			: base(start, end)
		{
		}
		
		public DocumentStart() {
		}
	}
}
