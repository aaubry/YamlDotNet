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

#if NET6_0_OR_GREATER
using System;
using System.Globalization;
using FakeItEasy;
using FluentAssertions;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Converters;
using YamlDotNet.Serialization.NamingConventions;

namespace YamlDotNet.Test.Serialization
{
    /// <summary>
    /// This represents the test entity for the <see cref="TimeOnlyConverter"/> class.
    /// </summary>
    public class TimeOnlyConverterTests
    {
        /// <summary>
        /// Tests whether the Accepts() method should return expected result or not.
        /// </summary>
        /// <param name="type"><see cref="Type"/> to check.</param>
        /// <param name="expected">Expected result.</param>
        [Theory]
        [InlineData(typeof(TimeOnly), true)]
        [InlineData(typeof(string), false)]
        public void Given_Type_Accepts_ShouldReturn_Result(Type type, bool expected)
        {
            var converter = new TimeOnlyConverter();

            var result = converter.Accepts(type);

            result.Should().Be(expected);
        }

        /// <summary>
        /// Tests whether the ReadYaml() method should throw <see cref="FormatException"/> or not.
        /// </summary>
        /// <param name="hour">Hour value.</param>
        /// <param name="minute">Minute value.</param>
        /// <param name="second">Second value.</param>
        /// <remarks>The converter instance uses its default parameter of "T".</remarks>
        [Theory]
        [InlineData(6, 12, 31)]
        public void Given_Yaml_WithInvalidDateTimeFormat_WithDefaultParameters_ReadYaml_ShouldThrow_Exception(int hour, int minute, int second)
        {
            var yaml = $"{hour:00}-{minute:00}-{second:00}";

            var parser = A.Fake<IParser>();
            A.CallTo(() => parser.Current).ReturnsLazily(() => new Scalar(yaml));

            var converter = new TimeOnlyConverter();

            Action action = () => { converter.ReadYaml(parser, typeof(TimeOnly), null); };

            action.Should().Throw<FormatException>();
        }

        /// <summary>
        /// Tests whether the ReadYaml() method should return expected result or not.
        /// </summary>
        /// <param name="hour">Hour value.</param>
        /// <param name="minute">Minute value.</param>
        /// <param name="second">Second value.</param>
        /// <remarks>The converter instance uses its default parameter of "T".</remarks>
        [Theory]
        [InlineData(6, 12, 31)]
        public void Given_Yaml_WithValidDateTimeFormat_WithDefaultParameters_ReadYaml_ShouldReturn_Result(int hour, int minute, int second)
        {
            var yaml = $"{hour:00}:{minute:00}:{second:00}"; // This is the DateTime format of "T"

            var parser = A.Fake<IParser>();
            A.CallTo(() => parser.Current).ReturnsLazily(() => new Scalar(yaml));

            var converter = new TimeOnlyConverter();

            var result = converter.ReadYaml(parser, typeof(TimeOnly), null);

            result.Should().BeOfType<TimeOnly>();
            ((TimeOnly)result).Hour.Should().Be(hour);
            ((TimeOnly)result).Minute.Should().Be(minute);
            ((TimeOnly)result).Second.Should().Be(second);
        }

        /// <summary>
        /// Tests whether the ReadYaml() method should return expected result or not.
        /// </summary>
        /// <param name="hour">Hour value.</param>
        /// <param name="minute">Minute value.</param>
        /// <param name="second">Second value.</param>
        /// <param name="format1">Designated date/time format 1.</param>
        /// <param name="format2">Designated date/time format 2.</param>
        [Theory]
        [InlineData(6, 12, 31, "HH-mm-ss", "HH:mm:ss")]
        public void Given_Yaml_WithValidDateTimeFormat_ReadYaml_ShouldReturn_Result(int hour, int minute, int second, string format1, string format2)
        {
            var yaml = $"{hour:00}-{minute:00}-{second:00}";

            var parser = A.Fake<IParser>();
            A.CallTo(() => parser.Current).ReturnsLazily(() => new Scalar(yaml));

            var converter = new TimeOnlyConverter(formats: new[] { format1, format2 });

            var result = converter.ReadYaml(parser, typeof(TimeOnly), null);

            result.Should().BeOfType<TimeOnly>();
            ((TimeOnly)result).Hour.Should().Be(6);
            ((TimeOnly)result).Minute.Should().Be(12);
            ((TimeOnly)result).Second.Should().Be(31);
        }

        /// <summary>
        /// Tests whether the ReadYaml() method should return expected result or not.
        /// </summary>
        /// <param name="hour">Hour value.</param>
        /// <param name="minute">Minute value.</param>
        /// <param name="second">Second value.</param>
        /// <param name="format1">Designated date/time format 1.</param>
        /// <param name="format2">Designated date/time format 2.</param>
        [Theory]
        [InlineData(6, 12, 31, "HH-mm-ss", "HH:mm:ss")]
        public void Given_Yaml_WithSpecificCultureAndValidDateTimeFormat_ReadYaml_ShouldReturn_Result(int hour, int minute, int second, string format1, string format2)
        {
            var yaml = $"{hour:00}-{minute:00}-{second:00}";

            var parser = A.Fake<IParser>();
            A.CallTo(() => parser.Current).ReturnsLazily(() => new Scalar(yaml));

            var culture = new CultureInfo("ko-KR"); // Sample specific culture
            var converter = new TimeOnlyConverter(provider: culture, formats: new[] { format1, format2 });

            var result = converter.ReadYaml(parser, typeof(TimeOnly), null);

            result.Should().BeOfType<TimeOnly>();
            ((TimeOnly)result).Hour.Should().Be(6);
            ((TimeOnly)result).Minute.Should().Be(12);
            ((TimeOnly)result).Second.Should().Be(31);
        }

        /// <summary>
        /// Tests whether the ReadYaml() method should return expected result or not.
        /// </summary>
        /// <param name="format">Date/Time format.</param>
        /// <param name="value">Date/Time value.</param>
        [Theory]
        [InlineData("g", "01/11/2017 02:36")]
        [InlineData("G", "01/11/2017 02:36:16")]
        [InlineData("s", "2017-01-11T02:36:16")]
        [InlineData("t", "02:36")]
        [InlineData("T", "02:36:16")]
        [InlineData("u", "2017-01-11 02:36:16Z")]
        public void Given_Yaml_WithTimeFormat_ReadYaml_ShouldReturn_Result(string format, string value)
        {
            var expected = TimeOnly.ParseExact(value, format, CultureInfo.InvariantCulture);
            var converter = new TimeOnlyConverter(formats: new[] { "g", "G", "s", "t", "T", "u" });

            var parser = A.Fake<IParser>();
            A.CallTo(() => parser.Current).ReturnsLazily(() => new Scalar(value));

            var result = converter.ReadYaml(parser, typeof(TimeOnly), null);

            result.Should().Be(expected);
        }

        /// <summary>
        /// Tests whether the ReadYaml() method should return expected result or not.
        /// </summary>
        /// <param name="format">Date/Time format.</param>
        /// <param name="locale">Locale value.</param>
        /// <param name="value">Date/Time value.</param>
        [Theory]
        [InlineData("g", "fr-FR", "13/01/2017 05:25")]
        [InlineData("G", "fr-FR", "13/01/2017 05:25:08")]
        [InlineData("s", "fr-FR", "2017-01-13T05:25:08")]
        [InlineData("t", "fr-FR", "05:25")]
        [InlineData("T", "fr-FR", "05:25:08")]
        [InlineData("u", "fr-FR", "2017-01-13 05:25:08Z")]
        // [InlineData("g", "ko-KR", "2017-01-13 오전 5:32")]
        // [InlineData("G", "ko-KR", "2017-01-13 오전 5:32:06")]
        [InlineData("s", "ko-KR", "2017-01-13T05:32:06")]
        // [InlineData("t", "ko-KR", "오전 5:32")]
        // [InlineData("T", "ko-KR", "오전 5:32:06")]
        [InlineData("u", "ko-KR", "2017-01-13 05:32:06Z")]
        public void Given_Yaml_WithLocaleAndTimeFormat_ReadYaml_ShouldReturn_Result(string format, string locale, string value)
        {
            var culture = new CultureInfo(locale);

            var expected = default(TimeOnly);
            try
            {
                expected = TimeOnly.ParseExact(value, format, culture);
            }
            catch (Exception ex)
            {
                var message = string.Format("Failed to parse the test argument to TimeOnly. The expected date/time format should look like this: '{0}'", DateTime.Now.ToString(format, culture));
                throw new Exception(message, ex);
            }

            var converter = new TimeOnlyConverter(provider: culture, formats: new[] { "g", "G", "s", "t", "T", "u" });

            var parser = A.Fake<IParser>();
            A.CallTo(() => parser.Current).ReturnsLazily(() => new Scalar(value));

            var result = converter.ReadYaml(parser, typeof(TimeOnly), null);

            result.Should().Be(expected);
        }

        /// <summary>
        /// Tests whether the WriteYaml method should return expected result or not.
        /// </summary>
        /// <param name="hour">Hour value.</param>
        /// <param name="minute">Minute value.</param>
        /// <param name="second">Second value.</param>
        /// <remarks>The converter instance uses its default parameter of "T".</remarks>
        [Theory]
        [InlineData(6, 12, 31)]
        public void Given_Values_WriteYaml_ShouldReturn_Result(int hour, int minute, int second)
        {
            var timeOnly = new TimeOnly(hour, minute, second);
            var formatted = timeOnly.ToString("T", CultureInfo.InvariantCulture);
            var obj = new TestObject() { TimeOnly = timeOnly };

            var builder = new SerializerBuilder();
            builder.WithNamingConvention(CamelCaseNamingConvention.Instance);
            builder.WithTypeConverter(new TimeOnlyConverter());

            var serialiser = builder.Build();

            var serialised = serialiser.Serialize(obj);

            serialised.Should().ContainEquivalentOf($"timeonly: {formatted}");
        }

        /// <summary>
        /// Tests whether the WriteYaml method should return expected result or not.
        /// </summary>
        /// <param name="hour">Hour value.</param>
        /// <param name="minute">Minute value.</param>
        /// <param name="second">Second value.</param>
        /// <param name="locale">Locale value.</param>
        /// <remarks>The converter instance uses its default parameter of "T".</remarks>
        [Theory]
        [InlineData(6, 12, 31, "es-ES")]
        [InlineData(6, 12, 31, "ko-KR")]
        public void Given_Values_WithLocale_WriteYaml_ShouldReturn_Result(int hour, int minute, int second, string locale)
        {
            var timeOnly = new TimeOnly(hour, minute, second);
            var culture = new CultureInfo(locale);
            var formatted = timeOnly.ToString("T", culture);
            var obj = new TestObject() { TimeOnly = timeOnly };

            var builder = new SerializerBuilder();
            builder.WithNamingConvention(CamelCaseNamingConvention.Instance);
            builder.WithTypeConverter(new TimeOnlyConverter(provider: culture));

            var serialiser = builder.Build();

            var serialised = serialiser.Serialize(obj);

            serialised.Should().ContainEquivalentOf($"timeonly: {formatted}");
        }

        /// <summary>
        /// Tests whether the WriteYaml method should return expected result or not.
        /// </summary>
        /// <param name="hour">Hour value.</param>
        /// <param name="minute">Minute value.</param>
        /// <param name="second">Second value.</param>
        /// <remarks>The converter instance uses its default parameter of "T".</remarks>
        [Theory]
        [InlineData(6, 12, 31)]
        public void Given_Values_WithFormats_WriteYaml_ShouldReturn_Result_WithFirstFormat(int hour, int minute, int second)
        {
            var timeOnly = new TimeOnly(hour, minute, second);
            var format = "HH:mm:ss";
            var formatted = timeOnly.ToString(format, CultureInfo.InvariantCulture);
            var obj = new TestObject() { TimeOnly = timeOnly };

            var builder = new SerializerBuilder();
            builder.WithNamingConvention(CamelCaseNamingConvention.Instance);
            builder.WithTypeConverter(new TimeOnlyConverter(formats: new[] { format, "T" }));

            var serialiser = builder.Build();

            var serialised = serialiser.Serialize(obj);

            serialised.Should().ContainEquivalentOf($"timeonly: {formatted}");
        }

        [Fact]
        public void JsonCompatible_EncaseTimeOnlyInDoubleQuotes()
        {
            var serializer = new SerializerBuilder().JsonCompatible().Build();
            var testObject = new TestObject { TimeOnly = new TimeOnly(6, 12, 31) };
            var actual = serializer.Serialize(testObject);

            actual.TrimNewLines().Should().ContainEquivalentOf("{\"TimeOnly\": \"06:12:31\"}");
        }

        /// <summary>
        /// This represents the test object entity.
        /// </summary>
        private class TestObject
        {
            /// <summary>
            /// Gets or sets the <see cref="System.TimeOnly"/> value.
            /// </summary>
            public TimeOnly TimeOnly { get; set; }
        }
    }
}
#endif
