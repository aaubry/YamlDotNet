
using System;

namespace YamlDotNet.Core.Tokens
{
	public class AnchorAlias : Token {
		private string value;
		
		public string Value {
			get {
				return value;
			}
		}
		
		public AnchorAlias(string value)
			: this(value, Mark.Empty, Mark.Empty)
		{
		}

		public AnchorAlias(string value, Mark start, Mark end)
			: base(start, end)
		{
			this.value = value;
		}
	}
}