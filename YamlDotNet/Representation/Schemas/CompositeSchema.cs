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
using System.Linq;
using YamlDotNet.Core;

namespace YamlDotNet.Representation.Schemas
{
    public sealed class CompositeSchema : ISchema
    {
        private readonly ISchema primary;
        private readonly ISchema secondary;

        public CompositeSchema(ISchema primary, ISchema secondary)
        {
            this.primary = primary ?? throw new ArgumentNullException(nameof(primary));
            this.secondary = secondary ?? throw new ArgumentNullException(nameof(secondary));
        }

        public ISchemaIterator Root => new CompositeIterator(primary.Root, secondary.Root);
        public IEnumerable<NodeMatcher> RootMatchers => primary.RootMatchers.Concat(secondary.RootMatchers);

        private sealed class CompositeIterator : ISchemaIterator
        {
            private readonly ISchemaIterator primary;
            private readonly ISchemaIterator secondary;

            public CompositeIterator(ISchemaIterator primary, ISchemaIterator secondary)
            {
                this.primary = primary ?? throw new ArgumentNullException(nameof(primary));
                this.secondary = secondary ?? throw new ArgumentNullException(nameof(secondary));
            }

            public bool TryEnterNode(INode node, [NotNullWhen(true)] out ISchemaIterator? childIterator, [NotNullWhen(true)] out INodeMapper? mapper)
            {
                return TryEnter(node, TryEnterNodeHelper, out childIterator, out mapper);
            }

            public bool TryEnterValue(object? value, [NotNullWhen(true)] out ISchemaIterator? childIterator, [NotNullWhen(true)] out INodeMapper? mapper)
            {
                return TryEnter(value, TryEnterValueHelper, out childIterator, out mapper);
            }

            private delegate bool TryEnterDelegate<T>(ISchemaIterator iterator, T item, [NotNullWhen(true)] out ISchemaIterator? childIterator, [NotNullWhen(true)] out INodeMapper? mapper);

            private static bool TryEnterNodeHelper(ISchemaIterator iterator, INode node, [NotNullWhen(true)] out ISchemaIterator? childIterator, [NotNullWhen(true)] out INodeMapper? mapper)
                => iterator.TryEnterNode(node, out childIterator, out mapper);

            private static bool TryEnterValueHelper(ISchemaIterator iterator, object? value, [NotNullWhen(true)] out ISchemaIterator? childIterator, [NotNullWhen(true)] out INodeMapper? mapper)
                => iterator.TryEnterValue(value, out childIterator, out mapper);

            private bool TryEnter<T>(T item, TryEnterDelegate<T> tryEnter, [NotNullWhen(true)] out ISchemaIterator? childIterator, [NotNullWhen(true)] out INodeMapper? mapper)
            {
                bool entered;
                ISchemaIterator? secondaryChildIterator;

                if (tryEnter(primary, item, out childIterator!, out mapper!))
                {
                    tryEnter(secondary, item, out secondaryChildIterator, out _);
                    entered = true;
                }
                else
                {
                    entered = tryEnter(secondary, item, out secondaryChildIterator, out mapper!);
                }

                if (secondaryChildIterator != null)
                {
                    childIterator = childIterator != null
                        ? new CompositeIterator(childIterator, secondaryChildIterator)
                        : secondaryChildIterator;
                }
                return entered;
            }

            public bool TryEnterMappingValue([NotNullWhen(true)] out ISchemaIterator? childIterator)
            {
                if (primary.TryEnterMappingValue(out childIterator))
                {
                    if (secondary.TryEnterMappingValue(out var secondaryChildIterator))
                    {
                        childIterator = new CompositeIterator(childIterator, secondaryChildIterator);
                        return true;
                    }
                    return true;
                }
                else
                {
                    return secondary.TryEnterMappingValue(out childIterator);
                }
            }

            public bool IsTagImplicit(IScalar scalar, out ScalarStyle style)
            {
                return primary.IsTagImplicit(scalar, out style)
                    || secondary.IsTagImplicit(scalar, out style);
            }

            public bool IsTagImplicit(ISequence sequence, out SequenceStyle style)
            {
                return primary.IsTagImplicit(sequence, out style)
                    || secondary.IsTagImplicit(sequence, out style);
            }

            public bool IsTagImplicit(IMapping mapping, out MappingStyle style)
            {
                return primary.IsTagImplicit(mapping, out style)
                    || secondary.IsTagImplicit(mapping, out style);
            }
        }
    }
}
