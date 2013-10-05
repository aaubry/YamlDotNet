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
		private readonly Type keyType;
		private readonly Type valueType;

		/// <summary>
		/// Initializes a new instance of the <see cref="DictionaryDescriptor" /> class.
		/// </summary>
		/// <param name="settings">The serializer settings.</param>
		/// <param name="type">The type.</param>
		public DictionaryDescriptor(YamlSerializerSettings settings, Type type) : base(settings, type)
		{
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
		/// Determines whether the value passed is readonly.
		/// </summary>
		/// <param name="thisObject">The this object.</param>
		/// <returns><c>true</c> if [is read only] [the specified this object]; otherwise, <c>false</c>.</returns>
		public bool IsReadOnly(object thisObject)
		{
			return ((IDictionary)thisObject).IsReadOnly;
		}
	}
}