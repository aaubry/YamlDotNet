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
using System.Globalization;
using FakeItEasy;
using FluentAssertions;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Converters;

namespace YamlDotNet.Test.Serialization
{
    /// <summary>
    /// This represents the test entity for the <see cref="DateTimeConverter"/> class.
    /// </summary>
    public class DateTimeOffsetConverterTests
    {
        private readonly DateTimeOffset _expected = new DateTimeOffset(new DateTime(2017, 1, 2, 3, 4, 5), new TimeSpan(-6, 0, 0));
        /// <summary>
        /// Tests whether the Accepts() method should return expected result or not.
        /// </summary>
        /// <param name="type"><see cref="Type"/> to check.</param>
        /// <param name="expected">Expected result.</param>
        [Theory]
        [InlineData(typeof(DateTimeOffset), true)]
        [InlineData(typeof(string), false)]
        public void AcceptsTypeReturns(Type type, bool expected)
        {
            var converter = new DateTimeOffsetConverter(CultureInfo.InvariantCulture);

            var result = converter.Accepts(type);

            result.Should().Be(expected);
        }

        [Fact]
        public void InvalidFormatThrowsException()
        {
            var yaml = "2019-01-01";

            var parser = A.Fake<IParser>();
            A.CallTo(() => parser.Current).ReturnsLazily(() => new Scalar(yaml));

            var converter = new DateTimeOffsetConverter(CultureInfo.InvariantCulture);

            Action action = () => { converter.ReadYaml(parser, typeof(DateTimeOffset), null); };

            action.ShouldThrow<FormatException>();
        }

        [Fact]
        public void ValidYamlReturnsDateTimeOffsetDefaultFormat()
        {
            var yaml = "2017-01-02T03:04:05.0000000-06:00";

            var parser = A.Fake<IParser>();
            A.CallTo(() => parser.Current).ReturnsLazily(() => new Scalar(yaml));
            var converter = new DateTimeOffsetConverter(CultureInfo.InvariantCulture);
            var actual = converter.ReadYaml(parser, typeof(DateTimeOffset), null);
            Assert.Equal(_expected, actual);
        }

        [Fact]
        public void ValidYamlReturnsDateTimeOffsetAdditionalFormats()
        {
            var yaml = "01/02/2017 03:04:05 -06:00";

            var parser = A.Fake<IParser>();
            A.CallTo(() => parser.Current).ReturnsLazily(() => new Scalar(yaml));

            var converter = new DateTimeOffsetConverter(
                CultureInfo.InvariantCulture,
                ScalarStyle.Any,
                DateTimeStyles.None,
                "O",
                "MM/dd/yyyy HH:mm:ss zzz");

            converter.ReadYaml(parser, typeof(DateTimeOffset), null);
            var actual = converter.ReadYaml(parser, typeof(DateTimeOffset), null);
            Assert.Equal(_expected, actual);
        }

        [Fact]
        public void ShouldSerializeRoundTripWithDefaults()
        {
            var converter = new DateTimeOffsetConverter(CultureInfo.InvariantCulture);
            var serializer = new SerializerBuilder().WithTypeConverter(converter).Build();
            var actual = serializer.Serialize(new Test { X = _expected }).NormalizeNewLines();
            var expected = "X: 2017-01-02T03:04:05.0000000-06:00\r\n".NormalizeNewLines();
            Assert.Equal(actual, expected);
        }

        [Fact]
        public void ShouldSerializeWithCustomFormat()
        {
            var converter = new DateTimeOffsetConverter(
                CultureInfo.InvariantCulture,
                ScalarStyle.Any,
                DateTimeStyles.None,
                "MM/dd/yyyy HH:mm:ss zzz");

            var serializer = new SerializerBuilder().WithTypeConverter(converter).Build();
            var actual = serializer.Serialize(new Test { X = _expected }).NormalizeNewLines();
            var expected = "X: 01/02/2017 03:04:05 -06:00\r\n".NormalizeNewLines();
            Assert.Equal(actual, expected);
        }

        [Fact]
        public void ShouldSerializeAndRespectQuotingStyle()
        {
            var converter = new DateTimeOffsetConverter(CultureInfo.InvariantCulture, ScalarStyle.DoubleQuoted);
            var serializer = new SerializerBuilder().WithTypeConverter(converter).Build();
            var actual = serializer.Serialize(new Test { X = _expected }).NormalizeNewLines();
            var expected = "X: \"2017-01-02T03:04:05.0000000-06:00\"\r\n".NormalizeNewLines();
            Assert.Equal(actual, expected);
        }

        private class Test
        {
            public DateTimeOffset X { get; set; }
        }
    }
}
