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
using YamlDotNet.Serialization.TypeInspectors;

namespace YamlDotNet.Serialization
{
    /// <summary>
    /// Applies the Yaml attribute overrides to another <see cref="ITypeInspector"/>.
    /// </summary>
    public sealed class YamlAttributeOverridesInspector : ReflectionTypeInspector
    {
        private readonly ITypeInspector innerTypeDescriptor;
        private readonly YamlAttributeOverrides overrides;

        public YamlAttributeOverridesInspector(ITypeInspector innerTypeDescriptor, YamlAttributeOverrides overrides)
        {
            this.innerTypeDescriptor = innerTypeDescriptor;
            this.overrides = overrides;
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
        {
            var properties = innerTypeDescriptor.GetProperties(type, container);
            if (overrides != null)
            {
                properties = properties
                    .Select(p => (IPropertyDescriptor)new OverridePropertyDescriptor(p, overrides, type));
            }

            return properties;
        }

        public sealed class OverridePropertyDescriptor : IPropertyDescriptor
        {
            private readonly IPropertyDescriptor baseDescriptor;
            private readonly YamlAttributeOverrides overrides;
            private readonly Type classType;

            public OverridePropertyDescriptor(IPropertyDescriptor baseDescriptor, YamlAttributeOverrides overrides, Type classType)
            {
                this.baseDescriptor = baseDescriptor;
                this.overrides = overrides;
                this.classType = classType;
            }

            public string Name { get { return baseDescriptor.Name; } }
            public bool Required { get => baseDescriptor.Required; }
            public bool AllowNulls { get => baseDescriptor.AllowNulls; }

            public bool CanWrite { get { return baseDescriptor.CanWrite; } }

            public Type Type { get { return baseDescriptor.Type; } }

            public Type? TypeOverride
            {
                get { return baseDescriptor.TypeOverride; }
                set { baseDescriptor.TypeOverride = value; }
            }

            public Type? ConverterType => baseDescriptor.ConverterType;

            public int Order
            {
                get { return baseDescriptor.Order; }
                set { baseDescriptor.Order = value; }
            }

            public ScalarStyle ScalarStyle
            {
                get { return baseDescriptor.ScalarStyle; }
                set { baseDescriptor.ScalarStyle = value; }
            }

            public void Write(object target, object? value)
            {
                baseDescriptor.Write(target, value);
            }

            public T? GetCustomAttribute<T>() where T : Attribute
            {
                var attr = overrides.GetAttribute<T>(classType, Name);
                return attr ?? baseDescriptor.GetCustomAttribute<T>();
            }

            public IObjectDescriptor Read(object target)
            {
                return baseDescriptor.Read(target);
            }
        }
    }
}
