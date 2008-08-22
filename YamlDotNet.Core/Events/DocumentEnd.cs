using System;

namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Represents a document end event.
	/// </summary>
	public class DocumentEnd : ParsingEvent
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DocumentEnd"/> class.
		/// </summary>
		/// <param name="start">The start position of the event.</param>
		/// <param name="end">The end position of the event.</param>
		public DocumentEnd(Mark start, Mark end)
			: base(start, end)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DocumentEnd"/> class.
		/// </summary>
		public DocumentEnd()
		{
		}
	}
}
