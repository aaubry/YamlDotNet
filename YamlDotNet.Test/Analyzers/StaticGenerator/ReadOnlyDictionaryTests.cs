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

using System.Collections;
using System.Collections.Generic;
using Xunit;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test.Analyzers.StaticGenerator
{
    public class ReadOnlyDictionaryTests
    {
        [Fact]
        public void TypeImplementingOnlyIReadOnlyDictionarySerializesAsMapping()
        {
            var serializer = new StaticSerializerBuilder(new StaticContext()).Build();
            var dictionary = new ReadOnlyOnlyDictionary(new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" },
            });

            var actual = serializer.Serialize(dictionary);

            Assert.Equal("key1: value1\r\nkey2: value2\r\n".NormalizeNewLines(), actual.NormalizeNewLines());
        }

        [Fact]
        public void TypeImplementingOnlyIReadOnlyDictionarySerializesAsMappingWhenNested()
        {
            var serializer = new StaticSerializerBuilder(new StaticContext()).Build();
            var outer = new ReadOnlyDictionaryHolder
            {
                Items = new ReadOnlyOnlyDictionary(new Dictionary<string, string> { { "key1", "value1" } }),
            };

            var actual = serializer.Serialize(outer);

            Assert.Equal("Items:\r\n  key1: value1\r\n".NormalizeNewLines(), actual.NormalizeNewLines());
        }
    }

    [YamlSerializable]
    public class ReadOnlyDictionaryHolder
    {
        public ReadOnlyOnlyDictionary Items { get; set; }
    }

    [YamlSerializable]
    public class ReadOnlyOnlyDictionary : IReadOnlyDictionary<string, string>
    {
        private readonly Dictionary<string, string> inner;

        public ReadOnlyOnlyDictionary() : this(new Dictionary<string, string>())
        {
        }

        public ReadOnlyOnlyDictionary(Dictionary<string, string> inner)
        {
            this.inner = inner;
        }

        public string this[string key] => inner[key];
        public IEnumerable<string> Keys => inner.Keys;
        public IEnumerable<string> Values => inner.Values;
        public int Count => inner.Count;
        public bool ContainsKey(string key) => inner.ContainsKey(key);
        public bool TryGetValue(string key, out string value) => inner.TryGetValue(key, out value);
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => inner.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
