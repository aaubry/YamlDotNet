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
using System.IO;
using FluentAssertions;
using Xunit;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test.Analyzers.StaticGenerator
{
    public class HandleExceptionTests
    {
        [Fact]
        public void StaticSerializationHandlesTargetInvocationException()
        {
            var obj = new ThrowingPropertyExample();
            var serializer = new StaticSerializerBuilder(new StaticContext())
                .WithExceptionHandler((e, o, p) => 
                    $"Exception of type {e.GetType().FullName} was thrown in property {p} " +
                    "of " + (ReferenceEquals(o, obj) ? "expected" : "unexpected") + " object")
                .Build();
            var writer = new StringWriter();

            serializer.Serialize(writer, obj);
            var serialized = writer.ToString();

            serialized.Should().Be(
                "Value: Exception of type System.InvalidOperationException was thrown in property Value of expected object\r\n"
                    .NormalizeNewLines());
        }

        [Fact]
        public void StaticSerializationDoesntHandleTargetInvocationExceptionByDefault()
        {
            var serializer = new StaticSerializerBuilder(new StaticContext()).Build();
            var writer = new StringWriter();
            var obj = new ThrowingPropertyExample();

            Assert.Throws<InvalidOperationException>(() => serializer.Serialize(writer, obj));
        }
    }

    [YamlSerializable]
    public class ThrowingPropertyExample
    {
        public string Value => throw new InvalidOperationException();
    }
}
