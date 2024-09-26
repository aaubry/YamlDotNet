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
    /// Aggregates the results from multiple <see cref="ITypeInspector" /> into a single one.
    /// </summary>
    public class CompositeTypeInspector : TypeInspectorSkeleton
    {
        private readonly IEnumerable<ITypeInspector> typeInspectors;

        public CompositeTypeInspector(params ITypeInspector[] typeInspectors)
            : this((IEnumerable<ITypeInspector>)typeInspectors)
        {
        }

        public CompositeTypeInspector(IEnumerable<ITypeInspector> typeInspectors)
        {
            this.typeInspectors = typeInspectors?.ToList() ?? throw new ArgumentNullException(nameof(typeInspectors));
        }

        public override string GetEnumName(Type enumType, string name)
        {
            foreach (var inspector in typeInspectors)
            {
                try
                {
                    return inspector.GetEnumName(enumType, name);
                }
                catch
                {
                    //inner inspectors throw when they can't handle the type so we swallow it
                }
            }

            throw new ArgumentOutOfRangeException(nameof(enumType) + "," + nameof(name), "Name not found on enum type");
        }

        public override string GetEnumValue(object enumValue)
        {
            if (enumValue == null)
            {
                throw new ArgumentNullException(nameof(enumValue));
            }

            foreach (var inspector in typeInspectors)
            {
                try
                {
                    return inspector.GetEnumValue(enumValue);
                }
                catch
                {
                    //inner inspectors throw when they can't handle the type so we swallow it
                }
            }

            throw new ArgumentOutOfRangeException(nameof(enumValue), $"Value not found for ({enumValue})");
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
        {
            return typeInspectors
                .SelectMany(i => i.GetProperties(type, container));
        }
    }
}
