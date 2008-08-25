using System;

namespace YamlDotNet.Core.Events
{
	public abstract class ParsingEvent
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
		
		public ParsingEvent() {
		}
		
		public ParsingEvent(Mark start, Mark end) {
			this.start = start;
			this.end = end;
		}
	}
}
