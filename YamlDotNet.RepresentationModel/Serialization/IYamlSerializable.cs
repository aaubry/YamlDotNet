using System;
using YamlDotNet.Core;

namespace YamlDotNet.RepresentationModel
{
	/// <summary>
	/// Allows an object to customize how it is serialized and deserialized.
	/// </summary>
	public interface IYamlSerializable
	{
		/// <summary>
		/// Reads this object's state from a YAML parser.
		/// </summary>
		void ReadYaml(Parser parser);

		/// <summary>
		/// Writes this object's state to a YAML emitter.
		/// </summary>
		void WriteYaml(Emitter emitter);
	}
}