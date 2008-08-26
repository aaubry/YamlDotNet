using System;

namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Represents an alias event.
	/// </summary>
    public class AnchorAlias : ParsingEvent
	{				
		private readonly string value;
		
		/// <summary>
		/// Gets the value of the alias.
		/// </summary>
		public string Value {
			get {
				return value;
			}
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="AnchorAlias"/> class.
		/// </summary>
		/// <param name="value">The value of the alias.</param>
		/// <param name="start">The start position of the event.</param>
		/// <param name="end">The end position of the event.</param>
		public AnchorAlias(string value, Mark start, Mark end)
			: base(start, end)
		{
			this.value = value;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="AnchorAlias"/> class.
		/// </summary>
		/// <param name="value">The value of the alias.</param>
		public AnchorAlias(string value)
			: this(value, Mark.Empty, Mark.Empty)
		{
		}
	}
}
