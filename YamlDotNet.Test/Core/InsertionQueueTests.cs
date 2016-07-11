//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) Antoine Aubry and contributors

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using YamlDotNet.Core;

namespace YamlDotNet.Test.Core
{
    public class InsertionQueueTests
    {
        [Fact]
        public void ShouldThrowExceptionWhenDequeuingEmptyContainer()
        {
            var queue = CreateQueue();

            Action action = () => queue.Dequeue();

            action.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void ShouldThrowExceptionWhenDequeuingContainerThatBecomesEmpty()
        {
            var queue = CreateQueue();

            queue.Enqueue(1);
            queue.Dequeue();
            Action action = () => queue.Dequeue();

            action.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void ShouldCorrectlyDequeueElementsAfterEnqueuing()
        {
            var queue = CreateQueue();

            WithTheRange(0, 10).Run(queue.Enqueue);

            OrderOfElementsIn(queue).Should().Equal(0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
        }

        [Fact]
        public void ShouldCorrectlyDequeueElementsWhenIntermixingEnqueuing()
        {
            var queue = CreateQueue();

            WithTheRange(0, 10).Run(queue.Enqueue);
            PerformTimes(5, queue.Dequeue);
            WithTheRange(10, 15).Run(queue.Enqueue);

            OrderOfElementsIn(queue).Should().Equal(5, 6, 7, 8, 9, 10, 11, 12, 13, 14);
        }

        [Fact]
        public void ShouldThrowExceptionWhenDequeuingAfterInserting()
        {
            var queue = CreateQueue();

            queue.Enqueue(1);
            queue.Insert(0, 99);
            PerformTimes(2, queue.Dequeue);
            Action action = () => queue.Dequeue();

            action.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void ShouldCorrectlyDequeueElementsWhenInserting()
        {
            var queue = CreateQueue();

            WithTheRange(0, 10).Run(queue.Enqueue);
            queue.Insert(5, 99);

            OrderOfElementsIn(queue).Should().Equal(0, 1, 2, 3, 4, 99, 5, 6, 7, 8, 9);
        }

        private static InsertionQueue<int> CreateQueue()
        {
            return new InsertionQueue<int>();
        }

        private IEnumerable<int> WithTheRange(int from, int to)
        {
            return Enumerable.Range(@from, to - @from);
        }

        private IEnumerable<int> OrderOfElementsIn(InsertionQueue<int> queue)
        {
            while (true)
            {
                if (queue.Count == 0)
                {
                    yield break;
                }
                yield return queue.Dequeue();
            }
        }

        public void PerformTimes(int times, Func<int> func)
        {
            WithTheRange(0, times).Run(x => func());
        }
    }
}