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
			if (!type.IsArray) throw new ArgumentException("Expecting array type", "type");

			// TODO handle dimensions
			if (type.GetArrayRank() != 1)
			{
				throw new ArgumentException("Cannot support dimension [{0}] for type [{1}]. Only supporting 1".DoFormat(type.GetArrayRank(), type.FullName));
			}

			elementType = type.GetElementType();
		}

		/// <summary>
		/// Gets the type of the array element.
		/// </summary>
		/// <value>The type of the element.</value>
		public Type ElementType { get { return elementType; } }

		protected override bool PrepareMember(MemberDescriptorBase member)
		{
			if (member.Name == "SyncRoot") return false;

			return base.PrepareMember(member);
		}
	}
}