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
    public sealed class Mapping : Node, IMapping, IReadOnlyDictionary<Node, Node>
    {
        private readonly IReadOnlyDictionary<Node, Node> items;

        public Mapping(INodeMapper mapper, IReadOnlyDictionary<Node, Node> items)
            : this(mapper, items, Mark.Empty, Mark.Empty)
        {
        }

        public Mapping(INodeMapper mapper, IReadOnlyDictionary<Node, Node> items, Mark start, Mark end) : base(mapper, start, end)
        {
            this.items = items ?? throw new ArgumentNullException(nameof(items));
        }

        public override NodeKind Kind => NodeKind.Mapping;
        public override IEnumerable<Node> Children
        {
            get
            {
                foreach(var (key, value) in items)
                {
                    yield return key;
                    yield return value;
                }
            }
        }

        public Node this[string key]
        {
            get
            {
                return TryGetValue(key, out var node)
                    ? node
                    : throw new KeyNotFoundException($"Key '{key}' not found.");
            }
        }

        public int Count => this.items.Count;
        public Node this[Node key] => this.items[key];
        public IEnumerable<Node> Keys => this.items.Keys;
        public IEnumerable<Node> Values => this.items.Values;

        public bool ContainsKey(Node key)
        {
            return this.items.ContainsKey(key);
        }

        public bool TryGetValue(string key, [NotNullWhen(true)] out Node? value)
        {
            // TODO: This could be a performance bottleneck. Maybe use a dedicated dictionary for this ?

            foreach (var item in this)
            {
                if (item.Key is Scalar scalar && scalar.Value == key)
                {
                    value = item.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        public bool TryGetValue(Node key, [MaybeNullWhen(false)] out Node value)
        {
            return this.items.TryGetValue(key, out value!);
        }

        public IEnumerator<KeyValuePair<Node, Node>> GetEnumerator() => this.items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override bool Equals([AllowNull] Node other)
        {
            if (!base.Equals(other))
            {
                return false;
            }

            // 'other' is always a Mapping when base.Equals returns true
            using var thisEnumerator = this.GetEnumerator();
            using var otherEnumerator = ((Mapping)other).GetEnumerator();

            bool thisHasNext, otherHasNext;
            while ((thisHasNext = thisEnumerator.MoveNext()) & (otherHasNext = otherEnumerator.MoveNext()))
            {
                var areEqual = thisEnumerator.Current.Key.Equals(otherEnumerator.Current.Key)
                    && thisEnumerator.Current.Value.Equals(otherEnumerator.Current.Value);

                if (!areEqual)
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
                hashCode = HashCode.CombineHashCodes(hashCode, child.Key, child.Value);
            }

            return hashCode;
        }

        public override string ToString() => $"Mapping {Tag}";
    }
}