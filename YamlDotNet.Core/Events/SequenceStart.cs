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

		/// <summary>
		/// Initializes a new instance of the <see cref="SequenceStart"/> class.
		/// </summary>
		/// <param name="anchor">The anchor.</param>
		/// <param name="tag">The tag.</param>
		/// <param name="start">The start position of the event.</param>
		/// <param name="end">The end position of the event.</param>
		public SequenceStart(string anchor, string tag, Mark start, Mark end)
			: base(start, end)
		{
			this.anchor = anchor;
			this.tag = tag;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SequenceStart"/> class.
		/// </summary>
		public SequenceStart()
		{
		}
	}
}