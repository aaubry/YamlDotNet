
using System;

namespace YamlDotNet.Core.Tokens
{
	/// <summary>
	/// Represents a document end token.
	/// </summary>
	public class DocumentEnd : Token
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DocumentEnd"/> class.
		/// </summary>
		public DocumentEnd()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DocumentEnd"/> class.
		/// </summary>
		/// <param name="start">The start position of the token.</param>
		/// <param name="end">The end position of the token.</param>
		public DocumentEnd(Mark start, Mark end)
			: base(start, end)
		{
		}
	}
}