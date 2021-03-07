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
    /// <summary>
    /// Maps a constant <see cref="String" /> to a different constant, for representation.
    /// </summary>
    public sealed class TranslateStringMapper : INodeMapper
    {
        private readonly string nativeValue;
        private readonly Scalar representation;

        public TranslateStringMapper(string representationValue, string nativeValue)
        {
            if (representationValue is null)
            {
                throw new ArgumentNullException(nameof(representationValue));
            }

            this.nativeValue = nativeValue ?? throw new ArgumentNullException(nameof(nativeValue));
            representation = new Scalar(this, representationValue);
        }

        public TagName Tag => YamlTagRepository.String;
        public INodeMapper Canonical => this;

        public object? Construct(Node node)
        {
            var actualValue = node.Expect<Scalar>().Value;
            if (!representation.Value.Equals(actualValue))
            {
                throw new YamlException(node.Start, node.End, $"Expected a scalar with a value of '{representation.Value}', instead the value was '{actualValue}'.");
            }
            return nativeValue;
        }

        public Node Represent(object? native, ISchemaIterator iterator, IRepresentationState state)
        {
            if (!nativeValue.Equals(native))
            {
                throw new YamlException($"Expected a string with a value of '{nativeValue}', instead the value was '{native}'.");
            }
            return representation;
        }

        public override string ToString()
        {
            return $"'{representation.Value}' <-> '{nativeValue}'";
        }
    }
}
