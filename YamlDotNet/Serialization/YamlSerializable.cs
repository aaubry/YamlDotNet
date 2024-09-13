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

namespace YamlDotNet.Serialization
{
    /// <summary>
    /// Put this attribute either on serializable types or on the <see cref="StaticContext"/> that you want
    /// the static analyzer to detect and use.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum, Inherited = false, AllowMultiple = true)]
    public sealed class YamlSerializableAttribute : Attribute
    {
        /// <summary>
        /// Use this constructor if the attribute is placed on a serializable class.
        /// </summary>
        public YamlSerializableAttribute()
        {
        }

        /// <summary>
        /// Use this constructor if the attribute is placed on the <see cref="StaticContext"/>.
        /// </summary>
        /// <param name="serializableType">The type for which to include static code generation.</param>
#pragma warning disable IDE0055 // Bug in Linux where IDE0055 is failing on the pragma warning disable IDE0060
#pragma warning disable IDE0060
        public YamlSerializableAttribute(Type serializableType)
#pragma warning restore IDE0060
#pragma warning restore IDE0055
        {
        }
    }
}
