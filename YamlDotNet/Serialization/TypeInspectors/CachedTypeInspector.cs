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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Helpers;

namespace YamlDotNet.Serialization.TypeInspectors
{
    /// <summary>
    /// Wraps another <see cref="ITypeInspector"/> and applies caching.
    /// </summary>
    public class CachedTypeInspector : TypeInspectorSkeleton
    {
        private readonly ITypeInspector innerTypeDescriptor;
        private readonly ConcurrentDictionary<Type, List<IPropertyDescriptor>> cache = new ConcurrentDictionary<Type, List<IPropertyDescriptor>>();
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, string>> enumNameCache = new ConcurrentDictionary<Type, ConcurrentDictionary<string, string>>();
        private readonly ConcurrentDictionary<object, string> enumValueCache = new ConcurrentDictionary<object, string>();

        public CachedTypeInspector(ITypeInspector innerTypeDescriptor)
        {
            this.innerTypeDescriptor = innerTypeDescriptor ?? throw new ArgumentNullException(nameof(innerTypeDescriptor));
        }

        public override string GetEnumName(Type enumType, string name)
        {
            var cache = enumNameCache.GetOrAdd(enumType, _ => new ConcurrentDictionary<string, string>());
            var result = cache.GetOrAdd(name, static (n, context) =>
            {
                var (et, typeDescriptor) = context;
                return typeDescriptor.GetEnumName(et, n);
            },
            (enumType, innerTypeDescriptor));
            return result;
        }

        public override string GetEnumValue(object enumValue)
        {
            var result = enumValueCache.GetOrAdd(enumValue, static (_, context) =>
            {
                var (ev, typeDescriptor) = context;
                return typeDescriptor.GetEnumValue(ev);
            },
            (enumValue, innerTypeDescriptor));
            return result;
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
        {
            return cache.GetOrAdd(type, static (t, context) =>
            {
                var (c, typeDescriptor) = context;
                return typeDescriptor.GetProperties(t, c).ToList();
            },
            (container, innerTypeDescriptor));
        }
    }
}
