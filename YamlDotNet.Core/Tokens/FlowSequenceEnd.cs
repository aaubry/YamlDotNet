
using System;

namespace YamlDotNet.Core.Tokens
{
	/// <summary>
	/// Represents a flow sequence end token.
	/// </summary>
	public class FlowSequenceEnd : Token
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FlowSequenceEnd"/> class.
		/// </summary>
		public FlowSequenceEnd()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FlowSequenceEnd"/> class.
		/// </summary>
		/// <param name="start">The start position of the token.</param>
		/// <param name="end">The end position of the token.</param>
		public FlowSequenceEnd(Mark start, Mark end)
			: base(start, end)
		{
		}
	}
}