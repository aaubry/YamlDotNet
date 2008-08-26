using System;

namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Represents a mapping end event.
	/// </summary>
	public class MappingEnd : ParsingEvent
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MappingEnd"/> class.
		/// </summary>
		/// <param name="start">The start position of the event.</param>
		/// <param name="end">The end position of the event.</param>
		public MappingEnd(Mark start, Mark end)
			: base(start, end)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MappingEnd"/> class.
		/// </summary>
		public MappingEnd()
			: this(Mark.Empty, Mark.Empty)
		{
		}
	}
}
