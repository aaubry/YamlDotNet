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

	public interface IPropertyDescriptor : IObjectDescriptor
	{
		string Name { get; }
		bool CanWrite { get; }

		void SetValue(object target, object value);

		T GetCustomAttribute<T>() where T : Attribute;
	}

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

	public sealed class ObjectDescriptor : IObjectDescriptor
	{
		public object Value { get; private set; }
		public Type Type { get; private set; }
		public Type StaticType { get; private set; }

		public ObjectDescriptor(object value, Type type, Type staticType)
		{
			Value = value;

			if (type == null)
			{
				throw new ArgumentNullException("type");
			}

			Type = type;

			if (staticType == null)
			{
				throw new ArgumentNullException("staticType");
			}
			
			StaticType = staticType;
		}
	}
}
