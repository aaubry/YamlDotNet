using System;

namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Represents a scalar event.
	/// </summary>
	public class Scalar : ParsingEvent, INodeEvent
	{
		private readonly string anchor;

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

		private readonly string tag;

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

		private readonly string value;

		/// <summary>
		/// Gets the value.
		/// </summary>
		/// <value>The value.</value>
		public string Value
		{
			get
			{
				return value;
			}
		}

		private readonly ScalarStyle style;

		/// <summary>
		/// Gets the style of the scalar.
		/// </summary>
		/// <value>The style.</value>
		public ScalarStyle Style
		{
			get
			{
				return style;
			}
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="Scalar"/> class.
		/// </summary>
		/// <param name="anchor">The anchor.</param>
		/// <param name="tag">The tag.</param>
		/// <param name="value">The value.</param>
		/// <param name="style">The style.</param>
		/// <param name="start">The start position of the event.</param>
		/// <param name="end">The end position of the event.</param>
		public Scalar(string anchor, string tag, string value, ScalarStyle style, Mark start, Mark end)
			: base(start, end)
		{
			this.anchor = anchor;
			this.tag = tag;
			this.style = style;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Scalar"/> class.
		/// </summary>
		/// <param name="anchor">The anchor.</param>
		/// <param name="tag">The tag.</param>
		/// <param name="style">The style.</param>
		public Scalar(string anchor, string tag, string value, ScalarStyle style)
			: this(anchor, tag, value, style, Mark.Empty, Mark.Empty)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Scalar"/> class.
		/// </summary>
		public Scalar()
		{
		}
	}
}
