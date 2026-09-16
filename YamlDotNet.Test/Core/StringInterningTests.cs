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
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Test.Core
{
    public class StringInterningTests
    {
        [Fact]
        public void AnchorNameDoesNotInternUniqueValues()
        {
            var value = UniqueValue("anchor");
            Assert.Null(string.IsInterned(value));

            var anchor = new AnchorName(value);

            Assert.Equal(value, anchor.Value);
            Assert.Null(string.IsInterned(value));
        }

        [Fact]
        public void TagNameDoesNotInternUniqueValues()
        {
            var value = "!" + UniqueValue("tag");
            Assert.Null(string.IsInterned(value));

            var tag = new TagName(value);

            Assert.Equal(value, tag.Value);
            Assert.Null(string.IsInterned(value));
        }

        [Fact]
        public void ScalarKeyDoesNotInternUniqueValues()
        {
            var value = UniqueValue("key");
            Assert.Null(string.IsInterned(value));

            var scalar = new Scalar(
                AnchorName.Empty,
                TagName.Empty,
                value,
                ScalarStyle.Plain,
                isPlainImplicit: true,
                isQuotedImplicit: false,
                Mark.Empty,
                Mark.Empty,
                isKey: true);

            Assert.True(scalar.IsKey);
            Assert.Equal(value, scalar.Value);
            Assert.Null(string.IsInterned(value));
        }

        private static string UniqueValue(string prefix)
        {
            return string.Concat(prefix, "_", Guid.NewGuid().ToString("N"));
        }
    }
}
