using System;

namespace YamlDotNet.RepresentationModel.Serialization.TypeResolvers
{
	/// <summary>
	/// The type returned will always be the static type.
	/// </summary>
	public sealed class StaticTypeResolver : ITypeResolver
	{
		public Type Resolve(Type staticType, object actualValue)
		{
			return staticType;
		}
	}
}