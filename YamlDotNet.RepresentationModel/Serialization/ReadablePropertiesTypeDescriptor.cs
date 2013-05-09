using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

		public virtual IEnumerable<IPropertyDescriptor> GetProperties(Type type)
		{
			return type
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(p => p.CanRead && p.GetGetMethod().GetParameters().Length == 0)
				.Select(p => (IPropertyDescriptor)new ReflectionPropertyDescriptor(p));
		}
	}
}
