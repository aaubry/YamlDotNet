using System;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Specifies the behavior of the <see cref="YamlSerializer"/>.
	/// </summary>
	[Flags]
	public enum YamlSerializerModes
	{
		/// <summary>
		/// Serializes using the default options
		/// </summary>
		None = 0,

		/// <summary>
		/// Ensures that it will be possible to deserialize the serialized objects.
		/// </summary>
		Roundtrip = 1,

		/// <summary>
		/// If this flag is specified, if the same object appears more than once in the
		/// serialization graph, it will be serialized each time instead of just once.
		/// </summary>
		/// <remarks>
		/// If the serialization graph contains circular references and this flag is set,
		/// a <see cref="StackOverflowException" /> will be thrown.
		/// If this flag is not set, there is a performance penalty because the entire
		/// object graph must be walked twice.
		/// </remarks>
		DisableAliases = 2,
	}
}