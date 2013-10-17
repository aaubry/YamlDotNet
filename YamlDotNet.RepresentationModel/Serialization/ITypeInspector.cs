using System;
using System.Collections.Generic;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Provides access to the properties of a type.
	/// </summary>
	public interface ITypeInspector
	{
		/// <summary>
		/// Gets all properties of the specified type.
		/// </summary>
		/// <param name="type">The type whose properties are to be enumerated.</param>
		/// <param name="container">The actual object of type <paramref name="type"/> whose properties are to be enumerated. Can be null.</param>
		/// <returns></returns>
		IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container);

		/// <summary>
		/// Gets the property of the type with the specified name.
		/// </summary>
		/// <param name="type">The type whose properties are to be searched.</param>
		/// <param name="container">The actual object of type <paramref name="type"/> whose properties are to be searched. Can be null.</param>
		/// <param name="name">The name of the property.</param>
		/// <returns></returns>
		IPropertyDescriptor GetProperty(Type type, object container, string name);
	}
}
