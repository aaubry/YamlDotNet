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
using Xunit;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test.Analyzers.StaticGenerator
{
    public class RootCollectionTests
    {
        [Fact]
        public void RootArrayWorks()
        {
            var deserializer = new StaticDeserializerBuilder(new StaticContext()).Build();
            var yaml = @"
- Test: hello
- Test: world
";

            var actual = deserializer.Deserialize<RootObject[]>(yaml);
            Assert.Equal("hello", actual[0].Test);
            Assert.Equal("world", actual[1].Test);
        }

        [Fact]
        public void RootListWorks()
        {
            var deserializer = new StaticDeserializerBuilder(new StaticContext()).Build();
            var yaml = @"
- Test: hello
- Test: world
";

            var actual = deserializer.Deserialize<List<RootObject>>(yaml);
            Assert.Equal("hello", actual[0].Test);
            Assert.Equal("world", actual[1].Test);
        }

        [Fact]
        public void RootDictionaryWorks()
        {
            var deserializer = new StaticDeserializerBuilder(new StaticContext()).Build();
            var yaml = @"
a:
 Test: hello
b:
 Test: world
";

            var actual = deserializer.Deserialize<Dictionary<string, RootObject>>(yaml);
            Assert.Equal("hello", actual["a"].Test);
            Assert.Equal("world", actual["b"].Test);
        }

        [Fact]
        public void RootObjectWorks()
        {
            var deserializer = new StaticDeserializerBuilder(new StaticContext()).Build();
            var yaml = @"
a: hello
b: world
";

            var actual = (IDictionary<object, object>) deserializer.Deserialize<object>(yaml);
            Assert.Equal("hello", actual["a"]);
            Assert.Equal("world", actual["b"]);
        }
    }
    [YamlSerializable]
    public class RootObject
    {
        public string Test { get; set; }
    }
}
