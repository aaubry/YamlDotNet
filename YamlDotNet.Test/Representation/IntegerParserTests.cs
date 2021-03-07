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
    public class IntegerParserTests
    {
        [Theory]
        [InlineData("-0b1010", -10L)]
        [InlineData("0b1001", 9L)]
        [InlineData("+0b1001", 9L)]
        [InlineData("0b0", 0L)]
        [InlineData("-0b0", 0L)]
        [InlineData("0b______0", 0L)]
        [InlineData("0b_0100_0111_1010_1111_0110_1011_0010_1110_1100_0000_0011_0100_0010_1111_1010_1011_", 5165465146154561451L)]
        [InlineData("0b0111111111111111111111111111111111111111111111111111111111111111", long.MaxValue)]
        [InlineData("-0b1000000000000000000000000000000000000000000000000000000000000000", long.MinValue)]
        public void ParseBase2_is_correct(string value, long expected)
        {
            var actual = IntegerParser.ParseBase2(value);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("0b1000000000000000000000000000000000000000000000000000000000000000")]
        [InlineData("-0b1000000000000000000000000000000000000000000000000000000000000001")]
        public void ParseBase2_detects_overflow(string value)
        {
            Assert.Throws<OverflowException>(() => IntegerParser.ParseBase2(value));
        }

        [Theory]
        [InlineData("-0644", -420L)]
        [InlineData("0644", 420L)]
        [InlineData("+0644", 420L)]
        [InlineData("00", 0L)]
        [InlineData("-00", 0L)]
        [InlineData("0______0", 0L)]
        [InlineData("0_436_573_262_730_015_027_653", 5165465146154561451L)]
        [InlineData("0777777777777777777777", long.MaxValue)]
        [InlineData("-01000000000000000000000", long.MinValue)]
        public void ParseBase8_is_correct(string value, long expected)
        {
            var actual = IntegerParser.ParseBase8(value);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("01777777777777777777777")]
        [InlineData("-01000000000000000000001")]
        public void ParseBase8_detects_overflow(string value)
        {
            Assert.Throws<OverflowException>(() => IntegerParser.ParseBase8(value));
        }

        [Theory]
        [InlineData("0o644", 420UL)]
        [InlineData("0o0", 0UL)]
        [InlineData("0o436573262730015027653", 5165465146154561451UL)]
        [InlineData("0o1777777777777777777777", ulong.MaxValue)]
        public void ParseBase8Unsigned_is_correct(string value, ulong expected)
        {
            var actual = IntegerParser.ParseBase8Unsigned(value);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("0o2000000000000000000000")]
        public void ParseBase8Unsigned_detects_overflow(string value)
        {
            Assert.Throws<OverflowException>(() => IntegerParser.ParseBase8Unsigned(value));
        }

        [Theory]
        [InlineData("-123", -123L)]
        [InlineData("123", 123L)]
        [InlineData("+123", 123L)]
        [InlineData("0", 0L)]
        [InlineData("-0", 0L)]
        [InlineData("______0", 0L)]
        [InlineData("516_546_514_615_456_145_1", 5165465146154561451L)]
        [InlineData("9223372036854775807", long.MaxValue)]
        [InlineData("-9223372036854775808", long.MinValue)]
        public void ParseBase10_is_correct(string value, long expected)
        {
            var actual = IntegerParser.ParseBase10(value);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("9223372036854775808")]
        [InlineData("-9223372036854775809")]
        public void ParseBase10_detects_overflow(string value)
        {
            Assert.Throws<OverflowException>(() => IntegerParser.ParseBase10(value));
        }

        [Theory]
        [InlineData("-0xf4C", -3916L)]
        [InlineData("0xf4C", 3916L)]
        [InlineData("+0xf4C", 3916L)]
        [InlineData("0x0", 0L)]
        [InlineData("-0x0", 0L)]
        [InlineData("0x______0", 0L)]
        [InlineData("0x47_AF_6B_2E_C0_34_2F_AB", 5165465146154561451L)]
        [InlineData("0x7FFFFFFFFFFFFFFF", long.MaxValue)]
        [InlineData("-0x8000000000000000", long.MinValue)]
        public void ParseBase16_is_correct(string value, long expected)
        {
            var actual = IntegerParser.ParseBase16(value);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("0x8000000000000000")]
        [InlineData("-0x8000000000000001")]
        public void ParseBase16_detects_overflow(string value)
        {
            Assert.Throws<OverflowException>(() => IntegerParser.ParseBase16(value));
        }

        [Theory]
        [InlineData("0xf4C", 3916UL)]
        [InlineData("0x0", 0UL)]
        [InlineData("0x47AF6B2EC0342FAB", 5165465146154561451UL)]
        [InlineData("0xFFFFFFFFFFFFFFFF", ulong.MaxValue)]
        public void ParseBase16Unsigned_is_correct(string value, ulong expected)
        {
            var actual = IntegerParser.ParseBase16Unsigned(value);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("0x10000000000000000")]
        public void ParseBase16Unsigned_detects_overflow(string value)
        {
            Assert.Throws<OverflowException>(() => IntegerParser.ParseBase16Unsigned(value));
        }

        [Theory]
        [InlineData("-14:10:53", -51053L)]
        [InlineData("14:10:53", 51053L)]
        [InlineData("+14:10:53", 51053L)]
        [InlineData("0:0", 0L)]
        [InlineData("-0:0", 0L)]
        [InlineData("____0:0", 0L)]
        [InlineData("6_642_830_692:4:16:18:10:51", 5165465146154561451L)]
        [InlineData("915:13:34:32:31:55:20:15:30:7", long.MaxValue)]
        [InlineData("-915:13:34:32:31:55:20:15:30:8", long.MinValue)]
        public void ParseBase60_is_correct(string value, long expected)
        {
            var actual = IntegerParser.ParseBase60(value);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("915:13:34:32:31:55:20:15:30:8")]
        [InlineData("-915:13:34:32:31:55:20:15:30:9")]
        public void ParseBase60_detects_overflow(string value)
        {
            Assert.Throws<OverflowException>(() => IntegerParser.ParseBase60(value));
        }
    }
}