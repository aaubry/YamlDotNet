using System;

namespace YamlDotNet.CoreCs.Tokens
{
	public class Tag : Token {
		private string handle;
		private string suffix;
		
		public string Handle {
			get {
				return handle;
			}
		}

		public string Suffix {
			get {
				return suffix;
			}
		}
		
		public Tag(string handle, string suffix)
			: this(handle, suffix, Mark.Empty, Mark.Empty)
		{
		}

		public Tag(string handle, string suffix, Mark start, Mark end)
			: base(start, end)
		{
			this.handle = handle;
			this.suffix = suffix;
		}
	}
}