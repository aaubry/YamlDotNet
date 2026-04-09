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
using YamlDotNet.Serialization;

namespace YamlDotNet.Test.Serialization
{
    /// <summary>
    /// Tests that types with a static Parse(string, IFormatProvider) method
    /// (i.e. types implementing IParsable&lt;T&gt; on .NET 7+) are correctly
    /// deserialized from YAML scalars without needing a custom IYamlTypeConverter.
    /// </summary>
    public class IParsableDeserializationTests
    {
        [Fact]
        public void TimeSpan_IsDeserialized()
        {
            var yaml = "Duration: 01:30:00\n";
            var deserializer = new DeserializerBuilder().Build();
            var result = deserializer.Deserialize<TimeSpanHolder>(yaml);
            result.Duration.Should().Be(new TimeSpan(1, 30, 0));
        }

        [Fact]
        public void TimeSpan_WithDays_IsDeserialized()
        {
            var yaml = "Duration: 1.02:03:04\n";
            var deserializer = new DeserializerBuilder().Build();
            var result = deserializer.Deserialize<TimeSpanHolder>(yaml);
            result.Duration.Should().Be(new TimeSpan(1, 2, 3, 4));
        }

        [Fact]
        public void TimeSpan_RoundTrips()
        {
            var serializer = new SerializerBuilder().Build();
            var deserializer = new DeserializerBuilder().Build();

            var original = new TimeSpanHolder { Duration = new TimeSpan(1, 2, 3, 4, 5) };
            var yaml = serializer.Serialize(original);
            var result = deserializer.Deserialize<TimeSpanHolder>(yaml);

            result.Duration.Should().Be(original.Duration);
        }

        [Fact]
        public void DateTimeOffset_IsDeserialized()
        {
            var yaml = "Stamp: 2025-01-02T03:04:05+00:00\n";
            var deserializer = new DeserializerBuilder().Build();
            var result = deserializer.Deserialize<DateTimeOffsetHolder>(yaml);
            result.Stamp.Should().Be(new DateTimeOffset(2025, 1, 2, 3, 4, 5, TimeSpan.Zero));
        }

#if NET6_0_OR_GREATER
        [Fact]
        public void DateOnly_IsDeserialized()
        {
            var yaml = "Day: 2025-06-15\n";
            var deserializer = new DeserializerBuilder().Build();
            var result = deserializer.Deserialize<DateOnlyHolder>(yaml);
            result.Day.Should().Be(new DateOnly(2025, 6, 15));
        }

        [Fact]
        public void TimeOnly_IsDeserialized()
        {
            var yaml = "At: 14:30:00\n";
            var deserializer = new DeserializerBuilder().Build();
            var result = deserializer.Deserialize<TimeOnlyHolder>(yaml);
            result.At.Should().Be(new TimeOnly(14, 30, 0));
        }
#endif

        private class TimeSpanHolder { public TimeSpan Duration { get; set; } }
        private class DateTimeOffsetHolder { public DateTimeOffset Stamp { get; set; } }
#if NET6_0_OR_GREATER
        private class DateOnlyHolder { public DateOnly Day { get; set; } }
        private class TimeOnlyHolder { public TimeOnly At { get; set; } }
#endif
    }
}
