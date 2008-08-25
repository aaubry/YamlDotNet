using System;

namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Represents a sequence start event.
	/// </summary>
	public class SequenceStart : ParsingEvent, INodeEvent
	{
		private string anchor;

		/// <summary>
		/// Gets the anchor.
		/// </summary>
		/// <value></value>
		public string Anchor
		{
			get
			{
				return anchor;
			}
		}

		private string tag;

		/// <summary>
		/// Gets the tag.
		/// </summary>
		/// <value></value>
		public string Tag
		{
			get
			{
				return tag;
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
			: base(start, end)
		{
			this.anchor = anchor;
			this.tag = tag;
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