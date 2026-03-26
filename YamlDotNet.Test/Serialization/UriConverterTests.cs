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
using FakeItEasy;
using FluentAssertions;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Converters;

namespace YamlDotNet.Test.Serialization
{
    public class UriConverterTests
    {
        [Theory]
        [InlineData(typeof(Uri), true)]
        [InlineData(typeof(string), false)]
        [InlineData(typeof(int), false)]
        public void Accepts_ShouldReturn_ExpectedResult(Type type, bool expected)
        {
            var converter = new UriConverter();
            converter.Accepts(type).Should().Be(expected);
        }

        [Theory]
        [InlineData("https://example.com")]
        [InlineData("http://localhost:8080/path?query=1")]
        [InlineData("/relative/path")]
        public void ReadYaml_ShouldReturn_Uri(string yamlValue)
        {
            var parser = A.Fake<IParser>();
            A.CallTo(() => parser.Current).Returns(new Scalar(yamlValue));
            A.CallTo(() => parser.MoveNext()).Returns(true);

            var converter = new UriConverter();
            var result = converter.ReadYaml(parser, typeof(Uri), null!);

            result.Should().BeOfType<Uri>();
            var uri = (Uri)result;
            uri.OriginalString.Should().Be(yamlValue);
        }

        [Fact]
        public void WriteYaml_ShouldWrite_AbsoluteUri()
        {
            var emitter = A.Fake<IEmitter>();
            var converter = new UriConverter();
            var uri = new Uri("https://example.com/path");

            converter.WriteYaml(emitter, uri, typeof(Uri), null!);

            A.CallTo(() => emitter.Emit(A<Scalar>.That.Matches(s => s.Value == "https://example.com/path"))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void WriteYaml_JsonCompatible_ShouldUseDoubleQuotes()
        {
            var emitter = A.Fake<IEmitter>();
            var converter = new UriConverter(jsonCompatible: true);
            var uri = new Uri("https://example.com");

            converter.WriteYaml(emitter, uri, typeof(Uri), null!);

            A.CallTo(() => emitter.Emit(A<Scalar>.That.Matches(s => s.Style == ScalarStyle.DoubleQuoted))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void RoundTrip_ShouldPreserveValue()
        {
            var serializer = new SerializerBuilder().Build();
            var deserializer = new DeserializerBuilder().Build();

            var original = new Uri("https://example.com/test?q=1&r=2");
            var yaml = serializer.Serialize(new { Endpoint = original });
            var result = deserializer.Deserialize<UriContainer>(yaml);

            result.Endpoint.Should().Be(original);
        }

        private class UriContainer
        {
            public Uri Endpoint { get; set; }
        }
    }
}
