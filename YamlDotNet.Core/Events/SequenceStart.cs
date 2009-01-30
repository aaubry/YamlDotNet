using System;
using System.Globalization;

namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Represents a sequence start event.
	/// </summary>
	public class SequenceStart : NodeEvent, ISequenceStart
	{
		/// <summary>
		/// Gets a value indicating the variation of depth caused by this event.
		/// The value can be either -1, 0 or 1. For start events, it will be 1,
		/// for end events, it will be -1, and for the remaining events, it will be 0.
		/// </summary>
		public override int NestingIncrease {
			get {
				return 1;
			}
		}

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

		/// <summary>
		/// Gets a value indicating whether this instance is canonical.
		/// </summary>
		/// <value></value>
		public override bool IsCanonical {
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

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"Sequence start [anchor = {0}, tag = {1}, isImplicit = {2}, style = {3}]",
				Anchor,
				Tag,
				isImplicit,
				style
			);
		}
	}
}