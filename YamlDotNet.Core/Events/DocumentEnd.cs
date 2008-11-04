using System;
using System.Globalization;

namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Represents a document end event.
	/// </summary>
	public class DocumentEnd : ParsingEvent
	{
		/// <summary>
		/// Gets the event type, which allows for simpler type comparisons.
		/// </summary>
		internal override EventType Type {
			get {
				return EventType.YAML_DOCUMENT_END_EVENT;
			}
		}
		
		private readonly bool isImplicit;

		/// <summary>
		/// Gets a value indicating whether this instance is implicit.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is implicit; otherwise, <c>false</c>.
		/// </value>
		public bool IsImplicit
		{
			get
			{
				return isImplicit;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DocumentEnd"/> class.
		/// </summary>
		/// <param name="isImplicit">Indicates whether the event is implicit.</param>
		/// <param name="start">The start position of the event.</param>
		/// <param name="end">The end position of the event.</param>
		public DocumentEnd(bool isImplicit, Mark start, Mark end)
			: base(start, end)
		{
			this.isImplicit = isImplicit;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DocumentEnd"/> class.
		/// </summary>
		/// <param name="isImplicit">Indicates whether the event is implicit.</param>
		public DocumentEnd(bool isImplicit)
			: this(isImplicit, Mark.Empty, Mark.Empty)
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
			return string.Format(
				CultureInfo.InvariantCulture,
				"Document end [isImplicit = {0}]",
				isImplicit
			);
		}
	}
}