
using System;

namespace YamlDotNet.Core.Tokens
{
	public class Anchor : Token {
		private string value;
		
		public string Value {
			get {
				return value;
			}
		}
		
		public Anchor(string value)
			: this(value, Mark.Empty, Mark.Empty)
		{
		}

		public Anchor(string value, Mark start, Mark end)
			: base(start, end)
		{
			this.value = value;
		}
	}
}