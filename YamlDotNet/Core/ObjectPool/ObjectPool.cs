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


// Adapted from https://github.com/dotnet/aspnetcore/blob/2f1db20456007c9515068a35a65afdf99af70bc6/src/ObjectPool/src/DefaultObjectPool.cs which is
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace YamlDotNet.Core.ObjectPool
{
    /// <summary>
    /// A pool of objects.
    /// </summary>
    /// <typeparam name="T">The type of objects to pool.</typeparam>
    internal abstract class ObjectPool<T> where T : class
    {
        /// <summary>
        /// Gets an object from the pool if one is available, otherwise creates one.
        /// </summary>
        /// <returns>A <typeparamref name="T"/>.</returns>
        public abstract T Get();

        /// <summary>
        /// Return an object to the pool.
        /// </summary>
        /// <param name="obj">The object to add to the pool.</param>
        public abstract void Return(T obj);
    }

    /// <summary>
    /// Methods for creating <see cref="ObjectPool{T}"/> instances.
    /// </summary>
    internal static class ObjectPool
    {
        /// <summary>
        /// Create a new <see cref="ObjectPool{T}"/> instance using the default implementation.
        /// </summary>
        /// <typeparam name="T">The type of object to be pooled</typeparam>
        /// <param name="policy">The <see cref="IPooledObjectPolicy{T}"/> to use (or <see cref="DefaultPooledObjectPolicy{T}"/> if <see langword="null"/>).</param>
        /// <returns>An instance of <see cref="DefaultObjectPool{T}"/> with the specified policy.</returns>
        public static ObjectPool<T> Create<T>(IPooledObjectPolicy<T>? policy = null) where T : class, new()
        {
            return new DefaultObjectPool<T>(policy ?? new DefaultPooledObjectPolicy<T>());
        }

        /// <inheritdoc cref="Create{T}(IPooledObjectPolicy{T}?)" />
        /// <param name="maximumRetained">The maximum number of objects to retain in the pool.</param>
        /// <param name="policy">The <see cref="IPooledObjectPolicy{T}"/> to use (or <see cref="DefaultPooledObjectPolicy{T}"/> if <see langword="null"/>).</param>
        public static ObjectPool<T> Create<T>(int maximumRetained, IPooledObjectPolicy<T>? policy = null) where T : class, new()
        {
            return new DefaultObjectPool<T>(policy ?? new DefaultPooledObjectPolicy<T>(), maximumRetained);
        }
    }
}
