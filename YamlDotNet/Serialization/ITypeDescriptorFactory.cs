using System;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// A factory to create an instance of a <see cref="ITypeDescriptor"/>
	/// </summary>
	internal interface ITypeDescriptorFactory
	{
		/// <summary>
		/// Tries to create an instance of a <see cref="ITypeDescriptor"/> from the type. Return null if this factory is not handling this type.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>ITypeDescriptor.</returns>
		ITypeDescriptor Find(Type type);
	}
}