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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YamlDotNet.Core;
using HashCode = YamlDotNet.Core.HashCode;

namespace YamlDotNet.Representation
{
    public sealed class Sequence : Node, ISequence, IReadOnlyList<Node>
    {
        private readonly IReadOnlyList<Node> items;

        public Sequence(INodeMapper mapper, IReadOnlyList<Node> items)
            : this(mapper, items, Mark.Empty, Mark.Empty)
        {
        }

        public Sequence(INodeMapper mapper, IReadOnlyList<Node> items, Mark start, Mark end) : base(mapper, start, end)
        {
            this.items = items ?? throw new ArgumentNullException(nameof(items));
        }

        public override NodeKind Kind => NodeKind.Sequence;
        public override IEnumerable<Node> Children => items;

        public int Count => this.items.Count;
        public Node this[int index] => this.items[index];

        public IEnumerator<Node> GetEnumerator() => this.items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override bool Equals([AllowNull] Node other)
        {
            if (!base.Equals(other))
            {
                return false;
            }

            // 'other' is always a Sequence when base.Equals returns true
            using var thisEnumerator = this.GetEnumerator();
            using var otherEnumerator = ((Sequence)other).GetEnumerator();

            bool thisHasNext, otherHasNext;
            while ((thisHasNext = thisEnumerator.MoveNext()) & (otherHasNext = otherEnumerator.MoveNext()))
            {
                if (!thisEnumerator.Current.Equals(otherEnumerator.Current))
                {
                    return false;
                }
            }

            return !thisHasNext && !otherHasNext;
        }

        public override int GetHashCode()
        {
            var hashCode = base.GetHashCode();
            foreach (var child in this)
            {
                hashCode = HashCode.CombineHashCodes(hashCode, child);
            }

            return hashCode;
        }

        public override string ToString() => $"Sequence {Tag}";
    }
}