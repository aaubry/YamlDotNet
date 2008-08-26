using System;

namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Represents a mapping start event.
	/// </summary>
	public class MappingStart : ParsingEvent, INodeEvent
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

		private readonly MappingStyle style;

		/// <summary>
		/// Gets the style of the mapping.
		/// </summary>
		public MappingStyle Style
		{
			get
			{
				return style;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MappingStart"/> class.
		/// </summary>
		/// <param name="anchor">The anchor.</param>
		/// <param name="tag">The tag.</param>
		/// <param name="isImplicit">Indicates whether the event is implicit.</param>
		/// <param name="style">The style of the mapping.</param>
		/// <param name="start">The start position of the event.</param>
		/// <param name="end">The end position of the event.</param>
		public MappingStart(string anchor, string tag, bool isImplicit, MappingStyle style, Mark start, Mark end)
			: base(start, end)
		{
			this.anchor = anchor;
			this.tag = tag;
			this.isImplicit = isImplicit;
			this.style = style;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MappingStart"/> class.
		/// </summary>
		/// <param name="anchor">The anchor.</param>
		/// <param name="tag">The tag.</param>
		/// <param name="isImplicit">Indicates whether the event is implicit.</param>
		/// <param name="style">The style of the mapping.</param>
		public MappingStart(string anchor, string tag, bool isImplicit, MappingStyle style)
			: this(anchor, tag, isImplicit, style, Mark.Empty, Mark.Empty)
		{
		}
	}
}
