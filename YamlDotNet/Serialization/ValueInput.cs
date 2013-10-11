namespace YamlDotNet.Serialization
{
	/// <summary>
	/// A value to serialize.
	/// </summary>
	public struct ValueInput
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ValueInput"/> struct that contains a value.
		/// </summary>
		/// <param name="value">The value.</param>
		public ValueInput(object value)
			: this()
		{
			Value = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ValueInput"/> struct with its value and associated tag.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="tag">The tag.</param>
		public ValueInput(object value, string tag) : this()
		{
			Value = value;
			Tag = tag;
		}

		/// <summary>
		/// The returned value or null if no value.
		/// </summary>
		public object Value { get; set; }

		/// <summary>
		/// Gets the tag attached to this value.
		/// </summary>
		/// <value>The tag.</value>
		public string Tag { get; set; }

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>A <see cref="System.String" /> that represents this instance.</returns>
		public override string ToString()
		{
			return string.Format("{0}{1}", Tag != null ? string.Format("{0} ", Tag) : string.Empty, Value);
		}
	}
}