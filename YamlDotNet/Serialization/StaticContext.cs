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
using YamlDotNet.Serialization.ObjectFactories;

namespace YamlDotNet.Serialization
{
    /// <summary>
    /// Holds the static object factory and type inspector to use when statically serializing/deserializing YAML.
    /// </summary>
    public abstract class StaticContext
    {
        /// <summary>
        /// Gets whether the type is known to the context
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns></returns>
        public virtual bool IsKnownType(Type type)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the <see cref="ITypeResolver"/> to use for serialization
        /// </summary>
        /// <returns></returns>
        public virtual ITypeResolver GetTypeResolver()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the factory to use for serialization and deserialization
        /// </summary>
        /// <returns></returns>
        public virtual StaticObjectFactory GetFactory()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the type inspector to use when statically serializing/deserializing YAML.
        /// </summary>
        /// <returns></returns>
        public virtual ITypeInspector GetTypeInspector()
        {
            throw new NotImplementedException();
        }
    }
}
