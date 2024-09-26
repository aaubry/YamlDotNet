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

namespace YamlDotNet.Test
{
    public class YamlTests
    {
        private const string SingleLine = "object:";
        private const string LeadingBlankLines = @"


object:";
        private const string LeadingBlankLinesWithWhitespace = @"    
    
    
object:";
        private const string TrailingBlankLines = @"object:


";
        private const string TrailingBlankLinesWithWhitespace = @"object:
    
    
";

        private const string Lines = @"this:
that:
theOtherThing:";
        private const string IndentedLines = @"    this:
    that:
    theOtherThing:";

        private const string NestedLines = @"Map1:
    Map2:
        - entry 1
        - entry 2
        - entry 3";
        private const string IndentedNestedLines = @"    Map1:
        Map2:
            - entry 1
            - entry 2
            - entry 3";

        private const string SomeBlankLines = @"this:

that:


theOtherThing:";
        private const string SomeBlankLinesWithWhitespace = @"this:
    
that:
    
    
theOtherThing:";

        [Theory]
        [InlineData(SingleLine, SingleLine)]
        [InlineData(LeadingBlankLines, SingleLine)]
        [InlineData(LeadingBlankLinesWithWhitespace, SingleLine)]
        [InlineData(TrailingBlankLines, SingleLine)]
        [InlineData(TrailingBlankLinesWithWhitespace, SingleLine)]
        [InlineData(Lines, Lines)]
        [InlineData(IndentedLines, Lines)]
        [InlineData(NestedLines, NestedLines)]
        [InlineData(IndentedNestedLines, NestedLines)]
        [InlineData(SomeBlankLines, SomeBlankLines)]
        [InlineData(SomeBlankLinesWithWhitespace, SomeBlankLines)]
        public void TextProducesExpectedOutput(string text, string expectedText)
        {
            expectedText = expectedText.NormalizeNewLines();
            var result = Yaml.Text(text);

            result.NormalizeNewLines().Should().Be(expectedText);
        }

        [Fact]
        public void TextThrowsArgumentOutOfRangeExceptionForInsuffientIndentation()
        {
            const string BadlyIndentedLines = @"    this:
  that:
    theOtherThing:";
            var expectedMessage =
#if NETFRAMEWORK
                "Incorrectly indented line '  that:', #1." + Environment.NewLine + "Parameter name: yamlText";
#else
                "Incorrectly indented line '  that:', #1. (Parameter 'yamlText')";
#endif
            Action act = () => Yaml.Text(BadlyIndentedLines);

            act.Should().ThrowExactly<ArgumentException>().WithMessage(expectedMessage);
        }
    }
}
