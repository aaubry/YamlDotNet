using System;
using System.Collections.Generic;
using System.Reflection;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Provides access to the properties of a type.
	/// </summary>
	public interface ITypeDescriptor
	{
		IEnumerable<IPropertyDescriptor> GetProperties(Type type);
	}

	public interface IPropertyDescriptor
	{
		string Name { get; }
		PropertyInfo Property { get; }
	}
}
