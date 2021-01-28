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

using FakeItEasy;
using FluentAssertions;
using System;
using System.IO;
using Xunit;
using YamlDotNet.Core;

namespace YamlDotNet.Test.Core
{
    public class LookAheadBufferTests
    {
        private const string TestString = "abcdefghi";
        private const int Capacity = 4;

        [Fact]
        public void ShouldHaveReadOnceWhenPeekingAtOffsetZero()
        {
            var reader = new StringReader(TestString);
            var buffer = CreateBuffer(reader, Capacity);

            buffer.Peek(0).Should().Be('a');
        }

        [Fact]
        public void ShouldHaveReadTwiceWhenPeekingAtOffsetOne()
        {
            var reader = new StringReader(TestString);
            var buffer = CreateBuffer(reader, Capacity);

            buffer.Peek(0);

            buffer.Peek(1).Should().Be('b');
        }

        [Fact]
        public void ShouldHaveReadThriceWhenPeekingAtOffsetTwo()
        {
            var reader = new StringReader(TestString);
            var buffer = CreateBuffer(reader, Capacity);

            buffer.Peek(0);
            buffer.Peek(1);

            buffer.Peek(2).Should().Be('c');
        }

        [Fact]
        public void ShouldNotHaveReadAfterSkippingOneCharacter()
        {
            var reader = new StringReader(TestString);
            var buffer = CreateBuffer(reader, Capacity);

            buffer.Peek(2);

            buffer.Skip(1);

            buffer.Peek(0).Should().Be('b');
            buffer.Peek(1).Should().Be('c');
        }

        [Fact]
        public void ShouldHaveReadOnceAfterSkippingOneCharacter()
        {
            var reader = new StringReader(TestString);
            var buffer = CreateBuffer(reader, Capacity);

            buffer.Peek(2);

            buffer.Skip(1);
            buffer.Peek(2).Should().Be('d');
        }

        [Fact]
        public void ShouldHaveReadTwiceAfterSkippingOneCharacter()
        {
            var reader = new StringReader(TestString);
            var buffer = CreateBuffer(reader, Capacity);

            buffer.Peek(2);

            buffer.Skip(1);
            buffer.Peek(3).Should().Be('e');
        }

        [Fact]
        public void ShouldHaveReadOnceAfterSkippingFiveCharacters()
        {
            var reader = new StringReader(TestString);
            var buffer = CreateBuffer(reader, Capacity);

            buffer.Peek(2);
            buffer.Skip(1);
            buffer.Peek(3);

            buffer.Skip(4);
            buffer.Peek(0).Should().Be('f');
        }

        [Fact]
        public void ShouldHaveReadOnceAfterSkippingSixCharacters()
        {
            var reader = new StringReader(TestString);
            var buffer = CreateBuffer(reader, Capacity);

            buffer.Peek(2);
            buffer.Skip(1);
            buffer.Peek(3);
            buffer.Skip(4);
            buffer.Peek(0);

            buffer.Skip(1);
            buffer.Peek(0).Should().Be('g');
        }

        [Fact]
        public void ShouldHaveReadOnceAfterSkippingSevenCharacters()
        {
            var reader = new StringReader(TestString);
            var buffer = CreateBuffer(reader, Capacity);

            buffer.Peek(2);
            buffer.Skip(1);
            buffer.Peek(3);
            buffer.Skip(4);
            buffer.Peek(1);

            buffer.Skip(2);
            buffer.Peek(0).Should().Be('h');
        }

        [Fact]
        public void ShouldHaveReadOnceAfterSkippingEightCharacters()
        {
            var reader = new StringReader(TestString);
            var buffer = CreateBuffer(reader, Capacity);

            buffer.Peek(2);
            buffer.Skip(1);
            buffer.Peek(3);
            buffer.Skip(4);
            buffer.Peek(2);

            buffer.Skip(3);
            buffer.Peek(0).Should().Be('i');
        }

        [Fact]
        public void ShouldHaveReadOnceAfterSkippingNineCharacters()
        {
            var reader = new StringReader(TestString);
            var buffer = CreateBuffer(reader, Capacity);

            buffer.Peek(2);
            buffer.Skip(1);
            buffer.Peek(3);
            buffer.Skip(4);
            buffer.Peek(3);

            buffer.Skip(4);
            buffer.Peek(0).Should().Be('\0');
        }

        [Fact]
        public void ShouldFindEndOfInput()
        {
            var reader = new StringReader(TestString);
            var buffer = CreateBuffer(reader, Capacity);

            buffer.Peek(2);
            buffer.Skip(1);
            buffer.Peek(3);
            buffer.Skip(4);
            buffer.Peek(3);
            buffer.Skip(4);
            buffer.Peek(0);

            buffer.EndOfInput.Should().BeTrue();
        }

        [Fact]
        public void ShouldThrowWhenSkippingBeyondCurrentBuffer()
        {
            var reader = new StringReader(TestString);
            var buffer = CreateBuffer(reader, Capacity);

            buffer.Peek(3);
            Action action = () => buffer.Skip(5);

            action.ShouldThrow<ArgumentOutOfRangeException>();
        }

        private static LookAheadBuffer CreateBuffer(TextReader reader, int capacity)
        {
            return new LookAheadBuffer(reader, capacity);
        }
    }
}