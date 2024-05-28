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
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test.Serialization
{
    public class PrivateConstructorTests
    {
        [Fact]
        public void PrivateConstructorsAreConsideredInRoundTripWhenEnabled()
        {
            var artifact = new Outer
            {
                Value = "test",
                InnerValue = new Inner("inner test")
            };
            var serializer = new SerializerBuilder()
                .EnablePrivateConstructors()
                .EnsureRoundtrip()
                .WithTagMapping(new TagName("outer"), typeof(Outer))
                .WithTagMapping(new TagName("inner"), typeof(Inner))
                .Build();
            var actual = serializer.Serialize(artifact).NormalizeNewLines().Trim(new char[] { '\r', '\n' });
            var expected = @"outer
Value: test
InnerValue: inner
  Value: inner test".NormalizeNewLines();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void PrivateConstructorsAreNotConsideredInRoundTripWhenNotEnabled()
        {
            var artifact = new Outer
            {
                Value = "test",
                InnerValue = new Inner("inner test")
            };
            var serializer = new SerializerBuilder()
                .EnsureRoundtrip()
                .WithTagMapping(new TagName("outer"), typeof(Outer))
                .WithTagMapping(new TagName("inner"), typeof(Inner))
                .Build();
            Assert.Throws<InvalidOperationException>(() =>
            {
                serializer.Serialize(artifact);
            });
        }

        [Fact]
        public void PrivateConstructorsAreConsideredWhenEnabled()
        {
            var yaml = @"
Value: test
InnerValue:
  Value: inner test";
            var deserializer = new DeserializerBuilder()
                .EnablePrivateConstructors()
                .Build();
            var value = deserializer.Deserialize<Outer>(yaml);
            Assert.Equal("test", value.Value);
            Assert.NotNull(value.InnerValue);
            Assert.Equal("inner test", value.InnerValue.Value);
        }

        [Fact]
        public void PrivateConstructorsAreNotConsideredWhenNotEnabled()
        {
            var yaml = @"
Value: test
InnerValue:
  Value: inner test";
            var deserializer = new DeserializerBuilder()
                .Build();
            var exception = Assert.Throws<YamlException>(() =>
            {
                deserializer.Deserialize<Outer>(yaml);
            });
            Assert.IsType<InvalidOperationException>(exception.InnerException);
            Assert.IsType<MissingMethodException>(exception.InnerException.InnerException);
        }

        [Fact]
        public void InternalConstructors()
        {
            Test(() => new TestClassInternal("test"), true);
            Test(() => new TestClassInternal("test"), false);
        }

        [Fact]
        public void PrivateConstructors()
        {
            Test(() => new TestClassPrivate("test"), true);
            Test(() => new TestClassPrivate("test"), false);
        }

        [Fact]
        public void ProtectedConstructors()
        {
            Test(() => new TestClassProtected("test"), true);
            Test(() => new TestClassProtected("test"), false);
        }

        [Fact]
        public void PublicConstructors()
        {
            Test(() => new TestClassPublic("test"), true);
            Test(() => new TestClassPublic("test"), false);
        }

        void Test<T>(Func<T> constructor, bool ensureRoundTrip)
        {
            var testClass = constructor();
            var serializerBuilder = new SerializerBuilder()
                .EnablePrivateConstructors()
                .IncludeNonPublicProperties();

            if (ensureRoundTrip)
            {
                serializerBuilder = serializerBuilder.EnsureRoundtrip();
            }
            var serializer = serializerBuilder.Build();

            var deserializer = new DeserializerBuilder()
                .EnablePrivateConstructors()
                .IncludeNonPublicProperties()
                .Build();

            var stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, testClass);
            var o = deserializer.Deserialize<T>(stringWriter.ToString());
        }

        public class TestClassInternal
        {
            internal TestClassInternal()
            {
            }

            public TestClassInternal(string value)
            {
                Value = value;
            }

            public string Value { get; private set; }

            public override string ToString()
            {
                return Value;
            }
        }

        public class TestClassPublic
        {
            public TestClassPublic()
            {
            }

            public TestClassPublic(string value)
            {
                Value = value;
            }

            public string Value { get; private set; }

            public override string ToString()
            {
                return Value;
            }
        }

        public class TestClassPrivate
        {
            public TestClassPrivate()
            {
            }

            public TestClassPrivate(string value)
            {
                Value = value;
            }

            public string Value { get; private set; }

            public override string ToString()
            {
                return Value;
            }
        }

        public class TestClassProtected
        {
            public TestClassProtected()
            {
            }

            public TestClassProtected(string value)
            {
                Value = value;
            }

            public string Value { get; private set; }

            public override string ToString()
            {
                return Value;
            }
        }


        private class Outer
        {
            public string Value { get; set; }
            public Inner InnerValue { get; set; }
        }

        private class Inner
        {
            public string Value { get; set; }

            private Inner()
            {

            }

            public Inner(string value)
            {
                Value = value;
            }
        }
    }
}
