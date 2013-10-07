using System;
using System.Collections.Generic;

namespace YamlDotNet.Serialization.Descriptors
{
	/// <summary>
	/// A default implementation for the <see cref="ITypeDescriptorFactory"/>.
	/// </summary>
	internal class TypeDescriptorFactory : ITypeDescriptorFactory
	{
		private readonly IAttributeRegistry attributeRegistry;
		private readonly Dictionary<Type,ITypeDescriptor> registeredDescriptors = new Dictionary<Type, ITypeDescriptor>();
		private readonly bool emitDefaultValues;

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeDescriptorFactory" /> class.
		/// </summary>
		/// <param name="attributeRegistry">The attribute registry.</param>
		/// <param name="emitDefaultValues">if set to <c>true</c> [emit default values].</param>
		/// <exception cref="System.ArgumentNullException">attributeRegistry</exception>
		public TypeDescriptorFactory(IAttributeRegistry attributeRegistry, bool emitDefaultValues)
		{
			if (attributeRegistry == null) throw new ArgumentNullException("attributeRegistry");
			this.emitDefaultValues = emitDefaultValues;
			this.attributeRegistry = attributeRegistry;
		}

		public ITypeDescriptor Find(Type type)
		{
			if (type == null)
				return null;

			// Caching is integrated in this class, avoiding a ChainedTypeDescriptorFactory
			ITypeDescriptor descriptor;
			if (registeredDescriptors.TryGetValue(type, out descriptor))
			{
				return descriptor;
			}

			descriptor = Create(type);

			// Register this descriptor
			registeredDescriptors.Add(type, descriptor);

			return descriptor;
		}

		/// <summary>
		/// Gets the settings.
		/// </summary>
		/// <value>The settings.</value>
		protected IAttributeRegistry AttributeRegistry
		{
			get { return attributeRegistry; }
		}

		/// <summary>
		/// Creates a type descriptor for the specified type.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>An instance of type descriptor.</returns>
		protected virtual ITypeDescriptor Create(Type type)
		{
			ITypeDescriptor descriptor;
			if (PrimitiveDescriptor.IsPrimitive(type))
			{
				descriptor = new PrimitiveDescriptor(attributeRegistry, type);
			}
			else if (DictionaryDescriptor.IsDictionary(type))
			{
				// IDictionary
				descriptor = new DictionaryDescriptor(attributeRegistry, type, emitDefaultValues);
			}
			else if (CollectionDescriptor.IsCollection(type))
			{
				// ICollection
				descriptor = new CollectionDescriptor(attributeRegistry, type, emitDefaultValues);
			}
			else if (type.IsArray)
			{
				// array[]
				descriptor = new ArrayDescriptor(attributeRegistry, type);
			}
			else
			{
				// standard object (class or value type)
				descriptor = new ObjectDescriptor(attributeRegistry, type, emitDefaultValues);
			}
			return descriptor;
		}
	}
}