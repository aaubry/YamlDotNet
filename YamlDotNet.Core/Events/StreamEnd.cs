using System;

namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Represents a stream end event.
	/// </summary>
	public class StreamEnd : ParsingEvent
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="StreamEnd"/> class.
		/// </summary>
		/// <param name="start">The start position of the event.</param>
		/// <param name="end">The end position of the event.</param>
		public StreamEnd(Mark start, Mark end)
			: base(start, end)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StreamEnd"/> class.
		/// </summary>
		public StreamEnd()
		{
		}
	}
}
