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
    /// Represents an object along with its type.
    /// </summary>
    public interface IObjectDescriptor
    {
        /// <summary>
        /// A reference to the object.
        /// </summary>
        object? Value { get; }

        /// <summary>
        /// The type that should be used when to interpret the <see cref="Value" />.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// The type of <see cref="Value" /> as determined by its container (e.g. a property).
        /// </summary>
        Type StaticType { get; }

        /// <summary>
        /// The style to be used for scalars.
        /// </summary>
        ScalarStyle ScalarStyle { get; }
    }

    public static class ObjectDescriptorExtensions
    {
        /// <summary>
        /// Returns the Value property of the <paramref name="objectDescriptor"/> if it is not null.
        /// This is useful in all places that the value must not be null.
        /// </summary>
        /// <param name="objectDescriptor">An object descriptor.</param>
        /// <exception cref="InvalidOperationException">Thrown when the Value is null</exception>
        /// <returns></returns>
        public static object NonNullValue(this IObjectDescriptor objectDescriptor)
        {
            return objectDescriptor.Value ?? throw new InvalidOperationException($"Attempted to use a IObjectDescriptor of type '{objectDescriptor.Type.FullName}' whose Value is null at a point whete it is invalid to do so. This may indicate a bug in YamlDotNet.");
        }
    }
}
