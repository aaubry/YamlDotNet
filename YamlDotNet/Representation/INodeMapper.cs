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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YamlDotNet.Core;
using YamlDotNet.Helpers;
using YamlDotNet.Representation.Schemas;

namespace YamlDotNet.Representation
{
    public interface INodeMapper
    {
        /// <summary>
        /// The tag to which this mapper applies.
        /// </summary>
        TagName Tag { get; }

        /// <summary>
        /// Constructs a native representation of the value contained by the specified <see cref="Node" />.
        /// </summary>
        /// <param name="node">
        /// The <see cref="Node" /> to be converted. It's tag must be equal to
        /// <see cref="Tag" /> and it must be of the correct kind (scalar, sequence or mapping).
        /// </param>
        /// <returns>
        /// Returns a native representation of the node. The ative representation could be, for example,
        /// a <see cref="System.Int32" />, if the node (a scalar in this case) represents a number.
        /// </returns>
        object? Construct(Node node);

        /// <summary>
        /// Creates a <see cref="Node"/> that represents the specified native value.
        /// </summary>
        /// <param name="native">The value to be converted. It must be compatible with this mapper's <see cref="Tag"/>.</param>
        /// <param name="iterator">The <see cref="ISchemaIterator" /> that should be used to resolve tags.</param>
        /// <param name="state">An <see cref="IRepresentationState" /> that is used to store state during the representation of an object graph.</param>
        /// <returns>
        /// Returns a <see cref="Node"/> that represents the <paramref name="native"/> in YAML,
        /// according to this <see cref="Tag"/>'s rules.
        /// </returns>
        Node Represent(object? native, ISchemaIterator iterator, IRepresentationState state);

        /// <summary>
        /// Gets the canonical version of this <see cref="INodeMapper" />.
        /// </summary>
        /// <remarks>
        /// Some <see cref="INodeMapper" /> offer specializations that construct and represent a specific
        /// format, out of the various formats defined by the schema. In such cases, this property should
        /// return an <see cref="INodeMapper" /> that recognizes all formats on construction,
        /// and uses the canonical form as representation.
        /// </remarks>
        INodeMapper Canonical { get; }
    }

    public interface IRepresentationState
    {
        /// <summary>
        /// A <see cref="Core.RecursionLevel" /> instance that is used to limit the maximum amount of recursion.
        /// </summary>
        RecursionLevel RecursionLevel { get; }

        /// <summary>
        /// Gets a previously computed representation of the specified <paramref name="native"/> value.
        /// </summary>
        bool TryGetMemorizedRepresentation(object native, [NotNullWhen(true)] out Node? representation);

        /// <summary>
        /// Stores a computed representation of the specified <paramref name="native"/> value.
        /// </summary>
        void MemorizeRepresentation(object native, Node representation);
    }

    public static class NodeMapperExtensions
    {
        public static Node RepresentMemorized(this INodeMapper mapper, object? native, ISchemaIterator iterator, IRepresentationState state)
        {
            return !(native is null) && state.TryGetMemorizedRepresentation(native, out var node)
                ? node
                : mapper.Represent(native, iterator, state);
        }
    }

    public sealed class RepresentationState : IRepresentationState
    {
        public RecursionLevel RecursionLevel { get; } = new RecursionLevel(200); // TODO: Configure recursion limit somewhere

        private readonly Dictionary<object, Node> memorizedRepresentations = new Dictionary<object, Node>(ReferenceEqualityComparer<object>.Default);

        public void MemorizeRepresentation(object native, Node representation)
        {
            memorizedRepresentations.Add(native, representation);
        }

        public bool TryGetMemorizedRepresentation(object native, [NotNullWhen(true)] out Node? representation)
        {
            return memorizedRepresentations.TryGetValue(native, out representation);
        }
    }
}
