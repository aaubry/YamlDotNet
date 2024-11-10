﻿// This file is part of YamlDotNet - A .NET library for YAML.
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
using System.Diagnostics;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Core.ObjectPool;
using YamlDotNet.Serialization;
using static YamlDotNet.Core.HashCode;

namespace YamlDotNet.RepresentationModel
{
    /// <summary>
    /// Represents a sequence node in the YAML document.
    /// </summary>
    [DebuggerDisplay("Count = {children.Count}")]
    public sealed class YamlSequenceNode : YamlNode, IEnumerable<YamlNode>, IYamlConvertible
    {
        private readonly List<YamlNode> children = new ();

        /// <summary>
        /// Gets the collection of child nodes.
        /// </summary>
        /// <value>The children.</value>
        public IList<YamlNode> Children
        {
            get
            {
                return children;
            }
        }

        /// <summary>
        /// Gets or sets the style of the node.
        /// </summary>
        /// <value>The style.</value>
        public SequenceStyle Style { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="YamlSequenceNode"/> class.
        /// </summary>
        internal YamlSequenceNode(IParser parser, DocumentLoadingState state)
        {
            Load(parser, state);
        }

        private void Load(IParser parser, DocumentLoadingState state)
        {
            var sequence = parser.Consume<SequenceStart>();
            Load(sequence, state);
            Style = sequence.Style;

            var hasUnresolvedAliases = false;
            while (!parser.TryConsume<SequenceEnd>(out var _))
            {
                var child = ParseNode(parser, state);
                children.Add(child);
                hasUnresolvedAliases |= child is YamlAliasNode;
            }

            if (hasUnresolvedAliases)
            {
                state.AddNodeWithUnresolvedAliases(this);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlSequenceNode"/> class.
        /// </summary>
        public YamlSequenceNode()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlSequenceNode"/> class.
        /// </summary>
        public YamlSequenceNode(params YamlNode[] children)
            : this((IEnumerable<YamlNode>)children)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlSequenceNode"/> class.
        /// </summary>
        public YamlSequenceNode(IEnumerable<YamlNode> children)
        {
            foreach (var child in children)
            {
                this.children.Add(child);
            }
        }

        /// <summary>
        /// Adds the specified child to the <see cref="Children"/> collection.
        /// </summary>
        /// <param name="child">The child.</param>
        public void Add(YamlNode child)
        {
            children.Add(child);
        }

        /// <summary>
        /// Adds a scalar node to the <see cref="Children"/> collection.
        /// </summary>
        /// <param name="child">The child.</param>
        public void Add(string child)
        {
            children.Add(new YamlScalarNode(child));
        }

        /// <summary>
        /// Resolves the aliases that could not be resolved when the node was created.
        /// </summary>
        /// <param name="state">The state of the document.</param>
        internal override void ResolveAliases(DocumentLoadingState state)
        {
            for (var i = 0; i < children.Count; ++i)
            {
                if (children[i] is YamlAliasNode)
                {
                    children[i] = state.GetNode(children[i].Anchor!, children[i].Start, children[i].End);
                }
            }
        }

        /// <summary>
        /// Saves the current node to the specified emitter.
        /// </summary>
        /// <param name="emitter">The emitter where the node is to be saved.</param>
        /// <param name="state">The state.</param>
        internal override void Emit(IEmitter emitter, EmitterState state)
        {
            emitter.Emit(new SequenceStart(Anchor, Tag, Tag.IsEmpty, Style));
            foreach (var node in children)
            {
                node.Save(emitter, state);
            }
            emitter.Emit(new SequenceEnd());
        }

        /// <summary>
        /// Accepts the specified visitor by calling the appropriate Visit method on it.
        /// </summary>
        /// <param name="visitor">
        /// A <see cref="IYamlVisitor"/>.
        /// </param>
        public override void Accept(IYamlVisitor visitor)
        {
            visitor.Visit(this);
        }

        /// <summary />
        public override bool Equals(object? obj)
        {
            var other = obj as YamlSequenceNode;
            var areEqual = other != null
                && Equals(Tag, other.Tag)
                && children.Count == other.children.Count;

            if (!areEqual)
            {
                return false;
            }

            for (var i = 0; i < children.Count; ++i)
            {
                if (!Equals(children[i], other!.children[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            var hashCode = 0;
            foreach (var item in children)
            {
                hashCode = CombineHashCodes(hashCode, item);
            }
            hashCode = CombineHashCodes(hashCode, Tag);
            return hashCode;
        }

        /// <summary>
        /// Recursively enumerates all the nodes from the document, starting on the current node,
        /// and throwing <see cref="MaximumRecursionLevelReachedException"/>
        /// if <see cref="RecursionLevel.Maximum"/> is reached.
        /// </summary>
        internal override IEnumerable<YamlNode> SafeAllNodes(RecursionLevel level)
        {
            level.Increment();
            yield return this;
            foreach (var child in children)
            {
                foreach (var node in child.SafeAllNodes(level))
                {
                    yield return node;
                }
            }
            level.Decrement();
        }

        /// <summary>
        /// Gets the type of node.
        /// </summary>
        public override YamlNodeType NodeType
        {
            get { return YamlNodeType.Sequence; }
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        internal override string ToString(RecursionLevel level)
        {
            if (!level.TryIncrement())
            {
                return MaximumRecursionLevelReachedToStringValue;
            }

            using var textBuilder = StringBuilderPool.Rent();
            var text = textBuilder.Builder;
            text.Append("[ ");

            foreach (var child in children)
            {
                if (text.Length > 2)
                {
                    text.Append(", ");
                }
                text.Append(child.ToString(level));
            }

            text.Append(" ]");

            level.Decrement();

            return text.ToString();
        }

        #region IEnumerable<YamlNode> Members

        /// <summary />
        public IEnumerator<YamlNode> GetEnumerator()
        {
            return Children.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        void IYamlConvertible.Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
        {
            Load(parser, new DocumentLoadingState());
        }

        void IYamlConvertible.Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
        {
            Emit(emitter, new EmitterState());
        }
    }
}
