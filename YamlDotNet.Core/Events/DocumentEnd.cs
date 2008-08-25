using System;

namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Represents a document end event.
	/// </summary>
	public class DocumentEnd : ParsingEvent
	{
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
		public DocumentEnd(bool isImplicit)
			: this(isImplicit, Mark.Empty, Mark.Empty)
		{
		}
	}
}
