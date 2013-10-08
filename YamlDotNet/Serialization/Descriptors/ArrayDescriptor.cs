using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace YamlDotNet.Serialization.Descriptors
{
	/// <summary>
	/// A descriptor for an array.
	/// </summary>
	public class ArrayDescriptor : ObjectDescriptor
	{
		private readonly Type elementType;
		private readonly Type listType;
		private readonly MethodInfo toArrayMethod;

		/// <summary>
		/// Initializes a new instance of the <see cref="ObjectDescriptor" /> class.
		/// </summary>
		/// <param name="attributeRegistry">The attribute registry.</param>
		/// <param name="type">The type.</param>
		/// <exception cref="System.ArgumentException">Expecting arrat type;type</exception>
		public ArrayDescriptor(IAttributeRegistry attributeRegistry, Type type)
			: base(attributeRegistry, type, false)
		{
			if (!type.IsArray) throw new ArgumentException("Expecting array type", "type");

			if (type.GetArrayRank() != 1)
			{
				throw new ArgumentException("Cannot support dimension [{0}] for type [{1}]. Only supporting dimension of 1".DoFormat(type.GetArrayRank(), type.FullName));
			}

			elementType = type.GetElementType();
			listType = typeof(List<>).MakeGenericType(ElementType);
			toArrayMethod = listType.GetMethod("ToArray");
		}

		/// <summary>
		/// Gets the type of the array element.
		/// </summary>
		/// <value>The type of the element.</value>
		public Type ElementType { get { return elementType; } }

		/// <summary>
		/// Creates the equivalent of list type for this array.
		/// </summary>
		/// <returns>A list type with same element type than this array.</returns>
		public Array CreateArray(int dimension)
		{
			return Array.CreateInstance(ElementType, dimension);
		}
	}
}