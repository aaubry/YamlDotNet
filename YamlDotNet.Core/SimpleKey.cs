using System;

namespace YamlDotNet.Core
{
	/// <summary>
	/// Represents a simple key.
	/// </summary>
	internal class SimpleKey
	{
		private bool isPossible;
		private readonly bool isRequired;
		private readonly int tokenNumber;
		private readonly Mark mark;

		/// <summary>
		/// Gets or sets a value indicating whether this instance is possible.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is possible; otherwise, <c>false</c>.
		/// </value>
		public bool IsPossible
		{
			get
			{
				return isPossible;
			}
			set
			{
				isPossible = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is required.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is required; otherwise, <c>false</c>.
		/// </value>
		public bool IsRequired
		{
			get
			{
				return isRequired;
			}
		}

		/// <summary>
		/// Gets or sets the token number.
		/// </summary>
		/// <value>The token number.</value>
		public int TokenNumber
		{
			get
			{
				return tokenNumber;
			}
		}

		/// <summary>
		/// Gets or sets the mark that indicates the location of the simple key.
		/// </summary>
		/// <value>The mark.</value>
		public Mark Mark
		{
			get
			{
				return mark;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleKey"/> class.
		/// </summary>
		public SimpleKey()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleKey"/> class.
		/// </summary>
		public SimpleKey(bool isPossible, bool isRequired, int tokenNumber, Mark mark)
		{
			this.isPossible = isPossible;
			this.isRequired = isRequired;
			this.tokenNumber = tokenNumber;
			this.mark = mark;
		}
	}
}