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
using Xunit;
using YamlDotNet.Representation.Schemas;

namespace YamlDotNet.Test.Representation
{
    public class IntegerFormatterTests
    {
        [Theory]
        [InlineData(123, "123")]
        [InlineData(-123, "-123")]
        [InlineData((short)123, "123")]
        [InlineData(123L, "123")]
        [InlineData(0UL, "0")]
        [InlineData(ulong.MaxValue, "18446744073709551615")]
        [InlineData(long.MaxValue, "9223372036854775807")]
        [InlineData(long.MinValue, "-9223372036854775808")]
        public void FormatBase10_is_correct(object value, string expected)
        {
            var actual = IntegerFormatter.FormatBase10(value);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(123, "0b1111011")]
        [InlineData(-123, "-0b1111011")]
        [InlineData((short)123, "0b1111011")]
        [InlineData(123L, "0b1111011")]
        [InlineData(0UL, "0b0")]
        [InlineData(long.MaxValue, "0b111111111111111111111111111111111111111111111111111111111111111")]
        [InlineData(long.MinValue, "-0b1000000000000000000000000000000000000000000000000000000000000000")]
        [InlineData(0x7F000000000000FFL, "0b111111100000000000000000000000000000000000000000000000011111111")]
        public void FormatBase2Signed_is_correct(object value, string expected)
        {
            var actual = IntegerFormatter.FormatBase2Signed(value);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(123, "0o173")]
        [InlineData((short)123, "0o173")]
        [InlineData(123L, "0o173")]
        [InlineData(0UL, "0o0")]
        [InlineData(ulong.MaxValue, "0o1777777777777777777777")]
        [InlineData(long.MaxValue, "0o777777777777777777777")]
        [InlineData(0xFF000000000000FFUL, "0o1774000000000000000377")]
        public void FormatBase8Unsigned_is_correct(object value, string expected)
        {
            var actual = IntegerFormatter.FormatBase8Unsigned(value);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(123, "0o173")]
        [InlineData(-123, "-0o173")]
        [InlineData((short)123, "0o173")]
        [InlineData(123L, "0o173")]
        [InlineData(0UL, "0o0")]
        [InlineData(long.MaxValue, "0o777777777777777777777")]
        [InlineData(long.MinValue, "-0o1000000000000000000000")]
        [InlineData(0x7F000000000000FFL, "0o774000000000000000377")]
        public void FormatBase8Signed_is_correct(object value, string expected)
        {
            var actual = IntegerFormatter.FormatBase8Signed(value);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(123, "0x7B")]
        [InlineData((short)123, "0x7B")]
        [InlineData(123L, "0x7B")]
        [InlineData(0UL, "0x0")]
        [InlineData(ulong.MaxValue, "0xFFFFFFFFFFFFFFFF")]
        [InlineData(long.MaxValue, "0x7FFFFFFFFFFFFFFF")]
        [InlineData(0xFF000000000000FFUL, "0xFF000000000000FF")]
        public void FormatBase16Unsigned_is_correct(object value, string expected)
        {
            var actual = IntegerFormatter.FormatBase16Unsigned(value);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(123, "0x7B")]
        [InlineData(-123, "-0x7B")]
        [InlineData((short)123, "0x7B")]
        [InlineData(123L, "0x7B")]
        [InlineData(0UL, "0x0")]
        [InlineData(long.MaxValue, "0x7FFFFFFFFFFFFFFF")]
        [InlineData(long.MinValue, "-0x8000000000000000")]
        [InlineData(0x7F000000000000FFU, "0x7F000000000000FF")]
        public void FormatBase16Signed_is_correct(object value, string expected)
        {
            var actual = IntegerFormatter.FormatBase16Signed(value);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(123, "2:3")]
        [InlineData(-123, "-2:3")]
        [InlineData((short)123, "2:3")]
        [InlineData(123L, "2:3")]
        [InlineData(0UL, "0")]
        [InlineData(long.MaxValue, "15:15:13:34:32:31:55:20:15:30:7")]
        [InlineData(long.MinValue, "-15:15:13:34:32:31:55:20:15:30:8")]
        public void FormatBase60Signed_is_correct(object value, string expected)
        {
            var actual = IntegerFormatter.FormatBase60Signed(value);
            Assert.Equal(expected, actual);
        }
    }
}