
using System;

namespace YamlDotNet.Core.Tokens
{
	/// <summary>
	/// Represents a flow mapping end token.
	/// </summary>
	public class FlowMappingEnd : Token
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FlowMappingEnd"/> class.
		/// </summary>
		public FlowMappingEnd()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FlowMappingEnd"/> class.
		/// </summary>
		/// <param name="start">The start position of the token.</param>
		/// <param name="end">The end position of the token.</param>
		public FlowMappingEnd(Mark start, Mark end)
			: base(start, end)
		{
		}
	}
}