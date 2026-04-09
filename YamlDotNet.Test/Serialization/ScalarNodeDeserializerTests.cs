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
using YamlDotNet.Serialization;

namespace YamlDotNet.Test.Serialization
{
    public class ScalarNodeDeserializerTests
    {
        private readonly IDeserializer deserializer = new DeserializerBuilder().Build();

        [Theory]
        [InlineData("2024-01-15")]
        [InlineData("2024-12-31T23:59:59Z")]
        [InlineData("2024-06-15T10:30:00")]
        public void DateTime_ParsesValidValues(string yamlValue)
        {
            var result = deserializer.Deserialize<DateTime>(yamlValue);
            result.Should().BeAfter(DateTime.MinValue);
        }

        [Theory]
        [InlineData("not-a-date")]
        [InlineData("hello world")]
        public void DateTime_ThrowsOnInvalidValues(string yamlValue)
        {
            Action act = () => deserializer.Deserialize<DateTime>(yamlValue);
            act.Should().Throw<YamlException>();
        }

        [Theory]
        [InlineData("1:00:00", 3600)]     // 1 hour = 3600 seconds
        [InlineData("1:30", 90)]           // 1*60 + 30 = 90
        [InlineData("1:00", 60)]           // 1*60 + 0 = 60
        public void Base60Integer_ParsesValidValues(string yamlValue, int expected)
        {
            var result = deserializer.Deserialize<int>(yamlValue);
            result.Should().Be(expected);
        }

        [Fact]
        public void Base60Integer_RejectsInvalidSexagesimalDigit()
        {
            // 1:99 is invalid because 99 >= 60
            Action act = () => deserializer.Deserialize<int>("1:99");
            act.Should().Throw<YamlException>()
                .WithInnerException<FormatException>()
                .WithMessage("*sexagesimal*less than 60*");
        }

        [Fact]
        public void Base60Integer_AllowsFirstChunkAbove59()
        {
            // The first chunk can be any value (it's the most significant digit)
            // 100:30 = 100*60 + 30 = 6030
            var result = deserializer.Deserialize<int>("100:30");
            result.Should().Be(6030);
        }

        [Fact]
        public void Base60Integer_RejectsSecondChunkOf60()
        {
            // 1:60 is invalid because 60 >= 60
            Action act = () => deserializer.Deserialize<int>("1:60");
            act.Should().Throw<YamlException>()
                .WithInnerException<FormatException>()
                .WithMessage("*sexagesimal*less than 60*");
        }

        [Theory]
        [InlineData("0b1010", 10)]     // binary
        [InlineData("0b11111111", 255)] // binary 255
        [InlineData("010", 8)]         // octal
        [InlineData("0x1F", 31)]       // hex
        [InlineData("0xFF", 255)]      // hex
        public void IntegerBases_ParseCorrectly(string yamlValue, int expected)
        {
            var result = deserializer.Deserialize<int>(yamlValue);
            result.Should().Be(expected);
        }
    }
}
