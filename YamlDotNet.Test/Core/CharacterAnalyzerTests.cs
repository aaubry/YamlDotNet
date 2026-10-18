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

using System.IO;
using FluentAssertions;
using Xunit;
using YamlDotNet.Core;

namespace YamlDotNet.Test.Core
{
    public class CharacterAnalyzerTests
    {
        [Theory]
        [InlineData(' ', true, false)]
        [InlineData('\t', true, false)]
        [InlineData('\r', false, true)]
        [InlineData('\n', false, true)]
        [InlineData('\x85', false, true)]
        [InlineData('\x2028', false, true)]
        [InlineData('\x2029', false, true)]
        [InlineData('a', false, false)]
        [InlineData(':', false, false)]
        [InlineData('\x0B', false, false)]
        [InlineData('\x0C', false, false)]
        [InlineData('\xA0', false, false)]
        [InlineData('\x3000', false, false)]
        public void ShouldClassifyWhitespaceAndLineBreaks(char character, bool isWhite, bool isBreak)
        {
            var analyzer = CreateAnalyzer(new string(character, 1));

            analyzer.IsWhite().Should().Be(isWhite);
            analyzer.IsBreak().Should().Be(isBreak);
            analyzer.IsBreakOrZero().Should().Be(isBreak);
            analyzer.IsWhiteBreakOrZero().Should().Be(isWhite || isBreak);
        }

        [Fact]
        public void ShouldTreatNulAsZero()
        {
            var analyzer = CreateAnalyzer("\0");

            analyzer.IsWhite().Should().BeFalse();
            analyzer.IsBreak().Should().BeFalse();
            analyzer.IsBreakOrZero().Should().BeTrue();
            analyzer.IsWhiteBreakOrZero().Should().BeTrue();
        }

        [Fact]
        public void ShouldTreatEndOfInputAsZero()
        {
            var analyzer = CreateAnalyzer("a");

            analyzer.IsWhite(1).Should().BeFalse();
            analyzer.IsBreak(1).Should().BeFalse();
            analyzer.IsBreakOrZero(1).Should().BeTrue();
            analyzer.IsWhiteBreakOrZero(1).Should().BeTrue();
        }

        [Fact]
        public void ShouldClassifyCharactersAtAnOffset()
        {
            var analyzer = CreateAnalyzer("a \t\r\n\x2028b");

            analyzer.IsWhiteBreakOrZero(0).Should().BeFalse();
            analyzer.IsWhite(1).Should().BeTrue();
            analyzer.IsWhite(2).Should().BeTrue();
            analyzer.IsBreak(3).Should().BeTrue();
            analyzer.IsBreak(4).Should().BeTrue();
            analyzer.IsBreak(5).Should().BeTrue();
            analyzer.IsWhiteBreakOrZero(6).Should().BeFalse();
        }

        private static CharacterAnalyzer<LookAheadBuffer> CreateAnalyzer(string input)
        {
            return new CharacterAnalyzer<LookAheadBuffer>(new LookAheadBuffer(new StringReader(input), 8));
        }
    }
}
