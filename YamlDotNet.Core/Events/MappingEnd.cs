using System;

namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Represents a mapping end event.
	/// </summary>
	public class MappingEnd : ParsingEvent
	{
		/// <summary>
		/// Gets the event type, which allows for simpler type comparisons.
		/// </summary>
		internal override EventType Type {
			get {
				return EventType.YAML_MAPPING_END_EVENT;
			}
		}
		
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

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			return "Mapping end";
		}
	}
}