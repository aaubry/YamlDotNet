using System;

namespace YamlDotNet.Core.Events
{
    public class AnchorAlias : ParsingEvent
	{				
		private readonly string value;
		
		public string Value {
			get {
				return value;
			}
		}
		
		public AnchorAlias(string value, Mark start, Mark end)
			: base(start, end)
		{
			this.value = value;
		}
		
		public AnchorAlias(string value)
			: this(value, Mark.Empty, Mark.Empty)
		{
		}
	}
}
