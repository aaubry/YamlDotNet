using System;

namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Represents a sequence end event.
	/// </summary>
	public class SequenceEnd : ParsingEvent, ISequenceEnd
	{
		/// <summary>
		/// Gets the event type, which allows for simpler type comparisons.
		/// </summary>
		internal override EventType Type {
			get {
				return EventType.YAML_SEQUENCE_END_EVENT;
			}
		}
		
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

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			return "Sequence end";
		}
	}
}
