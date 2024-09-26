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

namespace YamlDotNet.Serialization.TypeInspectors
{
    /// <summary>
    /// Wraps another <see cref="ITypeInspector"/> and applies a
    /// naming convention to the names of the properties.
    /// </summary>
    public class NamingConventionTypeInspector : TypeInspectorSkeleton
    {
        private readonly ITypeInspector innerTypeDescriptor;
        private readonly INamingConvention namingConvention;

        public NamingConventionTypeInspector(ITypeInspector innerTypeDescriptor, INamingConvention namingConvention)
        {
            this.innerTypeDescriptor = innerTypeDescriptor ?? throw new ArgumentNullException(nameof(innerTypeDescriptor));
            this.namingConvention = namingConvention ?? throw new ArgumentNullException(nameof(namingConvention));
        }

        public override string GetEnumName(Type enumType, string name) => this.innerTypeDescriptor.GetEnumName(enumType, name);

        public override string GetEnumValue(object enumValue) => this.innerTypeDescriptor.GetEnumValue(enumValue);

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
        {
            return innerTypeDescriptor.GetProperties(type, container)
                .Select(p =>
                {
                    var attribute = p.GetCustomAttribute<YamlMemberAttribute>();
                    if (attribute != null && !attribute.ApplyNamingConventions)
                    {
                        return p;
                    }
                    return new PropertyDescriptor(p) { Name = namingConvention.Apply(p.Name) };
                });
        }
    }
}
