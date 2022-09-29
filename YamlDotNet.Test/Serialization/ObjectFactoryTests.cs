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

using System.Collections;
using FluentAssertions;
using Xunit;
using YamlDotNet.Serialization.ObjectFactories;

namespace YamlDotNet.Test.Serialization
{
    public class ObjectFactoryTests : SerializationTestHelper
    {
        [Fact]
        public void NotSpecifyingObjectFactoryUsesDefault()
        {
            var text = "!empty {}";

            DeserializerBuilder.WithTagMapping("!empty", typeof(EmptyBase));
            var result = Deserializer.Deserialize(UsingReaderFor(text));

            result.Should().BeOfType<EmptyBase>();
        }

        [Fact]
        public void ObjectFactoryIsInvoked()
        {
            AssumingDeserializerWith(new LambdaObjectFactory(t => new EmptyDerived()));
            var text = "!empty {}";

            DeserializerBuilder.WithTagMapping("!empty", typeof(EmptyBase));
            var result = Deserializer.Deserialize(UsingReaderFor(text));

            result.Should().BeOfType<EmptyDerived>();
        }

        [Fact]
        public void DefaultObjectFactorySupportsNonGenericInterfaces()
        {
            var sut = new DefaultObjectFactory();
            var result = sut.Create(typeof(IList));
            Assert.IsAssignableFrom<IList>(result);
        }
    }
}
