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
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization.Converters
{
    /// <summary>
    /// Base class for YAML type converters that handle a single scalar type.
    /// Provides common functionality for type acceptance, scalar consumption, and emission.
    /// </summary>
    /// <typeparam name="T">The type this converter handles.</typeparam>
    public abstract class ScalarConverterBase<T> : IYamlTypeConverter
    {
        /// <inheritdoc/>
        public bool Accepts(Type type)
        {
            return type == typeof(T) || Nullable.GetUnderlyingType(type) == typeof(T);
        }

        /// <inheritdoc/>
        public abstract object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer);

        /// <inheritdoc/>
        public abstract void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer);

        /// <summary>
        /// Consumes and returns the scalar value from the parser.
        /// </summary>
        protected static string ConsumeScalarValue(IParser parser)
        {
            return parser.Consume<Scalar>().Value;
        }

        /// <summary>
        /// Emits a scalar value with the specified style.
        /// </summary>
        protected static void EmitScalar(IEmitter emitter, string formatted, ScalarStyle style)
        {
            emitter.Emit(new Scalar(AnchorName.Empty, TagName.Empty, formatted, style, true, false));
        }
    }
}
