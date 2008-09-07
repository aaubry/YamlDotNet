using System;

namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Represents an alias event.
	/// </summary>
	public class AnchorAlias : ParsingEvent
	{
		/// <summary>
		/// Gets the event type, which allows for simpler type comparisons.
		/// </summary>
		internal override EventType Type {
			get {
				return EventType.YAML_ALIAS_EVENT;
			}
		}
		
		private readonly string value;

		/// <summary>
		/// Gets the value of the alias.
		/// </summary>
		public string Value
		{
			get
			{
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
			if(string.IsNullOrEmpty(value)) {
				throw new YamlException("Anchor value must not be empty.");
			}

			if(!NodeEvent.anchorValidator.IsMatch(value)) {
				throw new YamlException("Anchor value must contain alphanumerical characters only.");
			}
			
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
