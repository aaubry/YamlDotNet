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
using Xunit.Abstractions;
using YamlDotNet.Core;

namespace YamlDotNet.Test.Core
{
    public class InsertionQueueTests
    {
        private readonly ITestOutputHelper output;

        public InsertionQueueTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("-43210--", 0, "-43210X-")]
        [InlineData("---43210", 0, "X--43210")]
        [InlineData("10---432", 0, "10X--432")]

        [InlineData("--43210-", 5, "-X43210-")]
        [InlineData("43210---", 5, "43210--X")]
        [InlineData("210---43", 5, "210--X43")]

        [InlineData("-43210--", 2, "-432X10-")]
        [InlineData("---43210", 2, "--432X10")]
        [InlineData("43210---", 2, "432X10--")]
        [InlineData("210---43", 2, "2X10--43")]
        [InlineData("10---432", 2, "10--432X")]

        [InlineData("-43210--", 4, "4X3210--")]
        public void CalculateInsertionParameters_is_correct(string initialState, int index, string expectedFinalState)
        {
            static (int capacity, int readPtr, int writePtr, int insertPtr, List<(char chr, int idx)> elements) ParseState(string state)
            {
                var chars = state.ToArray()
                    .Select((chr, idx) => (chr, idx))
                    .ToList();

                var elements = chars
                    .Where(e => e.chr != '-')
                    .ToList();

                var insertPtr = chars
                    .Where(e => e.chr == 'X')
                    .Select(e => e.idx)
                    .FirstOrDefault();

                var readPtr = chars
                    .SkipWhile(e => e.chr == '-')
                    .TakeWhile(e => e.chr != '-')
                    .Last()
                    .idx;

                var writePtr = chars
                    .SkipWhile(e => e.chr != '-')
                    .TakeWhile(e => e.chr == '-')
                    .Last()
                    .idx;

                return (state.Length, readPtr, writePtr, insertPtr, elements);
            }

            var (capacity, readPtr, writePtr, _, initialElements) = ParseState(initialState);
            var (finalCapacity, expectedReadPtr, expectedWritePtr, expectedInsertPtr, finalElements) = ParseState(expectedFinalState);

            if (capacity != finalCapacity)
            {
                throw new InvalidOperationException($"Invalid test data: capacity: {capacity}, finalCapacity: {finalCapacity}");
            }

            var capacityIsPowerOf2 =
                (int)(Math.Ceiling((Math.Log(capacity) / Math.Log(2)))) ==
                (int)(Math.Floor(((Math.Log(capacity) / Math.Log(2)))));

            if (!capacityIsPowerOf2)
            {
                throw new InvalidOperationException($"Capacity should be a power of 2, but was {capacity}.");
            }

            output.WriteLine("Initial State");
            output.WriteLine("=============");
            output.WriteLine("");
            PrintChars((readPtr, 'R'), (writePtr, 'W'));
            output.WriteLine(initialState);
            output.WriteLine("");

            output.WriteLine("Expected Final State");
            output.WriteLine("====================");
            output.WriteLine("");
            PrintChars((expectedReadPtr, 'R'), (expectedWritePtr, 'W'));
            output.WriteLine(expectedFinalState);
            PrintChars((expectedInsertPtr, '^'));
            output.WriteLine("");

            var movedElements = initialElements
                .Join(finalElements, e => e.chr, e => e.chr, (i, f) => (i.chr, from: i.idx, offset: f.idx - i.idx))
                .Where(c => c.offset != 0)
                .ToList();

            int expectedCopyIndex, expectedCopyOffset, expectedCopyLength;
            if (movedElements.Count != 0)
            {
                if (movedElements.Select(e => e.offset).Distinct().Count() != 1)
                {
                    throw new InvalidOperationException("Invalid test data");
                }

                expectedCopyIndex = movedElements.Select(e => e.from).Min();
                expectedCopyOffset = movedElements[0].offset;
                expectedCopyLength = movedElements.Count;
            }
            else
            {
                expectedCopyIndex = 0;
                expectedCopyOffset = 0;
                expectedCopyLength = 0;
            }

            var mask = capacity - 1; // Assuming that capacity is a power of 2

            InsertionQueue<int>.CalculateInsertionParameters(mask, initialElements.Count, index, ref readPtr, ref writePtr, out var insertPtr, out var copyIndex, out var copyOffset, out var copyLength);

            static string Format(int readPtr, int writePtr, int insertPtr, int copyIndex, int copyOffset, int copyLength)
            {
                return $"readPtr: {readPtr}, writePtr: {writePtr}, insertPtr: {insertPtr}, copyIndex: {copyIndex}, copyOffset: {copyOffset}, copyLength: {copyLength}";
            }

            var expected = Format(expectedReadPtr, expectedWritePtr, expectedInsertPtr, expectedCopyIndex, expectedCopyOffset, expectedCopyLength);
            var actual = Format(readPtr, writePtr, insertPtr, copyIndex, copyOffset, copyLength);

            output.WriteLine($"expected: {expected}");
            output.WriteLine($"actual:   {actual}");

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0, 4)]
        [InlineData(1, 4)]
        [InlineData(2, 4)]
        [InlineData(3, 4)]
        public void Resize_is_is_correct_when_enqueuing(int offsetBeforeResize, int initialCapacity)
        {
            var sut = new InsertionQueue<int>(initialCapacity);

            for (int i = 0; i < offsetBeforeResize; ++i)
            {
                sut.Enqueue(-1);
                sut.Dequeue();
            }

            for (int i = 0; i < initialCapacity; ++i)
            {
                sut.Enqueue(i + 1);
            }

            // Sanity checks
            Assert.Equal(initialCapacity, sut.Capacity);
            Assert.Equal(Enumerable.Range(1, initialCapacity), sut);

            sut.Enqueue(initialCapacity + 1);

            Assert.Equal(initialCapacity * 2, sut.Capacity);
            Assert.Equal(Enumerable.Range(1, initialCapacity + 1), sut);
        }

        [Theory]
        [InlineData(0, 0, 4)]
        [InlineData(0, 1, 4)]
        [InlineData(0, 2, 4)]
        [InlineData(0, 3, 4)]
        [InlineData(0, 4, 4)]
        [InlineData(1, 0, 4)]
        [InlineData(1, 1, 4)]
        [InlineData(1, 2, 4)]
        [InlineData(1, 3, 4)]
        [InlineData(1, 4, 4)]
        [InlineData(2, 0, 4)]
        [InlineData(2, 1, 4)]
        [InlineData(2, 2, 4)]
        [InlineData(2, 3, 4)]
        [InlineData(2, 4, 4)]
        [InlineData(3, 0, 4)]
        [InlineData(3, 1, 4)]
        [InlineData(3, 2, 4)]
        [InlineData(3, 3, 4)]
        [InlineData(3, 4, 4)]
        public void Resize_is_is_correct_when_inserting(int offsetBeforeResize, int insertionIndex, int initialCapacity)
        {
            var sut = new InsertionQueue<int>(initialCapacity);

            for (int i = 0; i < offsetBeforeResize; ++i)
            {
                sut.Enqueue(-1);
                sut.Dequeue();
            }

            for (int i = 0; i < initialCapacity; ++i)
            {
                sut.Enqueue(i + 1);
            }

            // Sanity checks
            Assert.Equal(initialCapacity, sut.Capacity);
            Assert.Equal(Enumerable.Range(1, initialCapacity), sut);

            sut.Insert(insertionIndex, initialCapacity + 1);

            Assert.Equal(initialCapacity * 2, sut.Capacity);

            var expectedSequence =
                Enumerable.Range(1, insertionIndex)
                .Concat(new[] { initialCapacity + 1 })
                .Concat(Enumerable.Range(insertionIndex + 1, initialCapacity - insertionIndex));

            Assert.Equal(expectedSequence, sut);

            sut.Enqueue(-1);
        }

        private void PrintChars(params (int idx, char chr)[] characters)
        {
            var text = new char[characters.Max(c => c.idx) + 1];
            for (int i = 0; i < text.Length; ++i)
            {
                text[i] = ' ';
            }

            foreach (var (idx, chr) in characters)
            {
                text[idx] = chr;
            }

            output.WriteLine(new string(text));
        }

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
            return new InsertionQueue<int>(1);
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

        private void PerformTimes(int times, Func<int> func)
        {
            WithTheRange(0, times).Run(x => func());
        }
    }
}