// This file is part of YamlDotNet - A .NET library for YAML.
// Copyright (c) Antoine Aubry and contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.TypeInspectors
{
    /// <summary>
    /// Returns the properties of a type that are writable.
    /// </summary>
    public sealed class WritablePropertiesTypeInspector : TypeInspectorSkeleton
    {
        private readonly ITypeResolver typeResolver;
        private readonly bool includeNonPublicProperties;

        public WritablePropertiesTypeInspector(ITypeResolver typeResolver)
            : this(typeResolver, false)
        {
        }

        public WritablePropertiesTypeInspector(ITypeResolver typeResolver, bool includeNonPublicProperties)
        {
            this.typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));
            this.includeNonPublicProperties = includeNonPublicProperties;
        }

        private static bool IsValidProperty(PropertyInfo property)
        {
            return property.CanWrite
                && property.GetSetMethod(true)!.GetParameters().Length == 1;
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
        {
            return type
                .GetProperties(includeNonPublicProperties)
                .Where(IsValidProperty)
                .Select(p => (IPropertyDescriptor)new ReflectionPropertyDescriptor(p, typeResolver))
                .ToArray();
        }

        private sealed class ReflectionPropertyDescriptor : IPropertyDescriptor
        {
            private readonly PropertyInfo propertyInfo;
            private readonly ITypeResolver typeResolver;

            public ReflectionPropertyDescriptor(PropertyInfo propertyInfo, ITypeResolver typeResolver)
            {
                this.propertyInfo = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));
                this.typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));
                ScalarStyle = ScalarStyle.Any;
            }

            public string Name => propertyInfo.Name;
            public Type Type => propertyInfo.PropertyType;
            public Type? TypeOverride { get; set; }
            public int Order { get; set; }
            public bool CanWrite => propertyInfo.CanWrite;
            public ScalarStyle ScalarStyle { get; set; }

            public void Write(object target, object? value)
            {
                propertyInfo.SetValue(target, value, null);
            }

            public T GetCustomAttribute<T>() where T : Attribute
            {
                var attributes = propertyInfo.GetAllCustomAttributes<T>();
                return (T)attributes.FirstOrDefault();
            }

            public IObjectDescriptor Read(object target)
            {
                var propertyValue = propertyInfo.ReadValue(target);
                var actualType = TypeOverride ?? typeResolver.Resolve(Type, propertyValue);
                return new ObjectDescriptor(propertyValue, actualType, Type, ScalarStyle);
            }
        }
    }
}
