using System;
using System.Text;

namespace YamlDotNet.CoreCs.Events
{
    public class StreamStart : Event
	{
		public StreamStart() {
		}

		public StreamStart(Mark start, Mark end)
			: base(start, end)
		{
		}
	}
}
