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

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace YamlDotNet.Core.ObjectPool
{
    /// <summary>
    /// Default implementation of <see cref="ObjectPool{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type to pool objects for.</typeparam>
    /// <remarks>This implementation keeps a cache of retained objects. This means that if objects are returned when the pool has already reached "maximumRetained" objects they will be available to be Garbage Collected.</remarks>
    internal class DefaultObjectPool<T> : ObjectPool<T> where T : class
    {
        private readonly Func<T> createFunc;
        private readonly Func<T, bool> returnFunc;
        private readonly int maxCapacity;
        private int numItems;

        private protected readonly ConcurrentQueue<T> items = new ConcurrentQueue<T>();
        private protected T? fastItem;

        /// <summary>
        /// Creates an instance of <see cref="DefaultObjectPool{T}"/>.
        /// </summary>
        /// <param name="policy">The pooling policy to use.</param>
        public DefaultObjectPool(IPooledObjectPolicy<T> policy)
            : this(policy, Environment.ProcessorCount * 2)
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="DefaultObjectPool{T}"/>.
        /// </summary>
        /// <param name="policy">The pooling policy to use.</param>
        /// <param name="maximumRetained">The maximum number of objects to retain in the pool.</param>
        public DefaultObjectPool(IPooledObjectPolicy<T> policy, int maximumRetained)
        {
            // cache the target interface methods, to avoid interface lookup overhead
            createFunc = policy.Create;
            returnFunc = policy.Return;
            maxCapacity = maximumRetained - 1;  // -1 to account for fastItem
        }

        /// <inheritdoc />
        public override T Get()
        {
            var item = fastItem;
            if (item == null || Interlocked.CompareExchange(ref fastItem, null, item) != item)
            {
                if (items.TryDequeue(out item))
                {
                    Interlocked.Decrement(ref numItems);
                    return item;
                }

                // no object available, so go get a brand new one
                return createFunc();
            }

            return item;
        }

        /// <inheritdoc />
        public override void Return(T obj)
        {
            ReturnCore(obj);
        }

        /// <summary>
        /// Returns an object to the pool.
        /// </summary>
        /// <returns>true if the object was returned to the pool</returns>
        private protected bool ReturnCore(T obj)
        {
            if (!returnFunc(obj))
            {
                // policy says to drop this object
                return false;
            }

            if (fastItem != null || Interlocked.CompareExchange(ref fastItem, obj, null) != null)
            {
                if (Interlocked.Increment(ref numItems) <= maxCapacity)
                {
                    items.Enqueue(obj);
                    return true;
                }

                // no room, clean up the count and drop the object on the floor
                Interlocked.Decrement(ref numItems);
                return false;
            }

            return true;
        }
    }
}
