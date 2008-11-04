using System;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Specifies the behavior of the <see cref="YamlSerializer"/>.
	/// </summary>
	[Flags]
	public enum YamlSerializerOptions
	{
		/// <summary>
		/// Serializes using the default options
		/// </summary>
		None,

		/// <summary>
		/// Ensures that it will be possible to deserialize the serialized objects.
		/// </summary>
		Roundtrip
	}
}