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

namespace YamlDotNet.Serialization
{
    /// <summary>
    /// Allows an object to customize how it is serialized and deserialized.
    /// </summary>
    public interface IYamlConvertible
    {
        /// <summary>
        /// Reads this object's state from a YAML parser.
        /// </summary>
        /// <param name="parser">The parser where the object's state should be read from.</param>
        /// <param name="expectedType">The type that the deserializer is expecting.</param>
        /// <param name="nestedObjectDeserializer">
        /// A function that will use the current deserializer
        /// to read an object of the given type from the parser.
        /// </param>
        void Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer);

        /// <summary>
        /// Writes this object's state to a YAML emitter.
        /// </summary>
        /// <param name="emitter">The emitter where the object's state should be written to.</param>
        /// <param name="nestedObjectSerializer">A function that will use the current serializer to write an object to the emitter.</param>
        void Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer);
    }

    /// <summary>
    /// Represents a function that is used to deserialize an object of the given type.
    /// </summary>
    /// <param name="type">The type that the deserializer should read.</param>
    /// <returns>Returns the object that was deserialized.</returns>
    public delegate object? ObjectDeserializer(Type type);

    /// <summary>
    /// Represents a function that is used to serialize an object of the given type.
    /// </summary>
    /// <param name="value">The object to be serialized.</param>
    /// <param name="type">
    /// The type that should be considered when emitting the object.
    /// If null, the actual type of the <paramref name="value" /> is used.
    /// </param>
    public delegate void ObjectSerializer(object? value, Type? type = null);
}
