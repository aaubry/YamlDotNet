
using System;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Adds behavior to <see cref="YamlSerializer"/>.
	/// </summary>
	public interface ISerializationBehavior
	{
		void SerializationStarting(Type type, object o);
	}
}