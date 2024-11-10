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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using YamlDotNet.Helpers;

namespace YamlDotNet.Serialization.Utilities
{
    /// <summary>
    /// A cache / map for <see cref="IYamlTypeConverter"/> instances.
    /// </summary>
    internal sealed class TypeConverterCache
    {
        private readonly IYamlTypeConverter[] typeConverters;
        private readonly ConcurrentDictionary<Type, (bool HasMatch, IYamlTypeConverter? TypeConverter)> cache = new();

        public TypeConverterCache(IEnumerable<IYamlTypeConverter>? typeConverters) : this(typeConverters?.ToArray() ?? new IYamlTypeConverter[]{})
        {
        }

        public TypeConverterCache(IYamlTypeConverter[] typeConverters)
        {
            this.typeConverters = typeConverters;
        }

        /// <summary>
        /// Returns the first <see cref="IYamlTypeConverter"/> that accepts the given type.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to lookup.</param>
        /// <param name="typeConverter">The <see cref="IYamlTypeConverter" /> that accepts this type or <see langword="false" /> if no converter is found.</param>
        /// <returns><see langword="true"/> if a type converter was found; <see langword="false"/> otherwise.</returns>
        public bool TryGetConverterForType(Type type, [NotNullWhen(true)] out IYamlTypeConverter? typeConverter)
        {
            var result = cache.GetOrAdd(type, static (t, tc) => LookupTypeConverter(t, tc), typeConverters);

            typeConverter = result.TypeConverter;
            return result.HasMatch;
        }

        /// <summary>
        /// Returns the <see cref="IYamlTypeConverter"/> of the given type.
        /// </summary>
        /// <param name="converter">The type of the converter.</param>
        /// <returns>The <see cref="IYamlTypeConverter"/> of the given type.</returns>
        /// <exception cref="ArgumentException">If no type converter of the given type is found.</exception>
        /// <remarks>
        /// Note that this method searches on the type of the <see cref="IYamlTypeConverter"/> itself. If you want to find a type converter
        /// that accepts a given <see cref="Type"/>, use <see cref="TryGetConverterForType(Type, out IYamlTypeConverter?)"/> instead.
        /// </remarks>
        public IYamlTypeConverter GetConverterByType(Type converter)
        {
            // Intentially avoids LINQ as this is on a hot path
            foreach (var typeConverter in typeConverters)
            {
                if (typeConverter.GetType() == converter)
                {
                    return typeConverter;
                }
            }

            throw new ArgumentException($"{nameof(IYamlTypeConverter)} of type {converter.FullName} not found", nameof(converter));
        }

        private static (bool HasMatch, IYamlTypeConverter? TypeConverter) LookupTypeConverter(Type type, IYamlTypeConverter[] typeConverters)
        {
            // Intentially avoids LINQ as this is on a hot path
            foreach (var typeConverter in typeConverters)
            {
                if (typeConverter.Accepts(type))
                {
                    return (true, typeConverter);
                }
            }

            return (false, null);
        }
    }
}
