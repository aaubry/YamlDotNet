
using System;

namespace YamlDotNet.Core.Tokens
{
	/// <summary>
	/// Represents a flow mapping start token.
	/// </summary>
	public class FlowMappingStart : Token
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FlowMappingStart"/> class.
		/// </summary>
		public FlowMappingStart()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FlowMappingStart"/> class.
		/// </summary>
		/// <param name="start">The start position of the token.</param>
		/// <param name="end">The end position of the token.</param>
		public FlowMappingStart(Mark start, Mark end)
			: base(start, end)
		{
		}
	}
}