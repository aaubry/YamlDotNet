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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace YamlDotNet.Serialization.TypeInspectors
{
    public abstract class TypeInspectorSkeleton : ITypeInspector
    {
        public abstract string GetEnumName(Type enumType, string name);

        public abstract string GetEnumValue(object enumValue);

        public abstract IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container);

        public IPropertyDescriptor GetProperty(Type type, object? container, string name, [MaybeNullWhen(true)] bool ignoreUnmatched, bool caseInsensitivePropertyMatching)
        {
            // This runs once per YAML key during deserialization, so avoid the per-call LINQ closure
            // and iterator that .Where(p => p.Name == name) allocated. When the property list is an
            // IReadOnlyList (it is when it comes from CachedTypeInspector) the match scan is
            // allocation-free. Semantics are unchanged: no match, single match, and ambiguous match
            // are handled exactly as before.
            var comparison = caseInsensitivePropertyMatching ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var properties = GetProperties(type, container);

            IPropertyDescriptor? match = null;

            if (properties is IReadOnlyList<IPropertyDescriptor> list)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    var candidate = list[i];
                    if (candidate.Name.Equals(name, comparison))
                    {
                        if (match != null)
                        {
                            throw MultipleMatchesException(type, container, name, comparison);
                        }

                        match = candidate;
                    }
                }
            }
            else
            {
                foreach (var candidate in properties)
                {
                    if (candidate.Name.Equals(name, comparison))
                    {
                        if (match != null)
                        {
                            throw MultipleMatchesException(type, container, name, comparison);
                        }

                        match = candidate;
                    }
                }
            }

            if (match == null)
            {
                if (ignoreUnmatched)
                {
                    return null!;
                }

                throw new SerializationException($"Property '{name}' not found on type '{type.FullName}'.");
            }

            return match;
        }

        private SerializationException MultipleMatchesException(Type type, object? container, string name, StringComparison comparison)
        {
            var matches = GetProperties(type, container)
                .Where(p => p.Name.Equals(name, comparison))
                .Select(p => p.Name);

            return new SerializationException(
                $"Multiple properties with the name/alias '{name}' already exists on type '{type.FullName}', maybe you're misusing YamlAlias or maybe you are using the wrong naming convention? The matching properties are: {string.Join(", ", matches.ToArray())}"
            );
        }

        public abstract bool HasParseMethod(Type type);

        public abstract object? Parse(string value, Type expectedType);
    }
}
