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
using Xunit;
using YamlDotNet.Representation.Schemas;

namespace YamlDotNet.Test.Representation
{
    public class TimestampParserTests
    {
        [Theory]
        [InlineData("2002-1-4", "2002-01-04T00:00:00.0000000Z")]
        [InlineData("2002-12-14", "2002-12-14T00:00:00.0000000Z")]
        [InlineData("2001-12-15T02:59:43.1Z", "2001-12-15T02:59:43.1000000Z")]
        [InlineData("2001-12-14t21:59:43.10-05:00", "2001-12-14T21:59:43.1000000-05:00")]
        [InlineData("2001-12-14 21:59:43.10 -5", "2001-12-14T21:59:43.1000000-05:00")]
        [InlineData("2001-12-15 2:59:43.10", "2001-12-15T02:59:43.1000000Z")]
        public void Parse_is_correct(string value, string expectedAsText)
        {
            var expected = DateTimeOffset.ParseExact(expectedAsText, "o", CultureInfo.InvariantCulture);
            var actual = TimestampParser.Parse(value);
            Assert.Equal(expected, actual);
        }
    }
}