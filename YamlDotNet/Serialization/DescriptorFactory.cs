using System;
using System.Collections;
using System.Collections.Generic;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// A default implementation for the <see cref="ITypeDescriptorFactory"/>.
	/// </summary>
	public class DescriptorFactory : ITypeDescriptorFactory
	{
		private readonly YamlSerializerSettings settings;
		protected readonly Dictionary<Type,ITypeDescriptor> RegisteredDescriptors = new Dictionary<Type, ITypeDescriptor>();

		/// <summary>
		/// Initializes a new instance of the <see cref="DescriptorFactory"/> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public DescriptorFactory(YamlSerializerSettings settings)
		{
			this.settings = settings;
		}

		/// <summary>
		/// Gets the settings.
		/// </summary>
		/// <value>The settings.</value>
		public YamlSerializerSettings Settings
		{
			get { return settings; }
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
				descriptor = new DictionaryDescriptor(settings, type);
			}
			else if (typeof(ICollection).IsAssignableFrom(type))
			{
				// ICollection
				descriptor = new CollectionDescriptor(settings, type);
			}
			else if (type.IsArray)
			{
				// array[]
				descriptor = new ArrayDescriptor(settings, type);
			}
			else
			{
				// standard object (class or value type)
				descriptor = new ObjectDescriptor(settings, type);
			}
			return descriptor;
		}
	}
}