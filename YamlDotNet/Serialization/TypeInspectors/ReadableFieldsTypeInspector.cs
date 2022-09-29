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
    /// Returns the properties and fields of a type that are readable.
    /// </summary>
    public sealed class ReadableFieldsTypeInspector : TypeInspectorSkeleton
    {
        private readonly ITypeResolver typeResolver;

        public ReadableFieldsTypeInspector(ITypeResolver typeResolver)
        {
            this.typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
        {
            return type
                .GetPublicFields()
                .Select(p => (IPropertyDescriptor)new ReflectionFieldDescriptor(p, typeResolver));
        }

        private sealed class ReflectionFieldDescriptor : IPropertyDescriptor
        {
            private readonly FieldInfo fieldInfo;
            private readonly ITypeResolver typeResolver;

            public ReflectionFieldDescriptor(FieldInfo fieldInfo, ITypeResolver typeResolver)
            {
                this.fieldInfo = fieldInfo;
                this.typeResolver = typeResolver;
                ScalarStyle = ScalarStyle.Any;
            }

            public string Name { get { return fieldInfo.Name; } }
            public Type Type { get { return fieldInfo.FieldType; } }
            public Type? TypeOverride { get; set; }
            public int Order { get; set; }
            public bool CanWrite { get { return !fieldInfo.IsInitOnly; } }
            public ScalarStyle ScalarStyle { get; set; }

            public void Write(object target, object? value)
            {
                fieldInfo.SetValue(target, value);
            }

            public T GetCustomAttribute<T>() where T : Attribute
            {
                var attributes = fieldInfo.GetCustomAttributes(typeof(T), true);
                return (T)attributes.FirstOrDefault();
            }

            public IObjectDescriptor Read(object target)
            {
                var propertyValue = fieldInfo.GetValue(target);
                var actualType = TypeOverride ?? typeResolver.Resolve(Type, propertyValue);
                return new ObjectDescriptor(propertyValue, actualType, Type, ScalarStyle);
            }
        }
    }
}
