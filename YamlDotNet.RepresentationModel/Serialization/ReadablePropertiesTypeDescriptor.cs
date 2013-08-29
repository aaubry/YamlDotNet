using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Returns the properties of a type that are readable.
	/// </summary>
	public sealed class ReadablePropertiesTypeDescriptor : TypeDescriptorSkeleton
	{
		private readonly ITypeResolver _reflectionPropertyDescriptorFactory;

		public ReadablePropertiesTypeDescriptor(ITypeResolver reflectionPropertyDescriptorFactory)
		{
			if (reflectionPropertyDescriptorFactory == null)
			{
				throw new ArgumentNullException("reflectionPropertyDescriptorFactory");
			}

			_reflectionPropertyDescriptorFactory = reflectionPropertyDescriptorFactory;
		}

		private static bool IsValidProperty(PropertyInfo property)
		{
			return property.CanRead
				&& property.GetGetMethod().GetParameters().Length == 0;
		}

		public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
		{
			return type
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(IsValidProperty)
				.Select(p =>
				{
					var propertyValue = container != null
						? p.GetValue(container, null)
						: null;

					return (IPropertyDescriptor)new ReflectionPropertyDescriptor(p, _reflectionPropertyDescriptorFactory.Resolve(p.PropertyType, propertyValue), propertyValue);
				});
		}

		private sealed class ReflectionPropertyDescriptor : IPropertyDescriptor
		{
			private readonly PropertyInfo _propertyInfo;

			public ReflectionPropertyDescriptor(PropertyInfo propertyInfo, Type type, object value)
			{
				_propertyInfo = propertyInfo;
				Type = type;
				Value = value;
			}

			public string Name { get { return _propertyInfo.Name; } }
			public Type Type { get; private set; }
			public Type StaticType { get { return _propertyInfo.PropertyType; } }
			public object Value { get; private set; }
			public bool CanWrite { get { return _propertyInfo.CanWrite; } }

			public void SetValue(object target, object value)
			{
				_propertyInfo.SetValue(target, value, null);
			}

			public T GetCustomAttribute<T>() where T : Attribute
			{
				return _propertyInfo.GetCustomAttributes(typeof(T), true)
					.Cast<T>()
					.SingleOrDefault();
			}
		}
	}
}
