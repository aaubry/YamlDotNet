
using System;

namespace YamlDotNet.Core.Tokens
{
	public class Scalar : Token {
		private string value;
		
		public string Value {
			get {
				return value;
			}
		}

		public ScalarStyle Style {
			get {
				return style;
			}
		}
		
		private ScalarStyle style;
		
		public Scalar(string value)
			: this(value, ScalarStyle.Any)
		{
		}
		
		public Scalar(string value, ScalarStyle style)
			: this(value, style, Mark.Empty, Mark.Empty)
		{
		}

		public Scalar(string value, ScalarStyle style, Mark start, Mark end)
			: base(start, end)
		{
			this.value = value;
			this.style = style;
		}
	}
}