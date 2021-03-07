//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) Antoine Aubry and contributors

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace YamlDotNet.Representation.Schemas
{
    internal static class TimestampParser
    {
        public static readonly Regex TimestampPattern = new Regex(@"^(?<date>\d{4}-\d{1,2}-\d{1,2})(([Tt]|\s+)(?<time>\d{1,2}:\d{1,2}:\d{1,2})(?<fraction>\.\d*)?\s*(Z|(?<offset>[+-]\d{1,2}(:\d{2})?))?)?$", StandardRegexOptions.Compiled | RegexOptions.ExplicitCapture);

        private const string ShortDateFormat = "yyyy-M-d";
        private const string LongDateFormatUtc = "yyyy-M-dTH:m:s.FFFFFFF";

        private static readonly string[] LongDateFormatsWithOffsets = new[]
        {
            LongDateFormatUtc + "zzz",
            LongDateFormatUtc + "zz",
            LongDateFormatUtc + "z",
        };

        public static DateTimeOffset Parse(string value)
        {
            var match = TimestampPattern.Match(value);
            if (!match.Success)
            {
                throw new FormatException($"Invalid timestamp format: {value}");
            }

            var date = match.Groups["date"].Value;
            var timeGroup = match.Groups["time"];
            if (timeGroup.Success)
            {
                var time = timeGroup.Value;
                var fraction = match.Groups["fraction"].Value;
                var offsetGroup = match.Groups["offset"];
                if (offsetGroup.Success)
                {
                    var normalizedValue = string.Concat(date, "T", time, fraction, offsetGroup.Value);
                    return DateTimeOffset.ParseExact(normalizedValue, LongDateFormatsWithOffsets, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                }
                else
                {
                    var normalizedValue = string.Concat(date, "T", time, fraction);
                    return DateTimeOffset.ParseExact(normalizedValue, LongDateFormatUtc, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                }
            }
            else
            {
                return DateTimeOffset.ParseExact(date, ShortDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            }
        }

        public static string Represent(object? date)
        {
            return date switch
            {
                DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK"),
                DateTime dateTime => dateTime.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK"),
                _ => throw new ArgumentException("The value should be either DateTime or DateTimeOffset", nameof(date)),
            };
        }
    }
}