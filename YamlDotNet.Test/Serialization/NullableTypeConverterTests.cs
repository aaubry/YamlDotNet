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
using YamlDotNet.Serialization.Converters;

namespace YamlDotNet.Test.Serialization
{
    public class NullableTypeConverterTests
    {
        // -- Guid ---------------------------------------------------------------

        [Fact]
        public void GuidConverter_Accepts_NullableGuid()
        {
            new GuidConverter(false).Accepts(typeof(Guid?)).Should().BeTrue();
        }

        [Fact]
        public void NullableGuid_WithValue_RoundTrips()
        {
            var obj = new NullableGuidHolder { Id = Guid.NewGuid() };
            Roundtrip(obj).Id.Should().Be(obj.Id);
        }

        [Fact]
        public void NullableGuid_WithNull_RoundTrips()
        {
            var obj = new NullableGuidHolder { Id = null };
            Roundtrip(obj).Id.Should().BeNull();
        }

        // -- TimeSpan -----------------------------------------------------------

        [Fact]
        public void TimeSpanConverter_Accepts_NullableTimeSpan()
        {
            new TimeSpanConverter().Accepts(typeof(TimeSpan?)).Should().BeTrue();
        }

        [Fact]
        public void NullableTimeSpan_WithValue_RoundTrips()
        {
            var obj = new NullableTimeSpanHolder { Duration = new TimeSpan(1, 2, 3) };
            Roundtrip(obj).Duration.Should().Be(obj.Duration);
        }

        [Fact]
        public void NullableTimeSpan_WithNull_RoundTrips()
        {
            var obj = new NullableTimeSpanHolder { Duration = null };
            Roundtrip(obj).Duration.Should().BeNull();
        }

        // -- Uri ----------------------------------------------------------------

        [Fact]
        public void NullableUri_WithValue_RoundTrips()
        {
            var obj = new NullableUriHolder { Endpoint = new Uri("https://example.com") };
            Roundtrip(obj).Endpoint.Should().Be(obj.Endpoint);
        }

        [Fact]
        public void NullableUri_WithNull_RoundTrips()
        {
            var obj = new NullableUriHolder { Endpoint = null };
            Roundtrip(obj).Endpoint.Should().BeNull();
        }

        // -- DateTimeOffset -----------------------------------------------------

        [Fact]
        public void DateTimeOffsetConverter_Accepts_NullableDateTimeOffset()
        {
            new DateTimeOffsetConverter().Accepts(typeof(DateTimeOffset?)).Should().BeTrue();
        }

        [Fact]
        public void NullableDateTimeOffset_WithValue_RoundTrips()
        {
            var converter = new DateTimeOffsetConverter();
            var obj = new NullableDateTimeOffsetHolder { Stamp = new DateTimeOffset(2025, 1, 2, 3, 4, 5, TimeSpan.FromHours(-6)) };

            var serializer = new SerializerBuilder().WithTypeConverter(converter).Build();
            var deserializer = new DeserializerBuilder().WithTypeConverter(converter).Build();

            var yaml = serializer.Serialize(obj);
            var result = deserializer.Deserialize<NullableDateTimeOffsetHolder>(yaml);
            result.Stamp.Should().Be(obj.Stamp);
        }

        [Fact]
        public void NullableDateTimeOffset_WithNull_RoundTrips()
        {
            var converter = new DateTimeOffsetConverter();
            var obj = new NullableDateTimeOffsetHolder { Stamp = null };

            var serializer = new SerializerBuilder().WithTypeConverter(converter).Build();
            var deserializer = new DeserializerBuilder().WithTypeConverter(converter).Build();

            var yaml = serializer.Serialize(obj);
            var result = deserializer.Deserialize<NullableDateTimeOffsetHolder>(yaml);
            result.Stamp.Should().BeNull();
        }

        // -- DateTime (via DateTimeConverter) ------------------------------------

        [Fact]
        public void DateTimeConverter_Accepts_NullableDateTime()
        {
            new DateTimeConverter().Accepts(typeof(DateTime?)).Should().BeTrue();
        }

        [Fact]
        public void NullableDateTime_WithNull_RoundTrips()
        {
            var converter = new DateTimeConverter();
            var obj = new NullableDateTimeHolder { When = null };

            var serializer = new SerializerBuilder().WithTypeConverter(converter).Build();
            var deserializer = new DeserializerBuilder().WithTypeConverter(converter).Build();

            var yaml = serializer.Serialize(obj);
            var result = deserializer.Deserialize<NullableDateTimeHolder>(yaml);
            result.When.Should().BeNull();
        }

        // -- DateTime (via DateTime8601Converter) --------------------------------

        [Fact]
        public void DateTime8601Converter_Accepts_NullableDateTime()
        {
            new DateTime8601Converter().Accepts(typeof(DateTime?)).Should().BeTrue();
        }

#if NET6_0_OR_GREATER
        // -- DateOnly -----------------------------------------------------------

        [Fact]
        public void DateOnlyConverter_Accepts_NullableDateOnly()
        {
            new DateOnlyConverter().Accepts(typeof(DateOnly?)).Should().BeTrue();
        }

        [Fact]
        public void NullableDateOnly_WithNull_RoundTrips()
        {
            var converter = new DateOnlyConverter();
            var obj = new NullableDateOnlyHolder { Day = null };

            var serializer = new SerializerBuilder().WithTypeConverter(converter).Build();
            var deserializer = new DeserializerBuilder().WithTypeConverter(converter).Build();

            var yaml = serializer.Serialize(obj);
            var result = deserializer.Deserialize<NullableDateOnlyHolder>(yaml);
            result.Day.Should().BeNull();
        }

        // -- TimeOnly -----------------------------------------------------------

        [Fact]
        public void TimeOnlyConverter_Accepts_NullableTimeOnly()
        {
            new TimeOnlyConverter().Accepts(typeof(TimeOnly?)).Should().BeTrue();
        }

        [Fact]
        public void NullableTimeOnly_WithNull_RoundTrips()
        {
            var converter = new TimeOnlyConverter();
            var obj = new NullableTimeOnlyHolder { At = null };

            var serializer = new SerializerBuilder().WithTypeConverter(converter).Build();
            var deserializer = new DeserializerBuilder().WithTypeConverter(converter).Build();

            var yaml = serializer.Serialize(obj);
            var result = deserializer.Deserialize<NullableTimeOnlyHolder>(yaml);
            result.At.Should().BeNull();
        }
#endif

        // -- Helpers ------------------------------------------------------------

        private static T Roundtrip<T>(T obj)
        {
            var serializer = new SerializerBuilder().Build();
            var deserializer = new DeserializerBuilder().Build();
            var yaml = serializer.Serialize(obj!);
            return deserializer.Deserialize<T>(yaml);
        }

        private class NullableGuidHolder { public Guid? Id { get; set; } }
        private class NullableTimeSpanHolder { public TimeSpan? Duration { get; set; } }
        private class NullableDateTimeHolder { public DateTime? When { get; set; } }
        private class NullableDateTimeOffsetHolder { public DateTimeOffset? Stamp { get; set; } }
#if NET6_0_OR_GREATER
        private class NullableDateOnlyHolder { public DateOnly? Day { get; set; } }
        private class NullableTimeOnlyHolder { public TimeOnly? At { get; set; } }
#endif

#nullable enable
        private class NullableUriHolder { public Uri? Endpoint { get; set; } }
#nullable restore
    }
}
