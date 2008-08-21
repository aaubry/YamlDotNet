
using System;

namespace YamlDotNet.CoreCs.Tokens
{
	public class TagDirective : Token {
		private readonly string handle;
		private readonly string prefix;
		
		public string Handle {
			get {
				return handle;
			}
		}

		public string Prefix {
			get {
				return prefix;
			}
		}
		
		public TagDirective(string handle, string prefix)
			: this(handle, prefix, Mark.Empty, Mark.Empty)
		{
		}

		public TagDirective(string handle, string prefix, Mark start, Mark end)
			: base(start, end)
		{
			this.handle = handle;
			this.prefix = prefix;
		}
	}
}