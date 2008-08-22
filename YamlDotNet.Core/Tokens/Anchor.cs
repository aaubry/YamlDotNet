
using System;

namespace YamlDotNet.Core.Tokens
{
	/// <summary>
	/// Represents an anchor token.
	/// </summary>
	public class Anchor : Token
	{
		private string value;

		/// <summary>
		/// Gets the value.
		/// </summary>
		/// <value>The value.</value>
		public string Value
		{
			get
			{
				return value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Anchor"/> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public Anchor(string value)
			: this(value, Mark.Empty, Mark.Empty)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Anchor"/> class.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="start">The start position of the token.</param>
		/// <param name="end">The end position of the token.</param>
		public Anchor(string value, Mark start, Mark end)
			: base(start, end)
		{
			this.value = value;
		}
	}
}