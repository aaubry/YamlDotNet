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
using YamlDotNet.Core;

namespace YamlDotNet.Representation.Schemas
{
    public static class NodeMapper
    {
        private sealed class LambdaNodeMapper : INodeMapper
        {
            private readonly Func<Node, object?> constructor;
            private readonly Func<object?, INodeMapper, Node> representer;

            public LambdaNodeMapper(TagName tag, Func<Node, object?> constructor, Func<object?, INodeMapper, Node> representer, INodeMapper? canonical)
            {
                Tag = tag;
                this.constructor = constructor ?? throw new ArgumentNullException(nameof(constructor));
                this.representer = representer ?? throw new ArgumentNullException(nameof(representer));
                Canonical = canonical ?? this;
            }

            public TagName Tag { get; }
            public INodeMapper Canonical { get; }

            public object? Construct(Node node) => constructor(node);
            public Node Represent(object? native, ISchemaIterator iterator, IRepresentationState state)
            {
                state.RecursionLevel.Increment();
                var node = representer(native, this);
                state.RecursionLevel.Decrement();
                return node;
            }

            public override string ToString() => Tag.ToString();
        }

        public static INodeMapper CreateScalarMapper(TagName tag, Func<Scalar, object?> constructor, Func<object?, string> representer, INodeMapper? canonical = null)
        {
            return CreateScalarMapper(tag, constructor, (n, m) => new Scalar(m, representer(n)), canonical);
        }

        public static INodeMapper CreateScalarMapper(TagName tag, Func<Scalar, object?> constructor, Func<object?, INodeMapper, Node> representer, INodeMapper? canonical = null)
        {
            return new LambdaNodeMapper(tag, n => constructor(n.Expect<Scalar>()), representer, canonical);
        }
    }
}
