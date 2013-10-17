using System;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Represents an object along with its type.
	/// </summary>
	public interface IObjectDescriptor
	{
		/// <summary>
		/// A reference to the object.
		/// </summary>
		object Value { get; }

		/// <summary>
		/// The type that should be used when to interpret the <see cref="Value" />.
		/// </summary>
		Type Type { get; }

		/// <summary>
		/// The type of <see cref="Value" /> as determined by its container (e.g. a property).
		/// </summary>
		Type StaticType { get; }
	}
}