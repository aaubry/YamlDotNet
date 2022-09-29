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
using FluentAssertions;
using Xunit;
using YamlDotNet.Core;

namespace YamlDotNet.Test.Core
{
    public class YamlExceptionTests
    {
        [Fact]
        public void VerifyToStringWithEmptyMarks()
        {
            var exception = new YamlException(Mark.Empty, Mark.Empty, "Test exception message");
            exception.ToString().Should().Be("(Line: 1, Col: 1, Idx: 0) - (Line: 1, Col: 1, Idx: 0): Test exception message");
            exception.Message.Should().Be("Test exception message");
        }

        [Fact]
        public void VerifyToStringWithNonEmptyMarks()
        {
            var exception = new YamlException(new Mark(1, 1, 1), new Mark(10, 10, 10), "Test exception message");
            exception.ToString().Should().Be("(Line: 1, Col: 1, Idx: 1) - (Line: 10, Col: 10, Idx: 10): Test exception message");
            exception.Message.Should().Be("Test exception message");
        }

        [Fact]
        public void VerifyToStringWithInnerExceptionAndMarks()
        {
            var exception = new YamlException(new Mark(1, 1, 1), new Mark(10, 10, 10), "Test exception message", new InvalidOperationException("Test inner exception"));
            exception.ToString().Should().Be("(Line: 1, Col: 1, Idx: 1) - (Line: 10, Col: 10, Idx: 10): Test exception message");
            exception.Message.Should().Be("Test exception message");
        }

        [Fact]
        public void VerifyToStringWithInnerException()
        {
            var exception = new YamlException("Test exception message", new InvalidOperationException("Test inner exception"));
            exception.ToString().Should().Be("(Line: 1, Col: 1, Idx: 0) - (Line: 1, Col: 1, Idx: 0): Test exception message");
            exception.Message.Should().Be("Test exception message");
        }
    }
}
