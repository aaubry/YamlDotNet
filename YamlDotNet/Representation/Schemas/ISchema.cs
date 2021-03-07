//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) Antoine Aubry and contributors

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YamlDotNet.Core;

namespace YamlDotNet.Representation.Schemas
{
    public interface ISchema
    {
        ISchemaIterator Root { get; }
        IEnumerable<Type> KnownTypes { get; }
    }

    public interface ISchemaIterator
    {
        bool TryEnterNode(INode node, [NotNullWhen(true)] out ISchemaIterator? childIterator, [NotNullWhen(true)] out INodeMapper? mapper);
        bool TryEnterValue(object? value, [NotNullWhen(true)] out ISchemaIterator? childIterator, [NotNullWhen(true)] out INodeMapper? mapper);
        bool TryEnterMappingValue([NotNullWhen(true)] out ISchemaIterator? childIterator);

        bool IsTagImplicit(IScalar scalar, out ScalarStyle style);
        bool IsTagImplicit(ISequence sequence, out SequenceStyle style);
        bool IsTagImplicit(IMapping mapping, out MappingStyle style);
    }

    public static class SchemaIteratorExtensions
    {
        public static ISchemaIterator EnterNode(this ISchemaIterator iterator, INode node, out INodeMapper mapper)
        {
            if (iterator.TryEnterNode(node, out var childIterator, out mapper!))
            {
                return childIterator;
            }

            mapper = new UnresolvedTagMapper(node.Tag);
            return NullSchemaIterator.Instance;
        }

        public static ISchemaIterator EnterValue(this ISchemaIterator iterator, object? value, out INodeMapper mapper)
        {
            if (iterator.TryEnterValue(value, out var childIterator, out mapper!))
            {
                return childIterator;
            }

            mapper = new UnresolvedValueMapper(value);
            return NullSchemaIterator.Instance;
        }

        public static ISchemaIterator EnterMappingValue(this ISchemaIterator iterator)
        {
            return iterator.TryEnterMappingValue(out var childIterator)
                ? childIterator
                : NullSchemaIterator.Instance;
        }

        private sealed class NullSchemaIterator : ISchemaIterator
        {
            private NullSchemaIterator() { }

            public static readonly ISchemaIterator Instance = new NullSchemaIterator();

            public bool TryEnterNode(INode node, [NotNullWhen(true)] out ISchemaIterator? childIterator, [NotNullWhen(true)] out INodeMapper? mapper)
            {
                childIterator = null;
                mapper = null;
                return false;
            }

            public bool TryEnterValue(object? value, [NotNullWhen(true)] out ISchemaIterator? childIterator, [NotNullWhen(true)] out INodeMapper? mapper)
            {
                childIterator = null;
                mapper = null;
                return false;
            }

            public bool TryEnterMappingValue([NotNullWhen(true)] out ISchemaIterator? childIterator)
            {
                childIterator = null;
                return false;
            }

            public bool IsTagImplicit(IScalar scalar, out ScalarStyle style)
            {
                style = default;
                return false;
            }

            public bool IsTagImplicit(ISequence sequence, out SequenceStyle style)
            {
                style = default;
                return false;
            }

            public bool IsTagImplicit(IMapping mapping, out MappingStyle style)
            {
                style = default;
                return false;
            }
        }
    }
}