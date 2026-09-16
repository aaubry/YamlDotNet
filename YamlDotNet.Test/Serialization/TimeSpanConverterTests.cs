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
    public class TimeSpanConverterTests
    {
        [Theory]
        [InlineData(typeof(TimeSpan), true)]
        [InlineData(typeof(string), false)]
        [InlineData(typeof(int), false)]
        public void Accepts_ShouldReturn_ExpectedResult(Type type, bool expected)
        {
            var converter = new TimeSpanConverter();
            converter.Accepts(type).Should().Be(expected);
        }

        [Theory]
        [InlineData("01:30:00", 1, 30, 0)]
        [InlineData("1.02:03:04", 26, 3, 4)]
        [InlineData("00:00:00", 0, 0, 0)]
        [InlineData("12:34:56.7890000", 12, 34, 56)]
        public void ReadYaml_ShouldReturn_TimeSpan(string yamlValue, int hours, int minutes, int seconds)
        {
            var parser = A.Fake<IParser>();
            A.CallTo(() => parser.Current).Returns(new Scalar(yamlValue));
            A.CallTo(() => parser.MoveNext()).Returns(true);

            var converter = new TimeSpanConverter();
            var result = converter.ReadYaml(parser, typeof(TimeSpan), null!);

            result.Should().BeOfType<TimeSpan>();
            var ts = (TimeSpan)result;
            ts.Hours.Should().Be(hours % 24);
            ts.Minutes.Should().Be(minutes);
            ts.Seconds.Should().Be(seconds);
        }

        [Fact]
        public void WriteYaml_ShouldWrite_TimeSpan()
        {
            var emitter = A.Fake<IEmitter>();
            var converter = new TimeSpanConverter();
            var timeSpan = new TimeSpan(1, 2, 3, 4, 5);

            converter.WriteYaml(emitter, timeSpan, typeof(TimeSpan), null!);

            A.CallTo(() => emitter.Emit(A<Scalar>.That.Matches(s => s.Value == "1.02:03:04.0050000"))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void WriteYaml_JsonCompatible_ShouldUseDoubleQuotes()
        {
            var emitter = A.Fake<IEmitter>();
            var converter = new TimeSpanConverter(jsonCompatible: true);
            var timeSpan = new TimeSpan(1, 30, 0);

            converter.WriteYaml(emitter, timeSpan, typeof(TimeSpan), null!);

            A.CallTo(() => emitter.Emit(A<Scalar>.That.Matches(s => s.Style == ScalarStyle.DoubleQuoted))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void RoundTrip_ShouldPreserveValue()
        {
            var serializer = new SerializerBuilder().Build();
            var deserializer = new DeserializerBuilder().Build();

            var original = new TimeSpan(1, 2, 3, 4, 5);
            var yaml = serializer.Serialize(new { Duration = original });
            var result = deserializer.Deserialize<TimeSpanContainer>(yaml);

            result.Duration.Should().Be(original);
        }

        private class TimeSpanContainer
        {
            public TimeSpan Duration { get; set; }
        }
    }
}
