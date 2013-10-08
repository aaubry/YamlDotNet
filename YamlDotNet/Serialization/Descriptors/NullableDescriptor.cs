using System;
using System.Collections.Generic;

namespace YamlDotNet.Serialization.Descriptors
{
	/// <summary>
	/// Describes a descriptor for a nullable type <see cref="Nullable{T}"/>.
	/// </summary>
	internal class NullableDescriptor : ObjectDescriptor
	{
		private static readonly List<IMemberDescriptor> EmptyMembers = new List<IMemberDescriptor>();

		/// <summary>
		/// Initializes a new instance of the <see cref="ObjectDescriptor" /> class.
		/// </summary>
		/// <param name="attributeRegistry">The attribute registry.</param>
		/// <param name="type">The type.</param>
		/// <exception cref="System.ArgumentException">Type [{0}] is not a primitive</exception>
		public NullableDescriptor(IAttributeRegistry attributeRegistry, Type type) : base(attributeRegistry, type, false)
		{
			if (!IsNullable(type))
				throw new ArgumentException("Type [{0}] is not a primitive");

			UnderlyingType = Nullable.GetUnderlyingType(type);
		}

		/// <summary>
		/// Gets the type underlying type T of the nullable <see cref="Nullable{T}"/>
		/// </summary>
		/// <value>The type of the element.</value>
		public Type UnderlyingType { get; private set;}

		/// <summary>
		/// Determines whether the specified type is nullable.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns><c>true</c> if the specified type is nullable; otherwise, <c>false</c>.</returns>
		public static bool IsNullable(Type type)
		{
			return type.IsNullable();
		}

		protected override System.Collections.Generic.List<IMemberDescriptor> PrepareMembers()
		{
			return EmptyMembers;
		}
	}
}