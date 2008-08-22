using System;

namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Represents a scalar event.
	/// </summary>
	public class Scalar : ParsingEvent, INodeEvent
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

		/// <summary>
		/// Initializes a new instance of the <see cref="Scalar"/> class.
		/// </summary>
		/// <param name="anchor">The anchor.</param>
		/// <param name="tag">The tag.</param>
		/// <param name="start">The start position of the event.</param>
		/// <param name="end">The end position of the event.</param>
		public Scalar(string anchor, string tag, Mark start, Mark end)
			: base(start, end)
		{
			this.anchor = anchor;
			this.tag = tag;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Scalar"/> class.
		/// </summary>
		/// <param name="anchor">The anchor.</param>
		/// <param name="tag">The tag.</param>
		public Scalar(string anchor, string tag)
			: this(anchor, tag, Mark.Empty, Mark.Empty)
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
