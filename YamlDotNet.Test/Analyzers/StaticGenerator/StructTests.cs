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
using System.Collections.Generic;
using Xunit;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test.Analyzers.StaticGenerator
{
    public class StructTests
    {
        [Fact]
        public void RegularStructWorks()
        {
            var deserializer = new StaticDeserializerBuilder(new StaticContext()).Build();
            var yaml = @"Single: 1.1
Dbl: 1.000001
U8: 1
I8: 1
U16: 1
I16: 1
I32: 1
U32: 1
I64: 1
U64: 1
Str: hello
C: w
# A Description
Hello: world
NestedPropertyStruct:
  Single: 1.1
  Dbl: 1.000001
  U8: 1
  I8: 1
  U16: 1
  I16: 1
  I32: 1
  U32: 1
  I64: 1
  U64: 1
  Str: hello
  C: w
  # A Description
  Hello: world
NestedField:
  Single: 1.1
  Dbl: 1.000001
  U8: 1
  I8: 1
  U16: 1
  I16: 1
  I32: 1
  U32: 1
  I64: 1
  U64: 1
  Str: hello
  C: w
  # A Description
  Hello: world
NestedListField:
- Single: 1.1
  Dbl: 1.000001
  U8: 1
  I8: 1
  U16: 1
  I16: 1
  I32: 1
  U32: 1
  I64: 1
  U64: 1
  Str: hello
  C: w
  # A Description
  Hello: world
NestedProperty:
  Single: 1.1
  Dbl: 1.000001
  U8: 1
  I8: 1
  U16: 1
  I16: 1
  I32: 1
  U32: 1
  I64: 1
  U64: 1
  Str: hello
  C: w
  # A Description
  Hello: world
NestedListProperty:
- Single: 1.1
  Dbl: 1.000001
  U8: 1
  I8: 1
  U16: 1
  I16: 1
  I32: 1
  U32: 1
  I64: 1
  U64: 1
  Str: hello
  C: w
  # A Description
  Hello: world
";
            var actual = deserializer.Deserialize<RegularStruct>(yaml);
            AssertRegularStruct(actual);
            AssertRegularStructProperty(actual.NestedPropertyStruct);
            AssertRegularNestedStruct(actual.NestedField);
            AssertRegularNestedStruct(actual.NestedProperty);
            Assert.NotNull(actual.NestedListField);
            foreach (var nestedStruct in actual.NestedListField)
            {
                AssertRegularNestedStruct(nestedStruct);
            }

            foreach (var nestedStruct in actual.NestedListProperty)
            {
                AssertRegularNestedStruct(nestedStruct);
            }

            var serializer = new StaticSerializerBuilder(new StaticContext()).DisableAliases().Build();
            var actualYaml = serializer.Serialize(actual);
            yaml = @"Single: 1.1
Dbl: 1.000001
U8: 1
I8: 1
U16: 1
I16: 1
I32: 1
U32: 1
I64: 1
U64: 1
Str: hello
C: w
# A Description
Hello: ""world""
NestedPropertyStruct:
  Single: 1.1
  Dbl: 1.000001
  U8: 1
  I8: 1
  U16: 1
  I16: 1
  I32: 1
  U32: 1
  I64: 1
  U64: 1
  Str: hello
  C: w
  # A Description
  Hello: ""world""
NestedField:
  Single: 1.1
  Dbl: 1.000001
  U8: 1
  I8: 1
  U16: 1
  I16: 1
  I32: 1
  U32: 1
  I64: 1
  U64: 1
  Str: hello
  C: w
  # A Description
  Hello: ""world""
NestedListField:
- Single: 1.1
  Dbl: 1.000001
  U8: 1
  I8: 1
  U16: 1
  I16: 1
  I32: 1
  U32: 1
  I64: 1
  U64: 1
  Str: hello
  C: w
  # A Description
  Hello: ""world""
NestedProperty:
  Single: 1.1
  Dbl: 1.000001
  U8: 1
  I8: 1
  U16: 1
  I16: 1
  I32: 1
  U32: 1
  I64: 1
  U64: 1
  Str: hello
  C: w
  # A Description
  Hello: ""world""
NestedListProperty:
- Single: 1.1
  Dbl: 1.000001
  U8: 1
  I8: 1
  U16: 1
  I16: 1
  I32: 1
  U32: 1
  I64: 1
  U64: 1
  Str: hello
  C: w
  # A Description
  Hello: ""world""";
            Assert.Equal(yaml.NormalizeNewLines().TrimNewLines(), actualYaml.NormalizeNewLines().TrimNewLines());

            return;

            void AssertRegularStruct(RegularStruct value)
            {
                Assert.Equal(1.1f, value.Single);
                Assert.Equal(1.000001, value.Dbl);
                Assert.Equal(1, value.U8);
                Assert.Equal(1, value.I8);
                Assert.Equal(1, value.U16);
                Assert.Equal(1, value.I16);
                Assert.Equal(1, value.I32);
                Assert.Equal((uint)1, value.U32);
                Assert.Equal(1, value.I64);
                Assert.Equal((ulong)1, value.U64);
                Assert.Equal("hello", value.Str);
                Assert.Equal('w', value.C);
                Assert.Equal("world", value.Member);
                Assert.Equal("", value.Ignored);
            }

            void AssertRegularStructProperty(RegularNestedStructProperty value)
            {
                Assert.Equal(1.1f, value.Single);
                Assert.Equal(1.000001, value.Dbl);
                Assert.Equal(1, value.U8);
                Assert.Equal(1, value.I8);
                Assert.Equal(1, value.U16);
                Assert.Equal(1, value.I16);
                Assert.Equal(1, value.I32);
                Assert.Equal((uint)1, value.U32);
                Assert.Equal(1, value.I64);
                Assert.Equal((ulong)1, value.U64);
                Assert.Equal("hello", value.Str);
                Assert.Equal('w', value.C);
                Assert.Equal("world", value.Member);
                Assert.Equal("", value.Ignored);
            }

            void AssertRegularNestedStruct(RegularNestedStruct value)
            {
                Assert.Equal(1.1f, value.Single);
                Assert.Equal(1.000001, value.Dbl);
                Assert.Equal(1, value.U8);
                Assert.Equal(1, value.I8);
                Assert.Equal(1, value.U16);
                Assert.Equal(1, value.I16);
                Assert.Equal(1, value.I32);
                Assert.Equal((uint)1, value.U32);
                Assert.Equal(1, value.I64);
                Assert.Equal((ulong)1, value.U64);
                Assert.Equal("hello", value.Str);
                Assert.Equal('w', value.C);
                Assert.Equal("world", value.Member);
                Assert.Equal("", value.Ignored);
            }
        }
    }

    [YamlSerializable]
    public struct RegularStruct
    {
        public float Single;
        public double Dbl;
        public byte U8;
        public sbyte I8;
        public ushort U16;
        public short I16;
        public int I32;
        public uint U32;
        public long I64;
        public ulong U64;
        public string Str;
        public char C;
        [YamlMember(Alias = "Hello", Description = "A Description", ScalarStyle = YamlDotNet.Core.ScalarStyle.DoubleQuoted)]
        public string Member;
        [YamlIgnore]
        public string Ignored;
        public RegularNestedStructProperty NestedPropertyStruct;
        public RegularNestedStruct NestedField;
        public List<RegularNestedStruct> NestedListField;
        public RegularNestedStruct NestedProperty { get; set; }
        public List<RegularNestedStruct> NestedListProperty { get; set; }

        public RegularStruct()
        {
            Ignored = "";
        }
    }

    [YamlSerializable]
    public struct RegularNestedStruct
    {
        public float Single;
        public double Dbl;
        public byte U8;
        public sbyte I8;
        public ushort U16;
        public short I16;
        public int I32;
        public uint U32;
        public long I64;
        public ulong U64;
        public string Str;
        public char C;
        [YamlMember(Alias = "Hello", Description = "A Description", ScalarStyle = YamlDotNet.Core.ScalarStyle.DoubleQuoted)]
        public string Member;
        [YamlIgnore]
        public string Ignored;

        public RegularNestedStruct()
        {
            Ignored = "";
        }
    }

    [YamlSerializable]
    public struct RegularNestedStructProperty
    {
        public float Single { get; set; }
        public double Dbl { get; set; }
        public byte U8 { get; set; }
        public sbyte I8 { get; set; }
        public ushort U16 { get; set; }
        public short I16 { get; set; }
        public int I32 { get; set; }
        public uint U32 { get; set; }
        public long I64 { get; set; }
        public ulong U64 { get; set; }
        public string Str { get; set; }
        public char C { get; set; }
        [YamlMember(Alias = "Hello", Description = "A Description", ScalarStyle = YamlDotNet.Core.ScalarStyle.DoubleQuoted)]
        public string Member { get; set; }
        [YamlIgnore]
        public string Ignored { get; set; }

        public RegularNestedStructProperty()
        {
            Ignored = "";
        }
    }
}
