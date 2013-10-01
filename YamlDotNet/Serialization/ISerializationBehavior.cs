
using System;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// Adds behavior to <see cref="YamlSerializer"/>.
	/// </summary>
	public interface ISerializationBehavior
	{
		void SerializationStarting(Type type, object o);
	}
}