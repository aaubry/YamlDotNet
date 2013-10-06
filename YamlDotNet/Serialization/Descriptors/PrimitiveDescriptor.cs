using System;
using System.Collections.Generic;

namespace YamlDotNet.Serialization.Descriptors
{
	/// <summary>
	/// Describes a primitive.
	/// </summary>
	public class PrimitiveDescriptor : ObjectDescriptor
	{
		private static readonly List<IMemberDescriptor> EmptyMembers = new List<IMemberDescriptor>();

		/// <summary>
		/// Initializes a new instance of the <see cref="ObjectDescriptor" /> class.
		/// </summary>
		/// <param name="attributeRegistry">The attribute registry.</param>
		/// <param name="type">The type.</param>
		/// <exception cref="System.ArgumentException">Type [{0}] is not a primitive</exception>
		public PrimitiveDescriptor(IAttributeRegistry attributeRegistry, Type type)
			: base(attributeRegistry, type)
		{
			if (!IsPrimitive(type))
				throw new ArgumentException("Type [{0}] is not a primitive");
		}

		/// <summary>
		/// Determines whether the specified type is a primitive.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns><c>true</c> if the specified type is primitive; otherwise, <c>false</c>.</returns>
		public static bool IsPrimitive(Type type)
		{
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Object:
				case TypeCode.Empty:
					return type == typeof (string);
			}
			return true;
		}

		protected override System.Collections.Generic.List<IMemberDescriptor> PrepareMembers()
		{
			return EmptyMembers;
		}
	}
}