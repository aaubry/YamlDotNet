// This file is part of YamlDotNet - A .NET library for YAML.
// Copyright (c) 2013 aaubry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Linq;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using YamlDotNet;
using YamlDotNet.Events;

namespace YamlDotNet.Serialization.NodeDeserializers
{
	public sealed class ObjectNodeDeserializer : INodeDeserializer
	{
		private readonly IObjectFactory _objectFactory;
		private readonly ITypeDescriptor _typeDescriptor;

		public ObjectNodeDeserializer(IObjectFactory objectFactory, ITypeDescriptor typeDescriptor)
		{
			_objectFactory = objectFactory;
			_typeDescriptor = typeDescriptor;
		}

		bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
		{
			var mapping = reader.Allow<MappingStart>();
			if (mapping == null)
			{
				value = null;
				return false;
			}
			
			value = _objectFactory.Create(expectedType);
			while (!reader.Accept<MappingEnd>())
			{
				var propertyName = reader.Expect<Scalar>();
				
				var property = _typeDescriptor.GetProperty(expectedType, propertyName.Value).Property;
				var propertyValue = nestedObjectDeserializer(reader, property.PropertyType);
				var propertyValuePromise = propertyValue as IValuePromise;
				if (propertyValuePromise == null)
				{
					var convertedValue = TypeConverter.ChangeType(propertyValue, property.PropertyType);
					property.SetValue(value, convertedValue, null);
				}
				else
				{
					var valueRef = value;
					propertyValuePromise.ValueAvailable += v =>
					{
						var convertedValue = TypeConverter.ChangeType(v, property.PropertyType);
						property.SetValue(valueRef, convertedValue, null);
					};
				}
			}

			reader.Expect<MappingEnd>();
			return true;
		}
	}
}
