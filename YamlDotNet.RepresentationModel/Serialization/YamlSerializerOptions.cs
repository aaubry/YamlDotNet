using System;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Specifies the behavior of the <see cref="YamlSerializer"/>.
	/// </summary>
	public enum YamlSerializerOptions
	{
		/// <summary>
		/// Serializes using the default options
		/// </summary>
		Default,

		/// <summary>
		/// Ensures that it will be possible to deserialize the serialized objects.
		/// </summary>
		Roundtrip
	}
}