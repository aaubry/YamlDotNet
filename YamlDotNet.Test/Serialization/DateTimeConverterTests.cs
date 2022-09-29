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
using YamlDotNet.Serialization.NamingConventions;

namespace YamlDotNet.Test.Serialization
{
    /// <summary>
    /// This represents the test entity for the <see cref="DateTimeConverter"/> class.
    /// </summary>
    public class DateTimeConverterTests
    {
        /// <summary>
        /// Tests whether the Accepts() method should return expected result or not.
        /// </summary>
        /// <param name="type"><see cref="Type"/> to check.</param>
        /// <param name="expected">Expected result.</param>
        [Theory]
        [InlineData(typeof(DateTime), true)]
        [InlineData(typeof(string), false)]
        public void Given_Type_Accepts_ShouldReturn_Result(Type type, bool expected)
        {
            var converter = new DateTimeConverter();

            var result = converter.Accepts(type);

            result.Should().Be(expected);
        }

        /// <summary>
        /// Tests whether the ReadYaml() method should throw <see cref="FormatException"/> or not.
        /// </summary>
        /// <param name="year">Year value.</param>
        /// <param name="month">Month value.</param>
        /// <param name="day">Day value.</param>
        /// <remarks>The converter instance uses its default parameters of "G" and UTC.</remarks>
        [Theory]
        [InlineData(2016, 12, 31)]
        public void Given_Yaml_WithInvalidDateTimeFormat_WithDefaultParameters_ReadYaml_ShouldThrow_Exception(int year, int month, int day)
        {
            var yaml = $"{year}-{month:00}-{day:00}";

            var parser = A.Fake<IParser>();
            A.CallTo(() => parser.Current).ReturnsLazily(() => new Scalar(yaml));

            var converter = new DateTimeConverter();

            Action action = () => { converter.ReadYaml(parser, typeof(DateTime)); };

            action.ShouldThrow<FormatException>();
        }

        /// <summary>
        /// Tests whether the ReadYaml() method should return expected result or not.
        /// </summary>
        /// <param name="year">Year value.</param>
        /// <param name="month">Month value.</param>
        /// <param name="day">Day value.</param>
        /// <param name="hour">Hour value.</param>
        /// <param name="minute">Minute value.</param>
        /// <param name="second">Second value.</param>
        /// <remarks>The converter instance uses its default parameters of "G" and UTC.</remarks>
        [Theory]
        [InlineData(2016, 12, 31, 3, 0, 0)]
        public void Given_Yaml_WithValidDateTimeFormat_WithDefaultParameters_ReadYaml_ShouldReturn_Result(int year, int month, int day, int hour, int minute, int second)
        {
            var yaml = $"{month:00}/{day:00}/{year} {hour:00}:{minute:00}:{second:00}"; // This is the DateTime format of "G"

            var parser = A.Fake<IParser>();
            A.CallTo(() => parser.Current).ReturnsLazily(() => new Scalar(yaml));

            var converter = new DateTimeConverter();

            var result = converter.ReadYaml(parser, typeof(DateTime));

            result.Should().BeOfType<DateTime>();
            ((DateTime)result).Kind.Should().Be(DateTimeKind.Utc);
            ((DateTime)result).ToUniversalTime().Year.Should().Be(year);
            ((DateTime)result).ToUniversalTime().Month.Should().Be(month);
            ((DateTime)result).ToUniversalTime().Day.Should().Be(day);
            ((DateTime)result).ToUniversalTime().Hour.Should().Be(hour);
            ((DateTime)result).ToUniversalTime().Minute.Should().Be(minute);
            ((DateTime)result).ToUniversalTime().Second.Should().Be(second);
        }

        /// <summary>
        /// Tests whether the ReadYaml() method should return expected result or not.
        /// </summary>
        /// <param name="year">Year value.</param>
        /// <param name="month">Month value.</param>
        /// <param name="day">Day value.</param>
        /// <param name="hour">Hour value.</param>
        /// <param name="minute">Minute value.</param>
        /// <param name="second">Second value.</param>
        /// <remarks>The converter instance uses its default parameters of "G".</remarks>
        [Theory]
        [InlineData(2016, 12, 31, 3, 0, 0)]
        public void Given_Yaml_WithValidDateTimeFormat_WithDefaultParameterAndUnspecified_ReadYaml_ShouldReturn_Result(int year, int month, int day, int hour, int minute, int second)
        {
            var yaml = $"{month:00}/{day:00}/{year} {hour:00}:{minute:00}:{second:00}"; // This is the DateTime format of "G"

            var parser = A.Fake<IParser>();
            A.CallTo(() => parser.Current).ReturnsLazily(() => new Scalar(yaml));

            var converter = new DateTimeConverter(DateTimeKind.Unspecified);

            var result = converter.ReadYaml(parser, typeof(DateTime));

            result.Should().BeOfType<DateTime>();
            ((DateTime)result).Kind.Should().Be(DateTimeKind.Utc);
            ((DateTime)result).ToUniversalTime().Year.Should().Be(year);
            ((DateTime)result).ToUniversalTime().Month.Should().Be(month);
            ((DateTime)result).ToUniversalTime().Day.Should().Be(day);
            ((DateTime)result).ToUniversalTime().Hour.Should().Be(hour);
            ((DateTime)result).ToUniversalTime().Minute.Should().Be(minute);
            ((DateTime)result).ToUniversalTime().Second.Should().Be(second);
        }

        /// <summary>
        /// Tests whether the ReadYaml() method should return expected result or not.
        /// </summary>
        /// <param name="year">Year value.</param>
        /// <param name="month">Month value.</param>
        /// <param name="day">Day value.</param>
        /// <param name="hour">Hour value.</param>
        /// <param name="minute">Minute value.</param>
        /// <param name="second">Second value.</param>
        /// <remarks>The converter instance uses its default parameter of "G".</remarks>
        [Theory]
        [InlineData(2016, 12, 31, 3, 0, 0)]
        public void Given_Yaml_WithValidDateTimeFormat_WithDefaultParameterAndLocal_ReadYaml_ShouldReturn_Result(int year, int month, int day, int hour, int minute, int second)
        {
            var yaml = $"{month:00}/{day:00}/{year} {hour:00}:{minute:00}:{second:00}"; // This is the DateTime format of "G"

            var parser = A.Fake<IParser>();
            A.CallTo(() => parser.Current).ReturnsLazily(() => new Scalar(yaml));

            var converter = new DateTimeConverter(DateTimeKind.Local);

            var result = converter.ReadYaml(parser, typeof(DateTime));

            result.Should().BeOfType<DateTime>();
            ((DateTime)result).Kind.Should().Be(DateTimeKind.Local);
            ((DateTime)result).ToLocalTime().Year.Should().Be(year);
            ((DateTime)result).ToLocalTime().Month.Should().Be(month);
            ((DateTime)result).ToLocalTime().Day.Should().Be(day);
            ((DateTime)result).ToLocalTime().Hour.Should().Be(hour);
            ((DateTime)result).ToLocalTime().Minute.Should().Be(minute);
            ((DateTime)result).ToLocalTime().Second.Should().Be(second);
        }

        /// <summary>
        /// Tests whether the ReadYaml() method should return expected result or not.
        /// </summary>
        /// <param name="year">Year value.</param>
        /// <param name="month">Month value.</param>
        /// <param name="day">Day value.</param>
        /// <param name="format1">Designated date/time format 1.</param>
        /// <param name="format2">Designated date/time format 2.</param>
        /// <remarks>The converter instance uses its default parameter of UTC.</remarks>
        [Theory]
        [InlineData(2016, 12, 31, "yyyy-MM-dd", "yyyy/MM/dd HH:mm:ss")]
        public void Given_Yaml_WithValidDateTimeFormat_ReadYaml_ShouldReturn_Result(int year, int month, int day, string format1, string format2)
        {
            var yaml = $"{year}-{month:00}-{day:00}";

            var parser = A.Fake<IParser>();
            A.CallTo(() => parser.Current).ReturnsLazily(() => new Scalar(yaml));

            var converter = new DateTimeConverter(formats: new[] { format1, format2 });

            var result = converter.ReadYaml(parser, typeof(DateTime));

            result.Should().BeOfType<DateTime>();
            ((DateTime)result).Kind.Should().Be(DateTimeKind.Utc);
            ((DateTime)result).ToUniversalTime().Year.Should().Be(year);
            ((DateTime)result).ToUniversalTime().Month.Should().Be(month);
            ((DateTime)result).ToUniversalTime().Day.Should().Be(day);
            ((DateTime)result).ToUniversalTime().Hour.Should().Be(0);
            ((DateTime)result).ToUniversalTime().Minute.Should().Be(0);
            ((DateTime)result).ToUniversalTime().Second.Should().Be(0);
        }

        /// <summary>
        /// Tests whether the ReadYaml() method should return expected result or not.
        /// </summary>
        /// <param name="year">Year value.</param>
        /// <param name="month">Month value.</param>
        /// <param name="day">Day value.</param>
        /// <param name="format1">Designated date/time format 1.</param>
        /// <param name="format2">Designated date/time format 2.</param>
        /// <remarks>The converter instance uses its default parameter of UTC.</remarks>
        [Theory]
        [InlineData(2016, 12, 31, "yyyy-MM-dd", "yyyy/MM/dd HH:mm:ss")]
        public void Given_Yaml_WithSpecificCultureAndValidDateTimeFormat_ReadYaml_ShouldReturn_Result(int year, int month, int day, string format1, string format2)
        {
            var yaml = $"{year}-{month:00}-{day:00}";

            var parser = A.Fake<IParser>();
            A.CallTo(() => parser.Current).ReturnsLazily(() => new Scalar(yaml));

            var culture = new CultureInfo("ko-KR"); // Sample specific culture
            var converter = new DateTimeConverter(provider: culture, formats: new[] { format1, format2 });

            var result = converter.ReadYaml(parser, typeof(DateTime));

            result.Should().BeOfType<DateTime>();
            ((DateTime)result).Kind.Should().Be(DateTimeKind.Utc);
            ((DateTime)result).ToUniversalTime().Year.Should().Be(year);
            ((DateTime)result).ToUniversalTime().Month.Should().Be(month);
            ((DateTime)result).ToUniversalTime().Day.Should().Be(day);
            ((DateTime)result).ToUniversalTime().Hour.Should().Be(0);
            ((DateTime)result).ToUniversalTime().Minute.Should().Be(0);
            ((DateTime)result).ToUniversalTime().Second.Should().Be(0);
        }

        /// <summary>
        /// Tests whether the ReadYaml() method should return expected result or not.
        /// </summary>
        /// <param name="year">Year value.</param>
        /// <param name="month">Month value.</param>
        /// <param name="day">Day value.</param>
        /// <param name="format1">Designated date/time format 1.</param>
        /// <param name="format2">Designated date/time format 2.</param>
        /// <remarks>The converter instance uses its default parameter of UTC.</remarks>
        [Theory]
        [InlineData(2016, 12, 31, "yyyy-MM-dd", "yyyy/MM/dd HH:mm:ss")]
        public void Given_Yaml_WithValidDateTimeFormatAndUnspecified_ReadYaml_ShouldReturn_Result(int year, int month, int day, string format1, string format2)
        {
            var yaml = $"{year}-{month:00}-{day:00}";

            var parser = A.Fake<IParser>();
            A.CallTo(() => parser.Current).ReturnsLazily(() => new Scalar(yaml));

            var converter = new DateTimeConverter(DateTimeKind.Unspecified, formats: new[] { format1, format2 });

            var result = converter.ReadYaml(parser, typeof(DateTime));

            result.Should().BeOfType<DateTime>();
            ((DateTime)result).Kind.Should().Be(DateTimeKind.Utc);
            ((DateTime)result).ToUniversalTime().Year.Should().Be(year);
            ((DateTime)result).ToUniversalTime().Month.Should().Be(month);
            ((DateTime)result).ToUniversalTime().Day.Should().Be(day);
            ((DateTime)result).ToUniversalTime().Hour.Should().Be(0);
            ((DateTime)result).ToUniversalTime().Minute.Should().Be(0);
            ((DateTime)result).ToUniversalTime().Second.Should().Be(0);
        }

        /// <summary>
        /// Tests whether the ReadYaml() method should return expected result or not.
        /// </summary>
        /// <param name="year">Year value.</param>
        /// <param name="month">Month value.</param>
        /// <param name="day">Day value.</param>
        /// <param name="format1">Designated date/time format 1.</param>
        /// <param name="format2">Designated date/time format 2.</param>
        /// <remarks>The converter instance uses its default parameter of UTC.</remarks>
        [Theory]
        [InlineData(2016, 12, 31, "yyyy-MM-dd", "yyyy/MM/dd HH:mm:ss")]
        public void Given_Yaml_WithValidDateTimeFormatAndLocal_ReadYaml_ShouldReturn_Result(int year, int month, int day, string format1, string format2)
        {
            var yaml = $"{year}-{month:00}-{day:00}";

            var parser = A.Fake<IParser>();
            A.CallTo(() => parser.Current).ReturnsLazily(() => new Scalar(yaml));

            var converter = new DateTimeConverter(DateTimeKind.Local, formats: new[] { format1, format2 });

            var result = converter.ReadYaml(parser, typeof(DateTime));

            result.Should().BeOfType<DateTime>();
            ((DateTime)result).Kind.Should().Be(DateTimeKind.Local);
            ((DateTime)result).ToLocalTime().Year.Should().Be(year);
            ((DateTime)result).ToLocalTime().Month.Should().Be(month);
            ((DateTime)result).ToLocalTime().Day.Should().Be(day);
            ((DateTime)result).ToLocalTime().Hour.Should().Be(0);
            ((DateTime)result).ToLocalTime().Minute.Should().Be(0);
            ((DateTime)result).ToLocalTime().Second.Should().Be(0);
        }

        /// <summary>
        /// Tests whether the ReadYaml() method should return expected result or not.
        /// </summary>
        /// <param name="format">Date/Time format.</param>
        /// <param name="value">Date/Time value.</param>
        /// <remarks>Standard format "F" and "U" cannot be used at the same time as their string representations are identical to each other.</remarks>
        [Theory]
        [InlineData("d", "01/11/2017")]
        [InlineData("D", "Wednesday, 11 January 2017")]
        [InlineData("f", "Wednesday, 11 January 2017 02:36")]
        [InlineData("F", "Wednesday, 11 January 2017 02:36:16")]
        [InlineData("g", "01/11/2017 02:36")]
        [InlineData("G", "01/11/2017 02:36:16")]
        [InlineData("M", "January 11")]
        [InlineData("O", "2017-01-11T02:36:16.5065149+00:00")]
        [InlineData("R", "Wed, 11 Jan 2017 02:36:16 GMT")]
        [InlineData("s", "2017-01-11T02:36:16")]
        [InlineData("t", "02:36")]
        [InlineData("T", "02:36:16")]
        [InlineData("u", "2017-01-11 02:36:16Z")]
        [InlineData("U", "Wednesday, 11 January 2017 02:36:16")]
        [InlineData("Y", "2017 January")]
        public void Given_Yaml_WithTimeFormat_ReadYaml_ShouldReturn_Result(string format, string value)
        {
            var expected = DateTime.ParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime();
            var converter = new DateTimeConverter(formats: new[] { "d", "D", "f", "F", "g", "G", "M", "O", "R", "s", "t", "T", "u", "U", "Y" });

            var parser = A.Fake<IParser>();
            A.CallTo(() => parser.Current).ReturnsLazily(() => new Scalar(value));

            var result = converter.ReadYaml(parser, typeof(DateTime));

            result.Should().Be(expected);
        }

        /// <summary>
        /// Tests whether the ReadYaml() method should return expected result or not.
        /// </summary>
        /// <param name="format">Date/Time format.</param>
        /// <param name="locale">Locale value.</param>
        /// <param name="value">Date/Time value.</param>
        /// <remarks>Standard format "F" and "U" cannot be used at the same time as their string representations are identical to each other.</remarks>
        [Theory]
        [InlineData("d", "fr-FR", "13/01/2017")]
        [InlineData("D", "fr-FR", "vendredi 13 janvier 2017")]
        [InlineData("f", "fr-FR", "vendredi 13 janvier 2017 05:25")]
        [InlineData("F", "fr-FR", "vendredi 13 janvier 2017 05:25:08")]
        [InlineData("g", "fr-FR", "13/01/2017 05:25")]
        [InlineData("G", "fr-FR", "13/01/2017 05:25:08")]
        [InlineData("M", "fr-FR", "13 janvier")]
        [InlineData("O", "fr-FR", "2017-01-13T05:25:08.2003629+00:00")]
        [InlineData("R", "fr-FR", "Fri, 13 Jan 2017 05:25:08 GMT")]
        [InlineData("s", "fr-FR", "2017-01-13T05:25:08")]
        [InlineData("t", "fr-FR", "05:25")]
        [InlineData("T", "fr-FR", "05:25:08")]
        [InlineData("u", "fr-FR", "2017-01-13 05:25:08Z")]
        [InlineData("U", "fr-FR", "vendredi 13 janvier 2017 05:25:08")]
        [InlineData("Y", "fr-FR", "janvier 2017")]
        // [InlineData("d", "ko-KR", "2017-01-13")]
        [InlineData("D", "ko-KR", "2017년 1월 13일 금요일")]
        // [InlineData("f", "ko-KR", "2017년 1월 13일 금요일 오전 5:32")]
        // [InlineData("F", "ko-KR", "2017년 1월 13일 금요일 오전 5:32:06")]
        // [InlineData("g", "ko-KR", "2017-01-13 오전 5:32")]
        // [InlineData("G", "ko-KR", "2017-01-13 오전 5:32:06")]
        [InlineData("M", "ko-KR", "1월 13일")]
        [InlineData("O", "ko-KR", "2017-01-13T05:32:06.6865069+00:00")]
        [InlineData("R", "ko-KR", "Fri, 13 Jan 2017 05:32:06 GMT")]
        [InlineData("s", "ko-KR", "2017-01-13T05:32:06")]
        // [InlineData("t", "ko-KR", "오전 5:32")]
        // [InlineData("T", "ko-KR", "오전 5:32:06")]
        [InlineData("u", "ko-KR", "2017-01-13 05:32:06Z")]
        // [InlineData("U", "ko-KR", "2017년 1월 13일 금요일 오전 5:32:06")]
        [InlineData("Y", "ko-KR", "2017년 1월")]
        public void Given_Yaml_WithLocaleAndTimeFormat_ReadYaml_ShouldReturn_Result(string format, string locale, string value)
        {
            var culture = new CultureInfo(locale);

            var expected = default(DateTime);
            try
            {
                expected = DateTime.ParseExact(value, format, culture, DateTimeStyles.AssumeUniversal).ToUniversalTime();
            }
            catch (Exception ex)
            {
                var message = string.Format("Failed to parse the test argument to DateTime. The expected date format should look like this: '{0}'", DateTime.Now.ToString(format, culture));
                throw new Exception(message, ex);
            }

            var converter = new DateTimeConverter(provider: culture, formats: new[] { "d", "D", "f", "F", "g", "G", "M", "O", "R", "s", "t", "T", "u", "U", "Y" });

            var parser = A.Fake<IParser>();
            A.CallTo(() => parser.Current).ReturnsLazily(() => new Scalar(value));

            var result = converter.ReadYaml(parser, typeof(DateTime));

            result.Should().Be(expected);
        }

        /// <summary>
        /// Tests whether the ReadYaml() method should return expected result or not.
        /// </summary>
        /// <param name="format">Date/Time format.</param>
        /// <param name="value">Date/Time value.</param>
        /// <remarks>Standard format "F" and "U" cannot be used at the same time as their string representations are identical to each other.</remarks>
        [Theory]
        [InlineData("d", "01/11/2017")]
        [InlineData("D", "Wednesday, 11 January 2017")]
        [InlineData("f", "Wednesday, 11 January 2017 02:36")]
        [InlineData("F", "Wednesday, 11 January 2017 02:36:16")]
        [InlineData("g", "01/11/2017 02:36")]
        [InlineData("G", "01/11/2017 02:36:16")]
        [InlineData("M", "January 11")]
        [InlineData("O", "2017-01-11T02:36:16.5065149+00:00")]
        [InlineData("R", "Wed, 11 Jan 2017 02:36:16 GMT")]
        [InlineData("s", "2017-01-11T02:36:16")]
        [InlineData("t", "02:36")]
        [InlineData("T", "02:36:16")]
        [InlineData("u", "2017-01-11 02:36:16Z")]
        [InlineData("Y", "2017 January")]
        public void Given_Yaml_WithTimeFormatAndLocal1_ReadYaml_ShouldReturn_Result(string format, string value)
        {
            var expected = DateTime.ParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal).ToLocalTime();
            var converter = new DateTimeConverter(DateTimeKind.Local, formats: new[] { "d", "D", "f", "F", "g", "G", "M", "O", "R", "s", "t", "T", "u", "Y" });

            var parser = A.Fake<IParser>();
            A.CallTo(() => parser.Current).ReturnsLazily(() => new Scalar(value));

            var result = converter.ReadYaml(parser, typeof(DateTime));

            result.Should().Be(expected);
        }

        /// <summary>
        /// Tests whether the ReadYaml() method should return expected result or not.
        /// </summary>
        /// <param name="format">Date/Time format.</param>
        /// <param name="value">Date/Time value.</param>
        /// <remarks>Standard format "F" and "U" cannot be used at the same time as their string representations are identical to each other.</remarks>
        [Theory]
        [InlineData("d", "01/11/2017")]
        [InlineData("D", "Wednesday, 11 January 2017")]
        [InlineData("f", "Wednesday, 11 January 2017 02:36")]
        [InlineData("g", "01/11/2017 02:36")]
        [InlineData("G", "01/11/2017 02:36:16")]
        [InlineData("M", "January 11")]
        [InlineData("O", "2017-01-11T02:36:16.5065149+00:00")]
        [InlineData("R", "Wed, 11 Jan 2017 02:36:16 GMT")]
        [InlineData("s", "2017-01-11T02:36:16")]
        [InlineData("t", "02:36")]
        [InlineData("T", "02:36:16")]
        [InlineData("u", "2017-01-11 02:36:16Z")]
        [InlineData("U", "Wednesday, 11 January 2017 02:36:16")]
        [InlineData("Y", "2017 January")]
        public void Given_Yaml_WithTimeFormatAndLocal2_ReadYaml_ShouldReturn_Result(string format, string value)
        {
            var expected = DateTime.ParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal).ToLocalTime();
            var converter = new DateTimeConverter(DateTimeKind.Local, formats: new[] { "d", "D", "f", "g", "G", "M", "O", "R", "s", "t", "T", "u", "U", "Y" });

            var parser = A.Fake<IParser>();
            A.CallTo(() => parser.Current).ReturnsLazily(() => new Scalar(value));

            var result = converter.ReadYaml(parser, typeof(DateTime));

            result.Should().Be(expected);
        }

        /// <summary>
        /// Tests whether the WriteYaml method should return expected result or not.
        /// </summary>
        /// <param name="year">Year value.</param>
        /// <param name="month">Month value.</param>
        /// <param name="day">Day value.</param>
        /// <param name="hour">Hour value.</param>
        /// <param name="minute">Minute value.</param>
        /// <param name="second">Second value.</param>
        /// <param name="kind"><see cref="DateTimeKind"/> value</param>
        /// <remarks>The converter instance uses its default parameters of "G" and UTC.</remarks>
        [Theory]
        [InlineData(2016, 12, 31, 3, 0, 0, DateTimeKind.Utc)]
        public void Given_Values_WriteYaml_ShouldReturn_Result(int year, int month, int day, int hour, int minute, int second, DateTimeKind kind)
        {
            var dt = new DateTime(year, month, day, hour, minute, second, kind);
            var formatted = dt.ToString("G", CultureInfo.InvariantCulture);
            var obj = new TestObject() { DateTime = dt };

            var builder = new SerializerBuilder();
            builder.WithNamingConvention(CamelCaseNamingConvention.Instance);
            builder.WithTypeConverter(new DateTimeConverter());

            var serialiser = builder.Build();

            var serialised = serialiser.Serialize(obj);

            serialised.Should().ContainEquivalentOf($"datetime: {formatted}");
        }

        /// <summary>
        /// Tests whether the WriteYaml method should return expected result or not.
        /// </summary>
        /// <param name="year">Year value.</param>
        /// <param name="month">Month value.</param>
        /// <param name="day">Day value.</param>
        /// <param name="hour">Hour value.</param>
        /// <param name="minute">Minute value.</param>
        /// <param name="second">Second value.</param>
        /// <param name="kind"><see cref="DateTimeKind"/> value</param>
        /// <param name="locale">Locale value.</param>
        /// <remarks>The converter instance uses its default parameters of "G" and UTC.</remarks>
        [Theory]
        [InlineData(2016, 12, 31, 3, 0, 0, DateTimeKind.Utc, "es-ES")]
        [InlineData(2016, 12, 31, 3, 0, 0, DateTimeKind.Utc, "ko-KR")]
        public void Given_Values_WithLocale_WriteYaml_ShouldReturn_Result(int year, int month, int day, int hour, int minute, int second, DateTimeKind kind, string locale)
        {
            var dt = new DateTime(year, month, day, hour, minute, second, kind);
            var culture = new CultureInfo(locale);
            var formatted = dt.ToString("G", culture);
            var obj = new TestObject() { DateTime = dt };

            var builder = new SerializerBuilder();
            builder.WithNamingConvention(CamelCaseNamingConvention.Instance);
            builder.WithTypeConverter(new DateTimeConverter(provider: culture));

            var serialiser = builder.Build();

            var serialised = serialiser.Serialize(obj);

            serialised.Should().ContainEquivalentOf($"datetime: {formatted}");
        }

        /// <summary>
        /// Tests whether the WriteYaml method should return expected result or not.
        /// </summary>
        /// <param name="year">Year value.</param>
        /// <param name="month">Month value.</param>
        /// <param name="day">Day value.</param>
        /// <param name="hour">Hour value.</param>
        /// <param name="minute">Minute value.</param>
        /// <param name="second">Second value.</param>
        /// <param name="kind"><see cref="DateTimeKind"/> value</param>
        /// <remarks>The converter instance uses its default parameters of "G" and UTC.</remarks>
        [Theory]
        [InlineData(2016, 12, 31, 3, 0, 0, DateTimeKind.Utc)]
        public void Given_Values_WithFormats_WriteYaml_ShouldReturn_Result_WithFirstFormat(int year, int month, int day, int hour, int minute, int second, DateTimeKind kind)
        {
            var dt = new DateTime(year, month, day, hour, minute, second, kind);
            var format = "yyyy-MM-dd HH:mm:ss";
            var formatted = dt.ToString(format, CultureInfo.InvariantCulture);
            var obj = new TestObject() { DateTime = dt };

            var builder = new SerializerBuilder();
            builder.WithNamingConvention(CamelCaseNamingConvention.Instance);
            builder.WithTypeConverter(new DateTimeConverter(kind, formats: new[] { format, "G" }));

            var serialiser = builder.Build();

            var serialised = serialiser.Serialize(obj);

            serialised.Should().ContainEquivalentOf($"datetime: {formatted}");
        }
    }

    /// <summary>
    /// This represents the test object entity.
    /// </summary>
    public class TestObject
    {
        /// <summary>
        /// Gets or sets the <see cref="System.DateTime"/> value.
        /// </summary>
        public DateTime DateTime { get; set; }
    }
}
