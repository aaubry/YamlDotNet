using System;

namespace YamlDotNet.CoreCs.Events
{
    public class DocumentEnd : Event
	{
		public DocumentEnd(Mark start, Mark end)
			: base(start, end)
		{
		}
			
		public DocumentEnd() {
		}
	}
}
