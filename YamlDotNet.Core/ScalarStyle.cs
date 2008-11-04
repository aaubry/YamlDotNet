using System;

namespace YamlDotNet.Core
{
	/// <summary>
	/// Specifies the style of a YAML scalar.
	/// </summary>
	public enum ScalarStyle
	{
		/// <summary>
		/// Let the emitter choose the style.
		/// </summary>
		Any,

		/// <summary>
		/// The plain scalar style.
		/// </summary>
		Plain,

		/// <summary>
		/// The single-quoted scalar style.
		/// </summary>
		SingleQuoted,

		/// <summary>
		/// The double-quoted scalar style.
		/// </summary>
		DoubleQuoted,

		/// <summary>
		/// The literal scalar style.
		/// </summary>
		Literal,

		/// <summary>
		/// The folded scalar style.
		/// </summary>
		Folded,
	}
}