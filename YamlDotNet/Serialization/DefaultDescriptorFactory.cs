using System;
using System.Collections;
using System.Collections.Generic;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// Class DefaultDescriptorFactory.
	/// </summary>
	public class DefaultDescriptorFactory : ITypeDescriptorFactory
	{
		private readonly YamlSerializerSettings settings;
		protected readonly Dictionary<Type,ITypeDescriptor> RegisteredDescriptors = new Dictionary<Type, ITypeDescriptor>();

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultDescriptorFactory"/> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public DefaultDescriptorFactory(YamlSerializerSettings settings)
		{
			this.settings = settings;
		}

		public virtual ITypeDescriptor Find(Type type)
		{
			if (type == null)
				return null;

			ITypeDescriptor descriptor;
			if (RegisteredDescriptors.TryGetValue(type, out descriptor))
			{
				return descriptor;
			} 

			if (typeof (IDictionary).IsAssignableFrom(type))
			{
				// IDictionary
				descriptor = new DictionaryDescriptor(settings, type);
			}
			else if (typeof (ICollection).IsAssignableFrom(type))
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

			// Register this descriptor
			RegisteredDescriptors.Add(type, descriptor);

			return descriptor;
		}
	}
}