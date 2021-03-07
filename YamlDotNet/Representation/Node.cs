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
using YamlDotNet.Serialization;
using HashCode = YamlDotNet.Core.HashCode;

namespace YamlDotNet.Representation
{
    public abstract class Node : INode, IEquatable<Node>
    {
        // Prevent extending this class from outside
        internal Node(INodeMapper mapper, Mark start, Mark end)
        {
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            Start = start;
            End = end;
        }

        public abstract NodeKind Kind { get; }
        public TagName Tag => Mapper.Tag;
        public INodeMapper Mapper { get; }
        public Mark Start { get; }
        public Mark End { get; }

        public abstract IEnumerable<Node> Children { get; }

        public TNode Expect<TNode>() where TNode : Node
        {
            return this is TNode specific
                ? specific
                : throw new UnexpectedNodeTypeException(this, typeof(TNode));
        }

        public virtual bool Equals([AllowNull] Node other)
        {
            // Start and End are not compared because they are not relevant to this node's identity.
            // The mapper already contains the tag, so there's no reason to compare it again.
            return other != null
                && this.Kind.Equals(other.Kind)
                && this.Mapper.Equals(other.Mapper);
        }

        public override bool Equals(object? obj) => Equals(obj as Node);

        public override int GetHashCode()
        {
            return HashCode.CombineHashCodes(Kind, Mapper);
        }
    }
}