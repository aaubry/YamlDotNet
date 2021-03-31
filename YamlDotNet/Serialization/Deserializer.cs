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
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization
{
    /// <summary>
    /// Deserializes objects from the YAML format.
    /// To customize the behavior of <see cref="Deserializer" />,
    /// use the <see cref="DeserializerBuilder" /> class.
    /// </summary>
    public sealed class Deserializer : IDeserializer
    {
        private readonly IValueDeserializer valueDeserializer;

        /// <summary>
        /// Initializes a new instance of <see cref="Deserializer" /> using the default configuration.
        /// </summary>
        /// <remarks>
        /// To customize the behavior of the deserializer, use <see cref="DeserializerBuilder" />.
        /// </remarks>
        public Deserializer()
            : this(new DeserializerBuilder().BuildValueDeserializer())
        {
        }

        /// <remarks>
        /// This constructor is private to discourage its use.
        /// To invoke it, call the <see cref="FromValueDeserializer"/> method.
        /// </remarks>
        private Deserializer(IValueDeserializer valueDeserializer)
        {
            this.valueDeserializer = valueDeserializer ?? throw new ArgumentNullException(nameof(valueDeserializer));
        }

        /// <summary>
        /// Creates a new <see cref="Deserializer" /> that uses the specified <see cref="IValueDeserializer" />.
        /// This method is available for advanced scenarios. The preferred way to customize the behavior of the
        /// deserializer is to use <see cref="DeserializerBuilder" />.
        /// </summary>
        public static Deserializer FromValueDeserializer(IValueDeserializer valueDeserializer)
        {
            return new Deserializer(valueDeserializer);
        }

        public T Deserialize<T>(string input)
        {
            using var reader = new StringReader(input);
            return Deserialize<T>(reader);
        }

        public T Deserialize<T>(TextReader input)
        {
            return Deserialize<T>(new Parser(input));
        }

        public object? Deserialize(TextReader input)
        {
            return Deserialize(input, typeof(object));
        }

        public object? Deserialize(string input, Type type)
        {
            using var reader = new StringReader(input);
            return Deserialize(reader, type);
        }

        public object? Deserialize(TextReader input, Type type)
        {
            return Deserialize(new Parser(input), type);
        }

        public T Deserialize<T>(IParser parser)
        {
            return (T)Deserialize(parser, typeof(T))!; // We really want an exception if we are trying to deserialize null into a non-nullable type
        }

        public object? Deserialize(IParser parser)
        {
            return Deserialize(parser, typeof(object));
        }

        /// <summary>
        /// Deserializes an object of the specified type.
        /// </summary>
        /// <param name="parser">The <see cref="IParser" /> from where to deserialize the object.</param>
        /// <param name="type">The static type of the object to deserialize.</param>
        /// <returns>Returns the deserialized object.</returns>
        public object? Deserialize(IParser parser, Type type)
        {
            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var hasStreamStart = parser.TryConsume<StreamStart>(out var _);

            var hasDocumentStart = parser.TryConsume<DocumentStart>(out var _);

            object? result = null;
            if (!parser.Accept<DocumentEnd>(out var _) && !parser.Accept<StreamEnd>(out var _))
            {
                using var state = new SerializerState();
                result = valueDeserializer.DeserializeValue(parser, type, state, valueDeserializer);
                state.OnDeserialization();
            }

            if (hasDocumentStart)
            {
                parser.Consume<DocumentEnd>();
            }

            if (hasStreamStart)
            {
                parser.Consume<StreamEnd>();
            }

            return result;
        }
    }
}
