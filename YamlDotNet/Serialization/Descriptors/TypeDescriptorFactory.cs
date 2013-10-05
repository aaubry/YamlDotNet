using System;
using System.Collections;
using System.Collections.Generic;

namespace YamlDotNet.Serialization.Descriptors
{
	/// <summary>
	/// A default implementation for the <see cref="ITypeDescriptorFactory"/>.
	/// </summary>
	public class TypeDescriptorFactory : ITypeDescriptorFactory
	{
		private readonly IAttributeRegistry attributeRegistry;
		protected readonly Dictionary<Type,ITypeDescriptor> RegisteredDescriptors = new Dictionary<Type, ITypeDescriptor>();

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeDescriptorFactory" /> class.
		/// </summary>
		/// <param name="attributeRegistry">The attribute registry.</param>
		public TypeDescriptorFactory(IAttributeRegistry attributeRegistry)
		{
			this.attributeRegistry = attributeRegistry;
		}

		/// <summary>
		/// Gets the settings.
		/// </summary>
		/// <value>The settings.</value>
		public IAttributeRegistry AttributeRegistry
		{
			get { return attributeRegistry; }
		}

		public ITypeDescriptor Find(Type type)
		{
			if (type == null)
				return null;

			// Caching is integrated in this class, avoiding a ChainedTypeDescriptorFactory
			ITypeDescriptor descriptor;
			if (RegisteredDescriptors.TryGetValue(type, out descriptor))
			{
				return descriptor;
			}

			descriptor = Create(type);

			// Register this descriptor
			RegisteredDescriptors.Add(type, descriptor);

			return descriptor;
		}

		/// <summary>
		/// Creates a type descriptor for the specified type.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>An instance of type descriptor.</returns>
		protected virtual ITypeDescriptor Create(Type type)
		{
			ITypeDescriptor descriptor;
			if (typeof(IDictionary).IsAssignableFrom(type))
			{
				// IDictionary
				descriptor = new DictionaryDescriptor(attributeRegistry, type);
			}
			else if (typeof(ICollection).IsAssignableFrom(type))
			{
				// ICollection
				descriptor = new CollectionDescriptor(attributeRegistry, type);
			}
			else if (type.IsArray)
			{
				// array[]
				descriptor = new ArrayDescriptor(attributeRegistry, type);
			}
			else
			{
				// standard object (class or value type)
				descriptor = new ObjectDescriptor(attributeRegistry, type);
			}
			return descriptor;
		}
	}
}