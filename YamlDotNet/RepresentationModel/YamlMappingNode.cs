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
using System.Globalization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Core.ObjectPool;
using YamlDotNet.Helpers;
using YamlDotNet.Serialization;
using static YamlDotNet.Core.HashCode;

namespace YamlDotNet.RepresentationModel
{
    /// <summary>
    /// Represents a mapping node in the YAML document.
    /// </summary>
    public sealed class YamlMappingNode : YamlNode, IEnumerable<KeyValuePair<YamlNode, YamlNode>>, IYamlConvertible
    {
        private readonly OrderedDictionary<YamlNode, YamlNode> children = new();

        /// <summary>
        /// Gets the children of the current node.
        /// </summary>
        /// <value>The children.</value>
        public IOrderedDictionary<YamlNode, YamlNode> Children
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
        public MappingStyle Style { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlMappingNode"/> class.
        /// </summary>
        internal YamlMappingNode(IParser parser, DocumentLoadingState state)
        {
            Load(parser, state);
        }

        private void Load(IParser parser, DocumentLoadingState state)
        {
            var mapping = parser.Consume<MappingStart>();
            Load(mapping, state);
            Style = mapping.Style;

            var hasUnresolvedAliases = false;
            while (!parser.TryConsume<MappingEnd>(out var _))
            {
                var key = ParseNode(parser, state);
                var value = ParseNode(parser, state);

                if (!children.TryAdd(key, value))
                {
                    throw new YamlException(key.Start, key.End, $"Duplicate key {key}");
                }

                hasUnresolvedAliases |= key is YamlAliasNode || value is YamlAliasNode;
            }

            if (hasUnresolvedAliases)
            {
                state.AddNodeWithUnresolvedAliases(this);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlMappingNode"/> class.
        /// </summary>
        public YamlMappingNode()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlMappingNode"/> class.
        /// </summary>
        public YamlMappingNode(params KeyValuePair<YamlNode, YamlNode>[] children)
            : this((IEnumerable<KeyValuePair<YamlNode, YamlNode>>)children)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlMappingNode"/> class.
        /// </summary>
        public YamlMappingNode(IEnumerable<KeyValuePair<YamlNode, YamlNode>> children)
        {
            foreach (var child in children)
            {
                this.children.Add(child);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlMappingNode"/> class.
        /// </summary>
        /// <param name="children">A sequence of <see cref="YamlNode"/> where even elements are keys and odd elements are values.</param>
        public YamlMappingNode(params YamlNode[] children)
            : this((IEnumerable<YamlNode>)children)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlMappingNode"/> class.
        /// </summary>
        /// <param name="children">A sequence of <see cref="YamlNode"/> where even elements are keys and odd elements are values.</param>
        public YamlMappingNode(IEnumerable<YamlNode> children)
        {
            using var enumerator = children.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var key = enumerator.Current;
                if (!enumerator.MoveNext())
                {
                    throw new ArgumentException("When constructing a mapping node with a sequence, the number of elements of the sequence must be even.");
                }

                Add(key, enumerator.Current);
            }
        }

        /// <summary>
        /// Adds the specified mapping to the <see cref="Children"/> collection.
        /// </summary>
        /// <param name="key">The key node.</param>
        /// <param name="value">The value node.</param>
        public void Add(YamlNode key, YamlNode value)
        {
            children.Add(key, value);
        }

        /// <summary>
        /// Adds the specified mapping to the <see cref="Children"/> collection.
        /// </summary>
        /// <param name="key">The key node.</param>
        /// <param name="value">The value node.</param>
        public void Add(string key, YamlNode value)
        {
            children.Add(new YamlScalarNode(key), value);
        }

        /// <summary>
        /// Adds the specified mapping to the <see cref="Children"/> collection.
        /// </summary>
        /// <param name="key">The key node.</param>
        /// <param name="value">The value node.</param>
        public void Add(YamlNode key, string value)
        {
            children.Add(key, new YamlScalarNode(value));
        }

        /// <summary>
        /// Adds the specified mapping to the <see cref="Children"/> collection.
        /// </summary>
        /// <param name="key">The key node.</param>
        /// <param name="value">The value node.</param>
        public void Add(string key, string value)
        {
            children.Add(new YamlScalarNode(key), new YamlScalarNode(value));
        }

        /// <summary>
        /// Resolves the aliases that could not be resolved when the node was created.
        /// </summary>
        /// <param name="state">The state of the document.</param>
        internal override void ResolveAliases(DocumentLoadingState state)
        {
            Dictionary<YamlNode, YamlNode>? keysToUpdate = null;
            Dictionary<YamlNode, YamlNode>? valuesToUpdate = null;
            foreach (var entry in children)
            {
                if (entry.Key is YamlAliasNode)
                {
                    keysToUpdate ??= new ();
                    // TODO: The representation model should be redesigned, because here the anchor could be null but that would be invalid YAML
                    keysToUpdate.Add(entry.Key, state.GetNode(entry.Key.Anchor!, entry.Key.Start, entry.Key.End));
                }
                if (entry.Value is YamlAliasNode)
                {
                    valuesToUpdate ??= new ();
                    // TODO: The representation model should be redesigned, because here the anchor could be null but that would be invalid YAML
                    valuesToUpdate.Add(entry.Key, state.GetNode(entry.Value.Anchor!, entry.Value.Start, entry.Value.End));
                }
            }
            if (valuesToUpdate != null)
            {
                foreach (var entry in valuesToUpdate)
                {
                    children[entry.Key] = entry.Value;
                }
            }
            if (keysToUpdate != null)
            {
                foreach (var entry in keysToUpdate)
                {
                    var value = children[entry.Key];
                    children.Remove(entry.Key);
                    children.Add(entry.Value, value);
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
            emitter.Emit(new MappingStart(Anchor, Tag, true, Style));
            foreach (var entry in children)
            {
                entry.Key.Save(emitter, state);
                entry.Value.Save(emitter, state);
            }
            emitter.Emit(new MappingEnd());
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
            var other = obj as YamlMappingNode;
            var areEqual = other != null
                && Equals(Tag, other.Tag)
                && children.Count == other.children.Count;

            if (!areEqual)
            {
                return false;
            }

            foreach (var entry in children)
            {
                if (!other!.children.TryGetValue(entry.Key, out var otherNode) || !Equals(entry.Value, otherNode))
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
            var hashCode = base.GetHashCode();

            foreach (var entry in children)
            {
                hashCode = CombineHashCodes(hashCode, entry.Key);
                hashCode = entry.Value.Anchor.IsEmpty
                    ? CombineHashCodes(hashCode, entry.Value)
                    : CombineHashCodes(hashCode, entry.Value.Anchor);
            }
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
                foreach (var node in child.Key.SafeAllNodes(level))
                {
                    yield return node;
                }
                foreach (var node in child.Value.SafeAllNodes(level))
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
            get { return YamlNodeType.Mapping; }
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
            text.Append("{ ");

            foreach (var child in children)
            {
                if (text.Length > 2)
                {
                    text.Append(", ");
                }
                text.Append("{ ").Append(child.Key.ToString(level)).Append(", ").Append(child.Value.ToString(level)).Append(" }");
            }

            text.Append(" }");

            level.Decrement();

            return text.ToString();
        }

        #region IEnumerable<KeyValuePair<YamlNode,YamlNode>> Members

        /// <summary />
        public IEnumerator<KeyValuePair<YamlNode, YamlNode>> GetEnumerator()
        {
            return children.GetEnumerator();
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

        /// <summary>
        /// Creates a <see cref="YamlMappingNode" /> containing a key-value pair for each property of the specified object.
        /// </summary>
        public static YamlMappingNode FromObject(object mapping)
        {
            if (mapping == null)
            {
                throw new ArgumentNullException(nameof(mapping));
            }

            var result = new YamlMappingNode();
            foreach (var property in mapping.GetType().GetPublicProperties())
            {
                // CanRead == true => GetGetMethod() != null
                if (property.CanRead && property.GetGetMethod(false)!.GetParameters().Length == 0)
                {
                    var value = property.GetValue(mapping, null);
                    if (!(value is YamlNode valueNode))
                    {
                        var valueAsString = Convert.ToString(value, CultureInfo.InvariantCulture);
                        valueNode = valueAsString ?? string.Empty;
                    }
                    result.Add(property.Name, valueNode);
                }
            }
            return result;
        }
    }
}
