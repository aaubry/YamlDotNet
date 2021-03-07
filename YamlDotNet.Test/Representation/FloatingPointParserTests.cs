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

using Xunit;
using YamlDotNet.Representation.Schemas;

namespace YamlDotNet.Test.Representation
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
}