using System;

namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Represents a sequence start event.
	/// </summary>
	public class SequenceStart : NodeEvent
	{
		/// <summary>
		/// Gets the event type, which allows for simpler type comparisons.
		/// </summary>
		internal override EventType Type {
			get {
				return EventType.YAML_SEQUENCE_START_EVENT;
			}
		}

		private readonly bool isImplicit;

		/// <summary>
		/// Gets a value indicating whether this instance is implicit.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is implicit; otherwise, <c>false</c>.
		/// </value>
		public bool IsImplicit
		{
			get
			{
				return isImplicit;
			}
		}
		
		internal override bool IsCanonical {
			get {
				return !isImplicit;
			}
		}

		private readonly SequenceStyle style;

		/// <summary>
		/// Gets the style.
		/// </summary>
		/// <value>The style.</value>
		public SequenceStyle Style
		{
			get
			{
				return style;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SequenceStart"/> class.
		/// </summary>
		/// <param name="anchor">The anchor.</param>
		/// <param name="tag">The tag.</param>
		/// <param name="isImplicit">if set to <c>true</c> [is implicit].</param>
		/// <param name="style">The style.</param>
		/// <param name="start">The start position of the event.</param>
		/// <param name="end">The end position of the event.</param>
		public SequenceStart(string anchor, string tag, bool isImplicit, SequenceStyle style, Mark start, Mark end)
			: base(anchor, tag, start, end)
		{
			this.isImplicit = isImplicit;
			this.style = style;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SequenceStart"/> class.
		/// </summary>
		public SequenceStart(string anchor, string tag, bool isImplicit, SequenceStyle style)
			: this(anchor, tag, isImplicit, style, Mark.Empty, Mark.Empty)
		{
		}
	}
}