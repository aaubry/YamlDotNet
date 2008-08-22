using System;

namespace YamlDotNet.Core.Tokens
{
	public abstract class Token
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
		
		public Token(Mark start, Mark end) {
			this.start = start;
			this.end = end;
		}
	}
}