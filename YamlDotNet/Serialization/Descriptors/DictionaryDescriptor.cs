using System;
using System.Collections;
using System.Collections.Generic;

namespace YamlDotNet.Serialization.Descriptors
{
	/// <summary>
	/// Provides a descriptor for a <see cref="System.Collections.IDictionary"/>.
	/// </summary>
	public class DictionaryDescriptor : ObjectDescriptor
	{
		private static readonly string SystemCollectionsNamespace = typeof(IList).Namespace;
		private static readonly List<string> ListOfMembersToRemove = new List<string> {"Comparer", "Keys", "Values"};

		private readonly Type keyType;
		private readonly Type valueType;

		/// <summary>
		/// Initializes a new instance of the <see cref="DictionaryDescriptor" /> class.
		/// </summary>
		/// <param name="attributeRegistry">The attribute registry.</param>
		/// <param name="type">The type.</param>
		public DictionaryDescriptor(IAttributeRegistry attributeRegistry, Type type)
			: base(attributeRegistry, type)
		{
			if (!IsDictionary(type))
				throw new ArgumentException("Expecting a type inheriting from System.Collections.IDictionary", "type");

			// extract Key, Value types from IDictionary<??, ??>
			var interfaceType = type.GetInterface(typeof(IDictionary<,>));
			if (interfaceType != null)
			{
				keyType = interfaceType.GetGenericArguments()[0];
				valueType = interfaceType.GetGenericArguments()[1];
			}
			else
			{
				keyType = typeof(object);
				valueType = typeof(object);
			}

			// Only Keys and Values
			IsPureDictionary = Count == 0;
		}

		/// <summary>
		/// Gets the type of the key.
		/// </summary>
		/// <value>The type of the key.</value>
		public Type KeyType
		{
			get { return keyType; }
		}

		/// <summary>
		/// Gets the type of the value.
		/// </summary>
		/// <value>The type of the value.</value>
		public Type ValueType
		{
			get { return valueType; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is pure dictionary.
		/// </summary>
		/// <value><c>true</c> if this instance is pure dictionary; otherwise, <c>false</c>.</value>
		public bool IsPureDictionary { get; private set; }

		/// <summary>
		/// Determines whether the value passed is readonly.
		/// </summary>
		/// <param name="thisObject">The this object.</param>
		/// <returns><c>true</c> if [is read only] [the specified this object]; otherwise, <c>false</c>.</returns>
		public bool IsReadOnly(object thisObject)
		{
			return ((IDictionary)thisObject).IsReadOnly;
		}

		/// <summary>
		/// Determines whether the specified type is a .NET dictionary.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns><c>true</c> if the specified type is dictionary; otherwise, <c>false</c>.</returns>
		public static bool IsDictionary(Type type)
		{
			return typeof (IDictionary).IsAssignableFrom(type);
		}

		protected override bool PrepareMember(MemberDescriptorBase member)
		{
			// Remove SyncRoot from members as well
			if (member is PropertyDescriptor && (member.DeclaringType.Namespace ?? string.Empty).StartsWith(SystemCollectionsNamespace)
				&& ListOfMembersToRemove.Contains(member.Name))
			{
				return false;
			}

			return base.PrepareMember(member);
		}
	}
}