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

namespace YamlDotNet.Serialization.ObjectFactories
{
    /// <summary>
    /// Gets information about and creates statically known, serializable, types.
    /// </summary>
    public abstract class StaticObjectFactory : IObjectFactory
    {
        /// <summary>
        /// Create an object of the specified type
        /// </summary>
        /// <param name="type">Type of object to create</param>
        /// <returns></returns>
        public abstract object Create(Type type);

        /// <summary>
        /// Gets whether the type is a dictionary or not
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns></returns>
        public abstract bool IsDictionary(Type type);

        /// <summary>
        /// Gets whether the type is a list
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns></returns>
        public abstract bool IsList(Type type);

        /// <summary>
        /// Gets the type of the key of a dictionary
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public abstract Type GetKeyType(Type type);

        /// <summary>
        /// Gets the type of the value part of a dictionary or list.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public abstract Type GetValueType(Type type);
    }
}
