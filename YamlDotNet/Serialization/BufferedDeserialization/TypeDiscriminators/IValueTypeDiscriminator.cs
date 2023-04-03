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
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.BufferedDeserialization.TypeDiscriminators
{
    /// <summary>
    /// An ITypeDiscriminator provides an interface for discriminating which dotnet type to deserialize a yaml
    /// stream into. They require the yaml stream to be buffered <see cref="TypeDiscriminatingNodeDeserializer" /> as they
    /// can inspect the yaml value, determine the desired type, and reset the yaml stream to then deserialize into
    /// that type.
    /// </summary>
    public interface ITypeDiscriminator
    {
        /// <summary>
        /// Gets the BaseType of the discriminator. All types that an ITypeDiscriminator may discriminate into must
        /// inherit from this type. This enables the deserializer to only buffer values of matching types.
        /// If you would like an ITypeDiscriminator to discriminate all yaml values, the BaseType will be object.
        /// </summary>
        Type BaseType { get; }

        /// <summary>
        /// Trys to discriminate a type from the current IParser. As discriminating the type will consume the parser, the
        /// parser will usually need to be a buffer so an instance of the discriminated type can be deserialized later.
        /// </summary>
        /// <param name="buffer">The IParser to consume and discriminate a type from.</param>
        /// <param name="suggestedType">The output type discriminated. Null if no type matched the discriminator.</param>
        /// <returns>Returns true if the discriminator matched the yaml stream.</returns>
        bool TryDiscriminate(IParser buffer, out Type? suggestedType);
    }
}