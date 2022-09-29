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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using YamlDotNet.Helpers;

namespace YamlDotNet.Core
{
    /// <summary>
    /// Generic queue on which items may be inserted
    /// </summary>
    public sealed class InsertionQueue<T> : IEnumerable<T>
    {
        private const int DefaultInitialCapacity = 1 << 7; // Must be a power of 2

        // Circular buffer
        private T[] items;
        private int readPtr;
        private int writePtr;
        private int mask;
        private int count = 0;

        public InsertionQueue(int initialCapacity = DefaultInitialCapacity)
        {
            if (initialCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialCapacity), "The initial capacity must be a positive number.");
            }

            if (!initialCapacity.IsPowerOfTwo())
            {
                throw new ArgumentException("The initial capacity must be a power of 2.", nameof(initialCapacity));
            }

            items = new T[initialCapacity];
            readPtr = initialCapacity / 2;
            writePtr = initialCapacity / 2;
            mask = initialCapacity - 1;
        }

        /// <summary>
        /// Gets the number of items that are contained by the queue.
        /// </summary>
        public int Count => count;
        public int Capacity => items.Length;

        /// <summary>
        /// Enqueues the specified item.
        /// </summary>
        /// <param name="item">The item to be enqueued.</param>
        public void Enqueue(T item)
        {
            ResizeIfNeeded();

            items[writePtr] = item;
            writePtr = (writePtr - 1) & mask;
            ++count;
        }

        /// <summary>
        /// Dequeues an item.
        /// </summary>
        /// <returns>Returns the item that been dequeued.</returns>
        public T Dequeue()
        {
            if (count == 0)
            {
                throw new InvalidOperationException("The queue is empty");
            }

            var item = items[readPtr];
            readPtr = (readPtr - 1) & mask;
            --count;
            return item;
        }

        /// <summary>
        /// Inserts an item at the specified index.
        /// </summary>
        /// <param name="index">The index where to insert the item.</param>
        /// <param name="item">The item to be inserted.</param>
        public void Insert(int index, T item)
        {
            if (index > count)
            {
                throw new InvalidOperationException("Cannot insert outside of the bounds of the queue");
            }

            ResizeIfNeeded();

            CalculateInsertionParameters(
                mask, count, index,
                ref readPtr, ref writePtr,
                out var insertPtr,
                out var copyIndex, out var copyOffset, out var copyLength
            );

            if (copyLength != 0)
            {
                Array.Copy(items, copyIndex, items, copyIndex + copyOffset, copyLength);
            }

            items[insertPtr] = item;
            ++count;
        }

        private void ResizeIfNeeded()
        {
            var capacity = items.Length;
            if (count == capacity)
            {
                Debug.Assert(readPtr == writePtr);

                var newItems = new T[capacity * 2];

                var beginCount = readPtr + 1;
                if (beginCount > 0)
                {
                    Array.Copy(items, 0, newItems, 0, beginCount);
                }

                writePtr += capacity;
                var endCount = capacity - beginCount;
                if (endCount > 0)
                {
                    Array.Copy(items, readPtr + 1, newItems, writePtr + 1, endCount);
                }

                items = newItems;
                mask = mask * 2 + 1;
            }
        }

        internal static void CalculateInsertionParameters(int mask, int count, int index, ref int readPtr, ref int writePtr, out int insertPtr, out int copyIndex, out int copyOffset, out int copyLength)
        {
            var indexOfLastElement = (readPtr + 1) & mask;
            if (index == 0)
            {
                insertPtr = readPtr = indexOfLastElement;

                // No copy is needed
                copyIndex = 0;
                copyOffset = 0;
                copyLength = 0;
                return;
            }

            insertPtr = (readPtr - index) & mask;
            if (index == count)
            {
                writePtr = (writePtr - 1) & mask;

                // No copy is needed
                copyIndex = 0;
                copyOffset = 0;
                copyLength = 0;
                return;
            }

            var canMoveRight = indexOfLastElement >= insertPtr;
            var moveRightCost = canMoveRight ? readPtr - insertPtr : int.MaxValue;

            var canMoveLeft = writePtr <= insertPtr;
            var moveLeftCost = canMoveLeft ? insertPtr - writePtr : int.MaxValue;

            if (moveRightCost <= moveLeftCost)
            {
                ++insertPtr;
                ++readPtr;
                copyIndex = insertPtr;
                copyOffset = 1;
                copyLength = moveRightCost;
            }
            else
            {
                copyIndex = writePtr + 1;
                copyOffset = -1;
                copyLength = moveLeftCost;
                writePtr = (writePtr - 1) & mask;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            var ptr = readPtr;
            for (var i = 0; i < Count; i++)
            {
                yield return items[ptr];
                ptr = (ptr - 1) & mask;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
