namespace YamlDotNet.Serialization
{
	/// <summary>
	/// An interface to plug custom <see cref="YamlStyle"/> to specifics objects instances while
	/// serializing. Use <see cref="SerializerSettings.DynamicStyleFormat"/>.
	/// </summary>
	public interface IDynamicStyleFormat
	{
		/// <summary>
		/// Gets the style for a specific instance/type. Return <see cref="YamlStyle.Any" /> if unspecified.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="value">The value.</param>
		/// <param name="descriptor">The descriptor.</param>
		/// <returns>The style to apply ot <see cref="YamlStyle.Any" /> if unspecified.</returns>
		YamlStyle GetStyle(SerializerContext context, object value, ITypeDescriptor descriptor);
	}
}