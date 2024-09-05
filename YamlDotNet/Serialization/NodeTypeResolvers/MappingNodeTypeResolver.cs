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
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization.NodeTypeResolvers
{
    public class MappingNodeTypeResolver : INodeTypeResolver
    {
        private readonly IDictionary<Type, Type> mappings;

        public MappingNodeTypeResolver(IDictionary<Type, Type> mappings)
        {
            if (mappings == null)
            {
                throw new ArgumentNullException(nameof(mappings));
            }

            foreach (var pair in mappings)
            {
                if (!pair.Key.IsAssignableFrom(pair.Value))
                {
                    throw new InvalidOperationException($"Type '{pair.Value}' does not implement type '{pair.Key}'.");
                }
            }

            this.mappings = mappings;
        }

        public bool Resolve(NodeEvent? nodeEvent, ref Type currentType)
        {
            if (mappings.TryGetValue(currentType, out var concreteType))
            {
                currentType = concreteType;
                return true;
            }

            return false;
        }
    }
}
