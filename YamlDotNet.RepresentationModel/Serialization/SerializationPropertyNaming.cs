namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// property naming strategy to use when serializing objects
	/// </summary>
	public enum SerializationPropertyNaming
	{
		/// <summary>
		/// Use standard property naming (keep YAML properties the same as the class)
		/// </summary>
		Standard,

		/// <summary>
		/// Use camel case for property naming (eg. "helloWorld")
		/// </summary>
		CamelCase,

		/// <summary>
		/// Use pascal case for property naming (eg. "HelloWorld")
		/// </summary>
		PascalCase,

		/// <summary>
		/// Use hyphenated property names (eg. "hello-world")
		/// </summary>
		Hyphenated,

		/// <summary>
		/// Use underscored property names (eg. "hello_world")
		/// </summary>
		Underscored
	}
}