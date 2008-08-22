using System;
using System.Text;

namespace YamlDotNet.Core.Events
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
