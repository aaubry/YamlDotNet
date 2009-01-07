using System;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Contains additional information about a deserialization.
	/// </summary>
	public interface IDeserializationContext
	{
		/// <summary>
		/// Gets the anchor of the specified object.
		/// </summary>
		/// <param name="value">The object that has an anchor.</param>
		/// <returns>Returns the anchor of the object, or null if no anchor was defined.</returns>
		string GetAnchor(object value);
	}
}