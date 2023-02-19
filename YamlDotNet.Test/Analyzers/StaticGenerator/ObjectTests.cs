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
using Xunit;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test.Analyzers.StaticGenerator
{
    public class ObjectTests
    {
        [Fact]
        public void InheritedMembersWorks()
        {
            var deserializer = new StaticDeserializerBuilder(new StaticContext()).Build();
            var yaml = @"NotInherited: hello
Inherited: world
";
            var actual = deserializer.Deserialize<SerializedInheritedClass>(yaml);
            Assert.Equal("hello", actual.NotInherited);
            Assert.Equal("world", actual.Inherited);
            var serializer = new StaticSerializerBuilder(new StaticContext()).Build();
            var actualYaml = serializer.Serialize(actual);
            Assert.Equal(yaml.NormalizeNewLines().TrimNewLines(), actualYaml.NormalizeNewLines().TrimNewLines());
        }

        [Fact]
        public void RegularObjectWorks()
        {
            var deserializer = new StaticDeserializerBuilder(new StaticContext()).Build();
            var yaml = @"Prop1: hello
Prop2: 1
Hello: world
Inner:
  Prop1: a
  Prop2: 2
";
            var actual = deserializer.Deserialize<RegularObjectOuter>(yaml);
            Assert.Equal("hello", actual.Prop1);
            Assert.Equal(1, actual.Prop2);
            Assert.Equal("world", actual.Member);
            Assert.Equal("I am ignored", actual.Ignored);
            Assert.NotNull(actual.Inner);
            Assert.Equal("a", actual.Inner.Prop1);
            Assert.Equal(2, actual.Inner.Prop2);

            var serializer = new StaticSerializerBuilder(new StaticContext()).Build();
            var actualYaml = serializer.Serialize(actual);
            yaml = @"Prop1: hello
Prop2: 1
# A Description
Hello: ""world""
Inner:
  Prop1: a
  Prop2: 2
";
            Assert.Equal(yaml.NormalizeNewLines().TrimNewLines(), actualYaml.NormalizeNewLines().TrimNewLines());
        }
    }
    public class InheritedClass
    {
        public string Inherited { get; set; }
    }

    [YamlSerializable]
    public class SerializedInheritedClass : InheritedClass
    {
        public string NotInherited { get; set; }
    }

    [YamlSerializable]
    public class RegularObjectOuter
    {
        public string Prop1 { get; set; }
        public int Prop2 { get; set; }
        [YamlMember(Alias = "Hello", Description = "A Description", ScalarStyle = YamlDotNet.Core.ScalarStyle.DoubleQuoted)]
        public string Member { get; set; }
        [YamlIgnore]
        public string Ignored { get; set; } = "I am ignored";
        public RegularObjectInner Inner { get; set; }
    }

    [YamlSerializable]
    public class RegularObjectInner
    {
        public string Prop1 { get; set; }
        public int Prop2 { get; set; }
    }
}
