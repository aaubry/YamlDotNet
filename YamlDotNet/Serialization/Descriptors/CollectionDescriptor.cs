using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace YamlDotNet.Serialization.Descriptors
{
	/// <summary>
	/// Provides a descriptor for a <see cref="System.Collections.ICollection"/>.
	/// </summary>
	public class CollectionDescriptor : ObjectDescriptor
	{
		private static readonly string SystemCollectionsNamespace = typeof(IList).Namespace;

		/// <summary>
		/// Initializes a new instance of the <see cref="CollectionDescriptor" /> class.
		/// </summary>
		/// <param name="attributeRegistry">The attribute registry.</param>
		/// <param name="type">The type.</param>
		public CollectionDescriptor(IAttributeRegistry attributeRegistry, Type type)
			: base(attributeRegistry, type)
		{
			if (!IsCollection(type))
				throw new ArgumentException("Expecting a type inheriting from System.Collections.ICollection", "type");

			// Gets the element type
			var collectionType = type.GetInterface(typeof(ICollection<>));
			ElementType = (collectionType != null) ? collectionType.GetGenericArguments()[0] : typeof(object);

			// Finds if it is a pure list
			var capacityMember = this["Capacity"] as PropertyDescriptor;
			HasOnlyCapacity = Count == 1 && capacityMember != null &&
			              (capacityMember.DeclaringType.Namespace ?? string.Empty).StartsWith(SystemCollectionsNamespace);

			IsPureCollection = Count == 0;
		}

		/// <summary>
		/// Gets or sets the type of the element.
		/// </summary>
		/// <value>The type of the element.</value>
		public Type ElementType { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the collection has only the capacity as a property defined.
		/// </summary>
		/// <value><c>true</c> if the collection has only the capacity as a property defined; otherwise, <c>false</c>.</value>
		public bool HasOnlyCapacity { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this instance is a pure collection (no public property/field)
		/// </summary>
		/// <value><c>true</c> if this instance is pure collection; otherwise, <c>false</c>.</value>
		public bool IsPureCollection { get; private set; }

		/// <summary>
		/// Determines whether the specified type is collection.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns><c>true</c> if the specified type is collection; otherwise, <c>false</c>.</returns>
		public static bool IsCollection(Type type)
		{
			return typeof (ICollection).IsAssignableFrom(type);
		}

		protected override bool PrepareMember(MemberDescriptorBase member)
		{
			// Remove SyncRoot from members as well
			if (member is PropertyDescriptor && member.Name == "SyncRoot" &&
				(member.DeclaringType.Namespace ?? string.Empty).StartsWith(SystemCollectionsNamespace))
			{
				return false;
			}
			return base.PrepareMember(member);
		}
	}
}