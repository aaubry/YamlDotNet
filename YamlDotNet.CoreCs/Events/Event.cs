using System;

namespace YamlDotNet.CoreCs.Events
{
	public abstract class Event
	{
		private Mark start;
		
		public Mark Start {
			get {
				return start;
			}
		}
		
		private Mark end;
		
		public Mark End {
			get {
				return end;
			}
		}
		
		public Event() {
		}
		
		public Event(Mark start, Mark end) {
			this.start = start;
			this.end = end;
		}
	}
}
