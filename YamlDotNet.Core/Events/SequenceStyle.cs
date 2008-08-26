using System;

namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Specifies the style of a sequence.
	/// </summary>
	public enum SequenceStyle
	{
		/// <summary>
		/// Let the emitter choose the style.
		/// </summary>
		Any,

		/// <summary>
		/// The block sequence style.
		/// </summary>
		Block,

		/// <summary>
		/// The flow sequence style.
		/// </summary>
		Flow
	}
}