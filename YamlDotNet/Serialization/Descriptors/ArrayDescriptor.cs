using System;

namespace YamlDotNet.Serialization.Descriptors
{
	/// <summary>
	/// A descriptor for an array.
	/// </summary>
	public class ArrayDescriptor : ObjectDescriptor
	{
		private readonly Type elementType;

		/// <summary>
		/// Initializes a new instance of the <see cref="ObjectDescriptor" /> class.
		/// </summary>
		/// <param name="attributeRegistry">The attribute registry.</param>
		/// <param name="type">The type.</param>
		/// <exception cref="System.ArgumentException">Expecting arrat type;type</exception>
		public ArrayDescriptor(IAttributeRegistry attributeRegistry, Type type)
			: base(attributeRegistry, type)
		{
			if (!type.IsArray) throw new ArgumentException("Expecting arrat type", "type");

			elementType = type.GetElementType();

			// TODO handle dimensions
		}

		/// <summary>
		/// Gets the type of the array element.
		/// </summary>
		/// <value>The type of the element.</value>
		public Type ElementType { get { return elementType; } }
	}
}