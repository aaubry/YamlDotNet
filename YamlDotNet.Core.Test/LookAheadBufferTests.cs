//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2008, 2009, 2010, 2011, 2012, 2013 Antoine Aubry
    
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
using System.IO;
using FakeItEasy;
using FakeItEasy.Core;
using FluentAssertions;
using Xunit;

namespace YamlDotNet.Core.Test
{
	public class LookAheadBufferTests
	{
		private const string TestString = "abcdefghi";
		private const int Capacity = 4;

		[Fact]
		public void ShouldHaveReadOnceWhenPeekingAtOffsetZero()
		{
			var reader = CreateFakeReader(TestString);
			var buffer = CreateBuffer(reader, Capacity);

			buffer.Peek(0).Should().Be('a');
			A.CallTo(() => reader.Read()).MustHaveHappened(Repeated.Exactly.Once);
		}

		[Fact]
		public void ShouldHaveReadTwiceWhenPeekingAtOffsetOne()
		{
			var reader = CreateFakeReader(TestString);
			var buffer = CreateBuffer(reader, Capacity);

			buffer.Peek(0);

			buffer.Peek(1).Should().Be('b');
			A.CallTo(() => reader.Read()).MustHaveHappened(Repeated.Exactly.Twice);
		}

		[Fact]
		public void ShouldHaveReadThriceWhenPeekingAtOffsetTwo()
		{
			var reader = CreateFakeReader(TestString);
			var buffer = CreateBuffer(reader, Capacity);

			buffer.Peek(0);
			buffer.Peek(1);

			buffer.Peek(2).Should().Be('c');
			A.CallTo(() => reader.Read()).MustHaveHappened(Repeated.Exactly.Times(3));
		}

		[Fact]
		public void ShouldNotHaveReadAfterSkippingOneCharacter()
		{
			var reader = CreateFakeReader(TestString);
			var buffer = CreateBuffer(reader, Capacity);

			buffer.Peek(2);

			using (OnlyTheseCalls)
			{
				buffer.Skip(1);

				buffer.Peek(0).Should().Be('b');
				buffer.Peek(1).Should().Be('c');
				A.CallTo(() => reader.Read()).MustNotHaveHappened();
			}
		}

		[Fact]
		public void ShouldHaveReadOnceAfterSkippingOneCharacter()
		{
			var reader = CreateFakeReader(TestString);
			var buffer = CreateBuffer(reader, Capacity);

			buffer.Peek(2);

			using (OnlyTheseCalls)
			{
				buffer.Skip(1);

				buffer.Peek(2).Should().Be('d');
				A.CallTo(() => reader.Read()).MustHaveHappened(Repeated.Exactly.Once);
			}
		}

		[Fact]
		public void ShouldHaveReadTwiceAfterSkippingOneCharacter()
		{
			var reader = CreateFakeReader(TestString);
			var buffer = CreateBuffer(reader, Capacity);

			buffer.Peek(2);

			using (OnlyTheseCalls) {
				buffer.Skip(1);

				buffer.Peek(3).Should().Be('e');
				A.CallTo(() => reader.Read()).MustHaveHappened(Repeated.Exactly.Twice);
			}
		}

		[Fact]
		public void ShouldHaveReadOnceAfterSkippingFiveCharacters()
		{
			var reader = CreateFakeReader(TestString);
			var buffer = CreateBuffer(reader, Capacity);

			buffer.Peek(2);
			buffer.Skip(1);
			buffer.Peek(3);

			using (OnlyTheseCalls) {
				buffer.Skip(4);

				buffer.Peek(0).Should().Be('f');
				A.CallTo(() => reader.Read()).MustHaveHappened(Repeated.Exactly.Once);
			}
		}

		[Fact]
		public void ShouldHaveReadOnceAfterSkippingSixCharacters()
		{
			var reader = CreateFakeReader(TestString);
			var buffer = CreateBuffer(reader, Capacity);

			buffer.Peek(2);
			buffer.Skip(1);
			buffer.Peek(3);
			buffer.Skip(4);
			buffer.Peek(0);

			using (OnlyTheseCalls) {
				buffer.Skip(1);

				buffer.Peek(0).Should().Be('g');
				A.CallTo(() => reader.Read()).MustHaveHappened(Repeated.Exactly.Once);
			}
		}

		[Fact]
		public void ShouldHaveReadOnceAfterSkippingSevenCharacters() {
			var reader = CreateFakeReader(TestString);
			var buffer = CreateBuffer(reader, Capacity);

			buffer.Peek(2);
			buffer.Skip(1);
			buffer.Peek(3);
			buffer.Skip(4);
			buffer.Peek(1);

			using (OnlyTheseCalls) {
				buffer.Skip(2);

				buffer.Peek(0).Should().Be('h');
				A.CallTo(() => reader.Read()).MustHaveHappened(Repeated.Exactly.Once);
			}
		}

		[Fact]
		public void ShouldHaveReadOnceAfterSkippingEightCharacters()
		{
			var reader = CreateFakeReader(TestString);
			var buffer = CreateBuffer(reader, Capacity);

			buffer.Peek(2);
			buffer.Skip(1);
			buffer.Peek(3);
			buffer.Skip(4);
			buffer.Peek(2);

			using (OnlyTheseCalls) {
				buffer.Skip(3);

				buffer.Peek(0).Should().Be('i');
				A.CallTo(() => reader.Read()).MustHaveHappened(Repeated.Exactly.Once);
			}
		}

		[Fact]
		public void ShouldHaveReadOnceAfterSkippingNineCharacters()
		{
			var reader = CreateFakeReader(TestString);
			var buffer = CreateBuffer(reader, Capacity);

			buffer.Peek(2);
			buffer.Skip(1);
			buffer.Peek(3);
			buffer.Skip(4);
			buffer.Peek(3);

			using (OnlyTheseCalls) {
				buffer.Skip(4);

				buffer.Peek(0).Should().Be('\0');
				A.CallTo(() => reader.Read()).MustHaveHappened(Repeated.Exactly.Once);
			}
		}

		[Fact]
		public void ShouldFindEndOfInput()
		{
			var reader = CreateFakeReader(TestString);
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
		public void ShouldThrowWhenPeekingBeyondCapacity()
		{
			var reader = CreateFakeReader(TestString);
			var buffer = CreateBuffer(reader, Capacity);

			Action action = () => buffer.Peek(4);

			action.ShouldThrow<ArgumentOutOfRangeException>();
		}

		[Fact]
		public void ShouldThrowWhenSkippingBeyondCurrentBuffer()
		{
			var reader = CreateFakeReader(TestString);
			var buffer = CreateBuffer(reader, Capacity);

			buffer.Peek(3);
			Action action = () => buffer.Skip(5);

			action.ShouldThrow<ArgumentOutOfRangeException>();
		}

		private static TextReader CreateFakeReader(string text)
		{
			return A.Fake<TextReader>(x => x.Wrapping(new StringReader(text)));
		}

		private static LookAheadBuffer CreateBuffer(TextReader reader, int capacity)
		{
			return new LookAheadBuffer(reader, capacity);
		}

		private static IFakeScope OnlyTheseCalls
		{
			get { return Fake.CreateScope(); }
		}
	}
}