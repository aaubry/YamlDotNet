using System;

namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Represents a sequence end event.
	/// </summary>
	public class SequenceEnd : ParsingEvent
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SequenceEnd"/> class.
		/// </summary>
		/// <param name="start">The start position of the event.</param>
		/// <param name="end">The end position of the event.</param>
		public SequenceEnd(Mark start, Mark end)
			: base(start, end)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SequenceEnd"/> class.
		/// </summary>
		public SequenceEnd()
			: this(Mark.Empty, Mark.Empty)
		{
		}
	}
}
