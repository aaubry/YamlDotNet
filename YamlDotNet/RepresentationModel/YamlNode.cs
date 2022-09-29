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
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.RepresentationModel
{
    /// <summary>
    /// Represents a single node in the YAML document.
    /// </summary>
    public abstract class YamlNode
    {
        private const int MaximumRecursionLevel = 1000;
        internal const string MaximumRecursionLevelReachedToStringValue = "WARNING! INFINITE RECURSION!";

        /// <summary>
        /// Gets or sets the anchor of the node.
        /// </summary>
        /// <value>The anchor.</value>
        public AnchorName Anchor { get; set; }

        /// <summary>
        /// Gets or sets the tag of the node.
        /// </summary>
        /// <value>The tag.</value>
        public TagName Tag { get; set; }

        /// <summary>
        /// Gets the position in the input stream where the event that originated the node starts.
        /// </summary>
        public Mark Start { get; private set; } = Mark.Empty;

        /// <summary>
        /// Gets the position in the input stream where the event that originated the node ends.
        /// </summary>
        public Mark End { get; private set; } = Mark.Empty;

        /// <summary>
        /// Loads the specified event.
        /// </summary>
        /// <param name="yamlEvent">The event.</param>
        /// <param name="state">The state of the document.</param>
        internal void Load(NodeEvent yamlEvent, DocumentLoadingState state)
        {
            Tag = yamlEvent.Tag;
            if (!yamlEvent.Anchor.IsEmpty)
            {
                Anchor = yamlEvent.Anchor;
                state.AddAnchor(this);
            }
            Start = yamlEvent.Start;
            End = yamlEvent.End;
        }

        /// <summary>
        /// Parses the node represented by the next event in <paramref name="parser" />.
        /// </summary>
        /// <returns>Returns the node that has been parsed.</returns>
        internal static YamlNode ParseNode(IParser parser, DocumentLoadingState state)
        {
            if (parser.Accept<Scalar>(out var _))
            {
                return new YamlScalarNode(parser, state);
            }

            if (parser.Accept<SequenceStart>(out var _))
            {
                return new YamlSequenceNode(parser, state);
            }

            if (parser.Accept<MappingStart>(out var _))
            {
                return new YamlMappingNode(parser, state);
            }

            if (parser.TryConsume<AnchorAlias>(out var alias))
            {
                return state.TryGetNode(alias.Value, out var node) ? node : new YamlAliasNode(alias.Value);
            }

            throw new ArgumentException("The current event is of an unsupported type.", nameof(parser));
        }

        /// <summary>
        /// Resolves the aliases that could not be resolved when the node was created.
        /// </summary>
        /// <param name="state">The state of the document.</param>
        internal abstract void ResolveAliases(DocumentLoadingState state);

        /// <summary>
        /// Saves the current node to the specified emitter.
        /// </summary>
        /// <param name="emitter">The emitter where the node is to be saved.</param>
        /// <param name="state">The state.</param>
        internal void Save(IEmitter emitter, EmitterState state)
        {
            if (!Anchor.IsEmpty && !state.EmittedAnchors.Add(Anchor))
            {
                emitter.Emit(new AnchorAlias(Anchor));
            }
            else
            {
                Emit(emitter, state);
            }
        }

        /// <summary>
        /// Saves the current node to the specified emitter.
        /// </summary>
        /// <param name="emitter">The emitter where the node is to be saved.</param>
        /// <param name="state">The state.</param>
        internal abstract void Emit(IEmitter emitter, EmitterState state);

        /// <summary>
        /// Accepts the specified visitor by calling the appropriate Visit method on it.
        /// </summary>
        /// <param name="visitor">
        /// A <see cref="IYamlVisitor"/>.
        /// </param>
        public abstract void Accept(IYamlVisitor visitor);

        public override string ToString()
        {
            var level = new RecursionLevel(MaximumRecursionLevel);
            return ToString(level);
        }

        internal abstract string ToString(RecursionLevel level);

        /// <summary>
        /// Gets all nodes from the document, starting on the current node.
        /// <see cref="MaximumRecursionLevelReachedException"/> is thrown if an infinite recursion is detected.
        /// </summary>
        public IEnumerable<YamlNode> AllNodes
        {
            get
            {
                var level = new RecursionLevel(MaximumRecursionLevel);
                return SafeAllNodes(level);
            }
        }

        /// <summary>
        /// When implemented, recursively enumerates all the nodes from the document, starting on the current node.
        /// If <see cref="RecursionLevel.Maximum"/> is reached, a <see cref="MaximumRecursionLevelReachedException"/> is thrown
        /// instead of continuing and crashing with a <see cref="StackOverflowException"/>.
        /// </summary>
        internal abstract IEnumerable<YamlNode> SafeAllNodes(RecursionLevel level);

        /// <summary>
        /// Gets the type of node.
        /// </summary>
        public abstract YamlNodeType NodeType
        {
            get;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="string"/> to <see cref="YamlNode"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator YamlNode(string value)
        {
            return new YamlScalarNode(value);
        }

        /// <summary>
        /// Performs an implicit conversion from string[] to <see cref="YamlNode"/>.
        /// </summary>
        /// <param name="sequence">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator YamlNode(string[] sequence)
        {
            return new YamlSequenceNode(sequence.Select(i => (YamlNode)i));
        }

        /// <summary>
        /// Converts a <see cref="YamlScalarNode" /> to a string by returning its value.
        /// </summary>
        public static explicit operator string?(YamlNode node)
        {
            return node is YamlScalarNode scalar
                ? scalar.Value
                : throw new ArgumentException($"Attempted to convert a '{node.NodeType}' to string. This conversion is valid only for Scalars.");
        }

        /// <summary>
        /// Gets the nth element in a <see cref="YamlSequenceNode" />.
        /// </summary>
        public YamlNode this[int index]
        {
            get
            {
                return this is YamlSequenceNode sequence
                    ? sequence.Children[index]
                    : throw new ArgumentException($"Accessed '{NodeType}' with an invalid index: {index}. Only Sequences can be indexed by number.");
            }
        }

        /// <summary>
        /// Gets the value associated with a key in a <see cref="YamlMappingNode" />.
        /// </summary>
        public YamlNode this[YamlNode key]
        {
            get
            {
                return this is YamlMappingNode mapping
                    ? mapping.Children[key]
                    : throw new ArgumentException($"Accessed '{NodeType}' with an invalid index: {key}. Only Mappings can be indexed by key.");
            }
        }
    }
}
