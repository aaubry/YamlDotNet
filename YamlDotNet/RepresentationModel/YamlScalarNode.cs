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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Schemas;
using static YamlDotNet.Core.HashCode;

namespace YamlDotNet.RepresentationModel
{
    /// <summary>
    /// Represents a scalar node in the YAML document.
    /// </summary>
    [DebuggerDisplay("{Value}")]
    public sealed class YamlScalarNode : YamlNode, IYamlConvertible
    {
        private bool forceImplicitPlain;
        private string? value;

        /// <summary>
        /// Gets or sets the value of the node.
        /// </summary>
        /// <value>The value.</value>
        public string? Value
        {
            get => value;
            set
            {
                if (value == null)
                {
                    forceImplicitPlain = true;
                }
                else
                {
                    forceImplicitPlain = false;
                }

                this.value = value;
            }
        }

        /// <summary>
        /// Gets or sets the style of the node.
        /// </summary>
        /// <value>The style.</value>
        public ScalarStyle Style { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlScalarNode"/> class.
        /// </summary>
        internal YamlScalarNode(IParser parser, DocumentLoadingState state)
        {
            Load(parser, state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Load(IParser parser, DocumentLoadingState state)
        {
            var scalar = parser.Consume<Scalar>();

            Load(scalar, state);

            var value = scalar.Value;
            if (scalar.Style == ScalarStyle.Plain && Tag.IsEmpty)
            {
                forceImplicitPlain = value.Length switch
                {
                    // we have an implicit null value without a tag stating it, fake it out
                    0 => true,
                    1 => value == "~",
                    4 => value == "null" || value == "Null" || value == "NULL",
                    // for backwards compatability we won't be setting the Value property to null
                    _ => false
                };
            }

            this.value = value;
            Style = scalar.Style;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlScalarNode"/> class.
        /// </summary>
        public YamlScalarNode()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlScalarNode"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public YamlScalarNode(string? value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Resolves the aliases that could not be resolved when the node was created.
        /// </summary>
        /// <param name="state">The state of the document.</param>
        internal override void ResolveAliases(DocumentLoadingState state)
        {
            throw new NotSupportedException("Resolving an alias on a scalar node does not make sense");
        }

        /// <summary>
        /// Saves the current node to the specified emitter.
        /// </summary>
        /// <param name="emitter">The emitter where the node is to be saved.</param>
        /// <param name="state">The state.</param>
        internal override void Emit(IEmitter emitter, EmitterState state)
        {
            var tag = Tag;
            var implicitPlain = tag.IsEmpty;

            if (forceImplicitPlain &&
                Style == ScalarStyle.Plain &&
                (Value == null || Value == ""))
            {
                tag = JsonSchema.Tags.Null;
                implicitPlain = true;
            }
            else if (tag.IsEmpty && Value == null &&
                (Style == ScalarStyle.Plain || Style == ScalarStyle.Any))
            {
                tag = JsonSchema.Tags.Null;
                implicitPlain = true;
            }

            emitter.Emit(new Scalar(Anchor, tag, Value ?? string.Empty, Style, implicitPlain, false));
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
            return obj is YamlScalarNode other
                && Equals(Tag, other.Tag)
                && Equals(Value, other.Value);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return CombineHashCodes(Tag.GetHashCode(), Value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="YamlScalarNode"/> to <see cref="string"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator string?(YamlScalarNode value)
        {
            return value.Value;
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        internal override string ToString(RecursionLevel level)
        {
            return Value ?? string.Empty;
        }

        /// <summary>
        /// Recursively enumerates all the nodes from the document, starting on the current node,
        /// and throwing <see cref="MaximumRecursionLevelReachedException"/>
        /// if <see cref="RecursionLevel.Maximum"/> is reached.
        /// </summary>
        internal override IEnumerable<YamlNode> SafeAllNodes(RecursionLevel level)
        {
            yield return this;
        }

        /// <summary>
        /// Gets the type of node.
        /// </summary>
        public override YamlNodeType NodeType
        {
            get { return YamlNodeType.Scalar; }
        }

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
