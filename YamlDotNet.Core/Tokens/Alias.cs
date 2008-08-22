
using System;

namespace YamlDotNet.Core.Tokens
{
	public class Alias : Token {
		private string value;
		
		public string Value {
			get {
				return value;
			}
		}
		
		public Alias(string value)
			: this(value, Mark.Empty, Mark.Empty)
		{
		}

		public Alias(string value, Mark start, Mark end)
			: base(start, end)
		{
			this.value = value;
		}
	}
}