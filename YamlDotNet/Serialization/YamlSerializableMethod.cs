namespace YamlDotNet.Serialization
{
	/// <summary>
	/// <para>Specify the way to store a property or field of some class or structure.</para>
	/// <para>See <see cref="Serializer"/> for detail.</para>
	/// </summary>
	public enum SerializeMemberMode
	{
		/// <summary>
		/// Use the default mode depending on the type of the field/property.
		/// </summary>
		Default,

		/// <summary>
		/// When restored, new object is created by using the parameters in
		/// the YAML data and assigned to the property / field. When the
		/// property / filed is writeable, this is the default.
		/// </summary>
		Assign,

		/// <summary>
		///  Only valid for a property / field that has a class or struct type.
		///  When restored, instead of recreating the whole class or struct,
		///  the members are independently restored. When the property / field
		///  is not writeable this is the default.
		/// </summary>
		Content,

		/// <summary>
		///  Only valid for a property / field that has an  array type of a 
		///  some value type. The content of the array is stored in a binary
		///  format encoded in base64 style.
		/// </summary>
		Binary,

		/// <summary>
		/// The property / field will not be stored.
		/// </summary>
		Never,

	}
}