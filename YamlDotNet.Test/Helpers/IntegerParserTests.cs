using System;
using Xunit;
using YamlDotNet.Helpers;

namespace YamlDotNet.Test.Helpers
{
    public class FloatingPointParserTests
    {
        [Theory]
        [InlineData("-123.45", -123.45)]
        [InlineData("123.45", 123.45)]
        [InlineData("+123.45", 123.45)]
        [InlineData(".45", 0.45)]
        [InlineData("0", 0.0)]
        [InlineData("-0", 0.0)]
        [InlineData("+0", 0.0)]
        [InlineData("5165465146154.561451", 5165465146154.561451)]
        [InlineData("1.7976931348623157E+308", double.MaxValue)]
        [InlineData("-1.7976931348623157E+308", double.MinValue)]
        public void ParseBase10Unseparated_is_correct(string value, double expected)
        {
            var actual = FloatingPointParser.ParseBase10Unseparated(value);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("-12_3.45", -123.45)]
        [InlineData("1_23.45", 123.45)]
        [InlineData("+12_3.45", 123.45)]
        [InlineData("_0_", 0.0)]
        [InlineData("-0_", 0.0)]
        [InlineData("+______0", 0.0)]
        [InlineData("_51_654_6_51_461_54._56_14_51_", 5165465146154.561451)]
        [InlineData("1.79769_31_348623157E+308", double.MaxValue)]
        [InlineData("-1.797_69313_48623157E+308", double.MinValue)]
        public void ParseBase10Separated_is_correct(string value, double expected)
        {
            var actual = FloatingPointParser.ParseBase10Separated(value);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("-14:10:53._123_", -51053.123)]
        [InlineData("14:10:53.12_3", 51053.123)]
        [InlineData("+14:10:53.1_23", 51053.123)]
        [InlineData("0:0.", 0.0)]
        [InlineData("-0:0.", 0.0)]
        [InlineData("____0:0.0", 0.0)]
        [InlineData("0:0.123", 0.123)]
        [InlineData("-0:0.123", -0.123)]
        [InlineData("6_642_830_692:4:16:18:10:51.1_2_3", 5165465146154561451.123)]
        public void ParseBase60_is_correct(string value, double expected)
        {
            var actual = FloatingPointParser.ParseBase60(value);
            Assert.Equal(expected, actual);
        }
    }

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