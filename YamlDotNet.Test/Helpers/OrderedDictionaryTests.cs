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

using System.Collections.Generic;
using System.Linq;
using Xunit;
using YamlDotNet.Helpers;

namespace YamlDotNet.Test.Helpers
{
    public class OrderedDictionaryTests
    {
        [Fact]
        public void OrderOfElementsIsMainted()
        {
            var dict = (IDictionary<int, string>)new OrderedDictionary<int, string>
            {
                { 3, "First" },
                { 2, "Temporary" },
                { 1, "Second" },
            };
            dict.Remove(2);
            dict.Add(4, "Inserted");
            dict[4] = "Third";

            Assert.Equal(3, dict.Count);
            Assert.Equal(new KeyValuePair<int, string>(3, "First"), dict.First());
            Assert.Equal(new KeyValuePair<int, string>(1, "Second"), dict.Skip(1).First());
            Assert.Equal(new KeyValuePair<int, string>(4, "Third"), dict.Skip(2).First());
            Assert.Equal(new[] { 3, 1, 4 }, dict.Keys.ToArray());
            Assert.Equal(new[] { "First", "Second", "Third" }, dict.Values.ToArray());
        }

        [Fact]
        public void KeysContainsWorks()
        {
            var dict = new OrderedDictionary<int, string>
            {
                { 3, "First item" },
                { 2, "Second item" },
                { 1, "Third item" },
            };

            Assert.False(dict.Keys.Contains(0));
            Assert.True(dict.Keys.Contains(1));
            Assert.True(dict.Keys.Contains(2));
            Assert.True(dict.Keys.Contains(3));
            Assert.False(dict.Keys.Contains(4));
        }

        [Fact]
        public void ValuesContainsWorks()
        {
            var dict = new OrderedDictionary<int, string>
            {
                { 3, "First item" },
                { 2, "Second item" },
                { 1, "Third item" },
            };

            Assert.False(dict.Values.Contains(null));
            Assert.True(dict.Values.Contains("First item"));
            Assert.True(dict.Values.Contains("Second item"));
            Assert.True(dict.Values.Contains("Third item"));
            Assert.False(dict.Values.Contains("Fourth item"));
        }

        [Fact]
        public void CanInsertAndRemoveAtIndex()
        {
            var dict = new OrderedDictionary<int, string>
            {
                { 3, "First" },
                { 2, "Temporary" },
                { 1, "Second" },
            };
            dict.RemoveAt(1);
            dict.Insert(0, 4, "Zero");

            Assert.Equal(3, dict.Count);
            Assert.Equal(new KeyValuePair<int, string>(4, "Zero"), dict.First());
            Assert.Equal(new KeyValuePair<int, string>(3, "First"), dict.Skip(1).First());
            Assert.Equal(new KeyValuePair<int, string>(1, "Second"), dict.Skip(2).First());
            Assert.Equal(new KeyValuePair<int, string>(4, "Zero"), dict[0]);
            Assert.Equal(new KeyValuePair<int, string>(3, "First"), dict[1]);
            Assert.Equal(new KeyValuePair<int, string>(1, "Second"), dict[2]);
            Assert.Equal(new[] { 4, 3, 1 }, dict.Keys.ToArray());
            Assert.Equal(new[] { "Zero", "First", "Second" }, dict.Values.ToArray());
        }
    }
}
