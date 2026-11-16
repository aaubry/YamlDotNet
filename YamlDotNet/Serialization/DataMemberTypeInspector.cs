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
using System.Runtime.Serialization;
using YamlDotNet.Serialization.TypeInspectors;

namespace YamlDotNet.Serialization
{
    /// <summary>
    /// Applies the <see cref="DataMemberAttribute"/> and <see cref="IgnoreDataMemberAttribute"/>
    /// attributes to another <see cref="ITypeInspector"/>.
    /// This inspector is opt-in. Only <see cref="DataMemberAttribute.Name"/> and
    /// <see cref="DataMemberAttribute.Order"/> (when explicitly set, i.e. not the attribute default of -1) are honored.
    /// Members without attributes are left untouched, and <see cref="DataContractAttribute"/> is not required.
    /// </summary>
    public sealed class DataMemberTypeInspector : TypeInspectorSkeleton
    {
        // The default value of DataMemberAttribute.Order when it has not been set explicitly.
        private const int UnsetOrder = -1;

        private readonly ITypeInspector innerTypeDescriptor;

        public DataMemberTypeInspector(ITypeInspector innerTypeDescriptor)
        {
            this.innerTypeDescriptor = innerTypeDescriptor;
        }

        public override string GetEnumName(Type enumType, string name) => innerTypeDescriptor.GetEnumName(enumType, name);

        public override string GetEnumValue(object enumValue) => innerTypeDescriptor.GetEnumValue(enumValue);

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
        {
            return innerTypeDescriptor.GetProperties(type, container)
                .Where(p => p.GetCustomAttribute<IgnoreDataMemberAttribute>() == null)
                .Select(p =>
                {
                    var member = p.GetCustomAttribute<DataMemberAttribute>();
                    if (member == null)
                    {
                        return p;
                    }

                    var descriptor = new PropertyDescriptor(p)
                    {
                        Order = p.Order
                    };

                    if (!string.IsNullOrEmpty(member.Name))
                    {
                        descriptor.Name = member.Name!;
                    }

                    if (member.Order != UnsetOrder)
                    {
                        descriptor.Order = member.Order;
                    }

                    return (IPropertyDescriptor)descriptor;
                })
                .OrderBy(p => p.Order);
        }

        public override bool HasParseMethod(Type type) => this.innerTypeDescriptor.HasParseMethod(type);

        public override object? Parse(string value, Type expectedType) => this.innerTypeDescriptor.Parse(value, expectedType);
    }
}
