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

using System.Text;
using Xunit;
using YamlDotNet.Helpers;

namespace YamlDotNet.Test.Helpers
{
    public class JsonHelperTests
    {
        [Fact]
        public void IsJsonLiteralReturnsTrueForNull()
        {
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("null")));
        }

        [Fact]
        public void IsJsonLiteralReturnsTrueForBooleans()
        {
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("true")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("false")));
        }

        [Fact]
        public void IsJsonLiteralReturnsTrueForNumbers()
        {
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("0")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("-0")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("0.0")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("-0.0")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("0e0")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("0e+0")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("0e-0")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("-0e0")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("-0e+0")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("-0e-0")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("0.0e0")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("0.0e+0")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("0.0e-0")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("-0.0e0")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("-0.0e+0")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("-0.0e-0")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("123")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("-123")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("123.45")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("-123.45")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("123e45")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("123e+45")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("123e-45")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("-123e45")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("-123e+45")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("-123e-45")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("123.45e67")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("123.45e+67")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("123.45e-67")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("-123.45e67")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("-123.45e+67")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("-123.45e-67")));
            Assert.True(JsonHelper.IsJsonLiteral(new StringBuilder("-123.45E+67")));
        }

        [Fact]
        public void IsJsonLiteralReturnsFalseForNonLiteralValues()
        {
            Assert.False(JsonHelper.IsJsonLiteral(new StringBuilder("foo")));

            Assert.False(JsonHelper.IsJsonLiteral(new StringBuilder("Null")));
            Assert.False(JsonHelper.IsJsonLiteral(new StringBuilder("True")));
            Assert.False(JsonHelper.IsJsonLiteral(new StringBuilder("False")));

            Assert.False(JsonHelper.IsJsonLiteral(new StringBuilder("NULL")));
            Assert.False(JsonHelper.IsJsonLiteral(new StringBuilder("TRUE")));
            Assert.False(JsonHelper.IsJsonLiteral(new StringBuilder("FALSE")));

            Assert.False(JsonHelper.IsJsonLiteral(new StringBuilder("-")));
            Assert.False(JsonHelper.IsJsonLiteral(new StringBuilder("+")));
            Assert.False(JsonHelper.IsJsonLiteral(new StringBuilder("+123")));
            Assert.False(JsonHelper.IsJsonLiteral(new StringBuilder("01")));
            Assert.False(JsonHelper.IsJsonLiteral(new StringBuilder("-01")));
            Assert.False(JsonHelper.IsJsonLiteral(new StringBuilder("123.45.67")));
            Assert.False(JsonHelper.IsJsonLiteral(new StringBuilder("123.")));
            Assert.False(JsonHelper.IsJsonLiteral(new StringBuilder(".123")));
            Assert.False(JsonHelper.IsJsonLiteral(new StringBuilder(".")));
            Assert.False(JsonHelper.IsJsonLiteral(new StringBuilder("123.45e67e8")));
            Assert.False(JsonHelper.IsJsonLiteral(new StringBuilder("123.45e67.8")));
            Assert.False(JsonHelper.IsJsonLiteral(new StringBuilder("123e67.8")));
            Assert.False(JsonHelper.IsJsonLiteral(new StringBuilder("123.45e")));
            Assert.False(JsonHelper.IsJsonLiteral(new StringBuilder("123.45e+")));
            Assert.False(JsonHelper.IsJsonLiteral(new StringBuilder("123.45e-")));
            Assert.False(JsonHelper.IsJsonLiteral(new StringBuilder("e")));
            Assert.False(JsonHelper.IsJsonLiteral(new StringBuilder("0xcoffee")));
            Assert.False(JsonHelper.IsJsonLiteral(new StringBuilder("0b101010")));
        }
    }
}
