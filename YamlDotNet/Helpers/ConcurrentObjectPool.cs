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


// Adapter from Microsoft code which is
// Copyright (c) Microsoft.
// All Rights Reserved.  Licensed under the Apache License, Version 2.0.

using System;
using System.Diagnostics;
using System.Threading;

namespace YamlDotNet.Helpers
{
    /// <summary>
    /// Generic implementation of object pooling pattern with predefined pool size limit. The main
    /// purpose is that limited number of frequently used objects can be kept in the pool for
    /// further recycling.
    ///
    /// Notes:
    /// 1) it is not the goal to keep all returned objects. Pool is not meant for storage. If there
    ///    is no space in the pool, extra returned objects will be dropped.
    ///
    /// 2) it is implied that if object was obtained from a pool, the caller will return it back in
    ///    a relatively short time. Keeping checked out objects for long durations is ok, but
    ///    reduces usefulness of pooling. Just new up your own.
    ///
    /// Not returning objects to the pool in not detrimental to the pool's work, but is a bad practice.
    /// Rationale:
    ///    If there is no intent for reusing the object, do not use pool - just use "new".
    /// </summary>
    [DebuggerStepThrough]
    internal sealed class ConcurrentObjectPool<T> where T : class
    {
        [DebuggerDisplay("{value,nq}")]
        private struct Element
        {
            internal T? value;
        }

        /// <remarks>
        /// Not using System.Func{T} because this file is linked into the (debugger) Formatter,
        /// which does not have that type (since it compiles against .NET 2.0).
        /// </remarks>
        internal delegate T Factory();

        // Storage for the pool objects. The first item is stored in a dedicated field because we
        // expect to be able to satisfy most requests from it.
        private T? firstItem;
        private readonly Element[] items;

        // factory is stored for the lifetime of the pool. We will call this only when pool needs to
        // expand. compared to "new T()", Func gives more flexibility to implementers and faster
        // than "new T()".
        private readonly Factory factory;

        internal ConcurrentObjectPool(Factory factory)
            : this(factory, Environment.ProcessorCount * 2)
        {
        }

        internal ConcurrentObjectPool(Factory factory, int size)
        {
            Debug.Assert(size >= 1);
            this.factory = factory;
            items = new Element[size - 1];
        }

        private T CreateInstance()
        {
            var inst = factory();
            return inst;
        }

        /// <summary>
        /// Produces an instance.
        /// </summary>
        /// <remarks>
        /// Search strategy is a simple linear probing which is chosen for it cache-friendliness.
        /// Note that Free will try to store recycled objects close to the start thus statistically
        /// reducing how far we will typically search.
        /// </remarks>
        internal T Allocate()
        {
            // PERF: Examine the first element. If that fails, AllocateSlow will look at the remaining elements.
            // Note that the initial read is optimistically not synchronized. That is intentional.
            // We will interlock only when we have a candidate. in a worst case we may miss some
            // recently returned objects. Not a big deal.
            var inst = firstItem;
            if (inst == null || inst != Interlocked.CompareExchange(ref firstItem, null, inst))
            {
                inst = AllocateSlow();
            }

            return inst;
        }

        private T AllocateSlow()
        {
            var elements = items;

            for (var i = 0; i < elements.Length; i++)
            {
                // Note that the initial read is optimistically not synchronized. That is intentional.
                // We will interlock only when we have a candidate. in a worst case we may miss some
                // recently returned objects. Not a big deal.
                var inst = elements[i].value;
                if (inst != null)
                {
                    if (inst == Interlocked.CompareExchange(ref elements[i].value, null, inst))
                    {
                        return inst;
                    }
                }
            }

            return CreateInstance();
        }

        /// <summary>
        /// Returns objects to the pool.
        /// </summary>
        /// <remarks>
        /// Search strategy is a simple linear probing which is chosen for it cache-friendliness.
        /// Note that Free will try to store recycled objects close to the start thus statistically
        /// reducing how far we will typically search in Allocate.
        /// </remarks>
        internal void Free(T obj)
        {
            Validate(obj);

            if (firstItem == null)
            {
                // Intentionally not using interlocked here.
                // In a worst case scenario two objects may be stored into same slot.
                // It is very unlikely to happen and will only mean that one of the objects will get collected.
                firstItem = obj;
            }
            else
            {
                FreeSlow(obj);
            }
        }

        private void FreeSlow(T obj)
        {
            var elements = items;
            for (var i = 0; i < elements.Length; i++)
            {
                if (elements[i].value == null)
                {
                    // Intentionally not using interlocked here.
                    // In a worst case scenario two objects may be stored into same slot.
                    // It is very unlikely to happen and will only mean that one of the objects will get collected.
                    elements[i].value = obj;
                    break;
                }
            }
        }

        [Conditional("DEBUG")]
        private void Validate(object obj)
        {
            Debug.Assert(obj != null, "freeing null?");

            Debug.Assert(firstItem != obj, "freeing twice?");

            var elements = items;
            for (var i = 0; i < elements.Length; i++)
            {
                var value = elements[i].value;
                if (value == null)
                {
                    return;
                }

                Debug.Assert(value != obj, "freeing twice?");
            }
        }
    }
}
