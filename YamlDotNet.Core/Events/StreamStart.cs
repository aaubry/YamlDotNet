using System;

namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Represents a stream start event.
	/// </summary>
	public class StreamStart : ParsingEvent, IStreamStart
	{
		/// <summary>
		/// Gets the event type, which allows for simpler type comparisons.
		/// </summary>
		internal override EventType Type {
			get {
				return EventType.YAML_STREAM_START_EVENT;
			}
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="StreamStart"/> class.
		/// </summary>
		public StreamStart()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StreamStart"/> class.
		/// </summary>
		/// <param name="start">The start position of the event.</param>
		/// <param name="end">The end position of the event.</param>
		public StreamStart(Mark start, Mark end)
			: base(start, end)
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
			return "Stream start";
		}
	}
}