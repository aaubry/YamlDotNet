using System;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Resolves the type of values.
	/// </summary>
	public interface ITypeResolver
	{
		Type Resolve(Type staticType, object actualValue);
	}
}