//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2013 Antoine Aubry
    
//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:
    
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
    
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace YamlDotNet.Serialization.TypeInspectors
{
	/// <summary>
	/// Returns the properties of a type that are readable.
	/// </summary>
	public sealed class ReadablePropertiesTypeInspector : TypeInspectorSkeleton
	{
		private readonly ITypeResolver _reflectionPropertyDescriptorFactory;

		public ReadablePropertiesTypeInspector(ITypeResolver reflectionPropertyDescriptorFactory)
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
				var attributes = _propertyInfo.GetCustomAttributes(typeof(T), true);
				return attributes.Length > 0
					? (T)attributes[0]
					: null;
			}
		}
	}
}
