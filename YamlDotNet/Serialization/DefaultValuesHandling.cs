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
    /// Specifies the strategy to handle default and null values during serialization of properties.
    /// </summary>
    [Flags]
    public enum DefaultValuesHandling
    {
        /// <summary>
        /// Specifies that all properties are to be emitted regardless of their value. This is the default behavior.
        /// </summary>
        Preserve = 0,

        /// <summary>
        /// Specifies that properties that contain null references or a null Nullable&lt;T&gt; are to be omitted.
        /// </summary>
        OmitNull = 1,

        /// <summary>
        /// Specifies that properties that that contain their default value, either default(T) or the value specified in DefaultValueAttribute are to be omitted.
        /// </summary>
        OmitDefaults = 2,

        /// <summary>
        /// Specifies that properties that that contain collections/arrays/enumerations that are empty are to be omitted.
        /// </summary>
        OmitEmptyCollections = 4,
    }
}
