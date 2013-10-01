using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Globalization;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Returns the properties of a type that are readable.
	/// </summary>
	public class ReadablePropertiesTypeDescriptor : ITypeDescriptor
	{
		private sealed class ReflectionPropertyDescriptor : IPropertyDescriptor
		{
			public ReflectionPropertyDescriptor(PropertyInfo propertyInfo)
			{
				if (propertyInfo == null)
				{
					throw new ArgumentNullException("propertyInfo");
				}

				Property = propertyInfo;
			}

			public PropertyInfo Property { get; private set; }

			public string Name
			{
				get { return Property.Name; }
			}
		}
		
		protected virtual bool IsValidProperty(PropertyInfo property)
		{
			return property.CanRead
				&& property.GetGetMethod().GetParameters().Length == 0;
		}

		public IEnumerable<IPropertyDescriptor> GetProperties(Type type)
		{
			return type
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(IsValidProperty)
				.Select(p => (IPropertyDescriptor)new ReflectionPropertyDescriptor(p));
		}
		
		public IPropertyDescriptor GetProperty(Type type, string name)
		{
			var property = type
				.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
			
			if(property == null || IsValidProperty(property))
			{
				throw new SerializationException(
					string.Format(
						CultureInfo.InvariantCulture,
						"Property '{0}' not found on type '{1}'.",
						name,
						type.FullName
					)
				);
			}
							
			return new ReflectionPropertyDescriptor(property);
		}
	}
}
