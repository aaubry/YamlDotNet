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
using System.Collections;
using System.Collections.Generic;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Callbacks;
using YamlDotNet.Serialization.NamingConventions;

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
Nested:
  NestedProp: abc
DictionaryOfArrays:
  a:
  - 1
  b:
  - 2
SomeValue: ""abc""
SomeDictionary:
  a: 1
  b: 2
";
            var actual = deserializer.Deserialize<RegularObjectOuter>(yaml);
            Assert.Equal("hello", actual.Prop1);
            Assert.Equal(1, actual.Prop2);
            Assert.Equal("world", actual.Member);
            Assert.Equal("I am ignored", actual.Ignored);
            Assert.NotNull(actual.Inner);
            Assert.Equal("a", actual.Inner.Prop1);
            Assert.Equal(2, actual.Inner.Prop2);
            Assert.NotNull(actual.Nested);
            Assert.Equal("abc", actual.Nested.NestedProp);
            Assert.Equal("1", actual.DictionaryOfArrays["a"][0]);
            Assert.Equal("2", actual.DictionaryOfArrays["b"][0]);
            Assert.Equal("abc", actual.SomeValue);
            Assert.Equal("1", ((IDictionary<object, object>)actual.SomeDictionary)["a"]);
            Assert.Equal("2", ((IDictionary<object, object>)actual.SomeDictionary)["b"]);

            var serializer = new StaticSerializerBuilder(new StaticContext()).Build();
            var actualYaml = serializer.Serialize(actual);
            yaml = @"Prop1: hello
Prop2: 1
# A Description
Hello: ""world""
Inner:
  Prop1: a
  Prop2: 2
Nested:
  NestedProp: abc
DictionaryOfArrays:
  a:
  - 1
  b:
  - 2
SomeValue: abc
SomeDictionary:
  a: 1
  b: 2";
            Assert.Equal(yaml.NormalizeNewLines().TrimNewLines(), actualYaml.NormalizeNewLines().TrimNewLines());
        }

        [Fact]
        public void EnumerablesAreTreatedAsLists() => ExecuteListOverrideTest<EnumerableClass>();

        [Fact]
        public void CollectionsAreTreatedAsLists() => ExecuteListOverrideTest<CollectionClass>();

        [Fact]
        public void IListsAreTreatedAsLists() => ExecuteListOverrideTest<ListClass>();

        [Fact]
        public void ReadOnlyCollectionsAreTreatedAsLists() => ExecuteListOverrideTest<ReadOnlyCollectionClass>();

        [Fact]
        public void ReadOnlyListsAreTreatedAsLists() => ExecuteListOverrideTest<ReadOnlyListClass>();

        [Fact]
        public void IListAreTreatedAsLists()
        {
            var yaml = @"Test:
- value1
- value2
";
            var deserializer = new StaticDeserializerBuilder(new StaticContext()).Build();
            var actual = deserializer.Deserialize<CollectionClass>(yaml);
            Assert.NotNull(actual);
            Assert.IsType<List<string>>(actual.Test);
            Assert.Equal("value1", ((List<string>)actual.Test)[0]);
            Assert.Equal("value2", ((List<string>)actual.Test)[1]);
        }

        [Fact]
        public void CallbacksAreExecuted()
        {
            var yaml = "Test: Hi";
            var deserializer = new StaticDeserializerBuilder(new StaticContext()).Build();
            var test = deserializer.Deserialize<TestState>(yaml);

            Assert.Equal(1, test.OnDeserializedCallCount);
            Assert.Equal(1, test.OnDeserializingCallCount);

            var serializer = new StaticSerializerBuilder(new StaticContext()).Build();
            yaml = serializer.Serialize(test);
            Assert.Equal(1, test.OnSerializedCallCount);
            Assert.Equal(1, test.OnSerializingCallCount);
        }

        [Fact]
        public void NamingConventionAppliedToEnum()
        {
            var serializer = new StaticSerializerBuilder(new StaticContext()).WithEnumNamingConvention(CamelCaseNamingConvention.Instance).Build();
            ScalarStyle style = ScalarStyle.Plain;
            var serialized = serializer.Serialize(style);
            Assert.Equal("plain", serialized.TrimNewLines());
        }

        [Fact]
        public void NamingConventionAppliedToEnumWhenDeserializing()
        {
            var serializer = new StaticDeserializerBuilder(new StaticContext()).WithEnumNamingConvention(UnderscoredNamingConvention.Instance).Build();
            var yaml = "Double_Quoted";
            ScalarStyle expected = ScalarStyle.DoubleQuoted;
            var actual = serializer.Deserialize<ScalarStyle>(yaml);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReadOnlyDictionariesAreTreatedAsDictionaries()
        {
            var yaml = @"Test:
  a: b
  c: d
";
            var deserializer = new StaticDeserializerBuilder(new StaticContext()).Build();
            var actual = deserializer.Deserialize<ReadOnlyDictionaryClass>(yaml);
            Assert.NotNull(actual);
            Assert.IsType<Dictionary<string, string>>(actual.Test);
            var dictionary = (Dictionary<string, string>)actual.Test;
            Assert.Equal(2, dictionary.Count);
            Assert.Equal("b", dictionary["a"]);
            Assert.Equal("d", dictionary["c"]);
        }

#if NET6_0_OR_GREATER
        [Fact]
        public void EnumDeserializationUsesEnumMemberAttribute()
        {
            var deserializer = new StaticDeserializerBuilder(new StaticContext()).Build();
            var yaml = "goodbye";
            var actual = deserializer.Deserialize<EnumMemberedEnum>(yaml);
            Assert.Equal(EnumMemberedEnum.Hello, actual);
        }

        [Fact]
        public void EnumSerializationUsesEnumMemberAttribute()
        {
            var serializer = new StaticSerializerBuilder(new StaticContext()).Build();
            var actual = serializer.Serialize(EnumMemberedEnum.Hello);
            Assert.Equal("goodbye", actual.TrimNewLines());
        }

        [YamlSerializable]
        public enum EnumMemberedEnum
        {
            No = 0,

            [System.Runtime.Serialization.EnumMember(Value = "goodbye")]
            Hello = 1
        }
#endif
        [Fact]
        public void ComplexTypeConverter_UsesSerializerToSerializeComplexTypes()
        {
            var serializer = new StaticSerializerBuilder(new StaticContext()).WithTypeConverter(new ComplexTypeConverter()).Build();
            var o = new ComplexType
            {
                InnerType1 = new InnerType
                {
                    Prop1 = "prop1",
                    Prop2 = "prop2"
                },
                InnerType2 = new InnerType
                {
                    Prop1 = "2.1",
                    Prop2 = "2.2"
                }
            };
            var actual = serializer.Serialize(o);
            var expected = @"inner.prop1: prop1
inner.prop2: prop2
prop2:
  Prop1: 2.1
  Prop2: 2.2".NormalizeNewLines();
            Assert.Equal(expected, actual.NormalizeNewLines().TrimNewLines());
        }

        [Fact]
        public void ComplexTypeConverter_UsesDeserializerToDeserializeComplexTypes()
        {
            var deserializer = new StaticDeserializerBuilder(new StaticContext()).WithTypeConverter(new ComplexTypeConverter()).Build();
            var yaml = @"inner.prop1: prop1
inner.prop2: prop2
prop2:
  Prop1: 2.1
  Prop2: 2.2";
            var actual = deserializer.Deserialize<ComplexType>(yaml);
            Assert.Equal("prop1", actual.InnerType1.Prop1);
            Assert.Equal("prop2", actual.InnerType1.Prop2);
            Assert.Equal("2.1", actual.InnerType2.Prop1);
            Assert.Equal("2.2", actual.InnerType2.Prop2);
        }

        [YamlSerializable]
        public class ComplexType
        {
            public InnerType InnerType1 { get; set; }
            public InnerType InnerType2 { get; set; }
        }

        [YamlSerializable]
        public class InnerType
        {
            public string Prop1 { get; set; }
            public string Prop2 { get; set; }
        }

        private class ComplexTypeConverter : IYamlTypeConverter
        {
            public bool Accepts(Type type)
            {
                if (type == typeof(ComplexType))
                {
                    return true;
                }
                return false;
            }

            public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
            {
                parser.Consume<MappingStart>();

                var result = new ComplexType();
                result.InnerType1 = new InnerType();

                Consume(parser, result, rootDeserializer);
                Consume(parser, result, rootDeserializer);
                Consume(parser, result, rootDeserializer);

                parser.Consume<MappingEnd>();

                return result;
            }

            public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
            {
                var c = (ComplexType)value;
                emitter.Emit(new MappingStart());
                emitter.Emit(new Scalar("inner.prop1"));
                emitter.Emit(new Scalar(c.InnerType1.Prop1));
                emitter.Emit(new Scalar("inner.prop2"));
                emitter.Emit(new Scalar(c.InnerType1.Prop2));
                emitter.Emit(new Scalar("prop2"));
                serializer(c.InnerType2);
                emitter.Emit(new MappingEnd());
            }

            private void Consume(IParser parser, ComplexType type, ObjectDeserializer deserializer)
            {
                var name = parser.Consume<Scalar>();
                if (name.Value == "inner.prop1")
                {
                    var value = parser.Consume<Scalar>();
                    type.InnerType1.Prop1 = value.Value;
                }
                else if (name.Value == "inner.prop2")
                {
                    var value = parser.Consume<Scalar>();
                    type.InnerType1.Prop2 = value.Value;
                }
                else if (name.Value == "prop2")
                {
                    var value = deserializer(typeof(InnerType));
                    type.InnerType2 = (InnerType)value;
                }
                else
                {
                    throw new Exception("Invalid property name");
                }
            }
        }

        private void ExecuteListOverrideTest<TClass>() where TClass : InterfaceLists
        {
            var yaml = @"Test:
- value1
- value2
";
            var deserializer = new StaticDeserializerBuilder(new StaticContext()).Build();
            var actual = deserializer.Deserialize<TClass>(yaml);
            Assert.NotNull(actual);
            Assert.IsType<List<string>>(actual.TestValue);
            Assert.Equal("value1", ((List<string>)actual.TestValue)[0]);
            Assert.Equal("value2", ((List<string>)actual.TestValue)[1]);
        }

        [YamlSerializable]
        public class TestState
        {
            public int OnDeserializedCallCount { get; set; }
            public int OnDeserializingCallCount { get; set; }
            public int OnSerializedCallCount { get; set; }
            public int OnSerializingCallCount { get; set; }

            public string Test { get; set; } = string.Empty;

            [OnDeserialized]
            public void Deserialized() => OnDeserializedCallCount++;

            [OnDeserializing]
            public void Deserializing() => OnDeserializingCallCount++;

            [OnSerialized]
            public void Serialized() => OnSerializedCallCount++;

            [OnSerializing]
            public void Serializing() => OnSerializingCallCount++;
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
        public NestedClass Nested { get; set; }

        public Dictionary<string, string[]> DictionaryOfArrays { get; set; }

        public object SomeValue { get; set; }

        public object SomeDictionary { get; set; }

        [YamlSerializable]
        public class NestedClass
        {
            public string NestedProp { get; set; }
        }
    }

    [YamlSerializable]
    public class RegularObjectInner
    {
        public string Prop1 { get; set; }
        public int Prop2 { get; set; }
    }

    [YamlSerializable]
    public class EnumerableClass : InterfaceLists<IEnumerable<string>>
    {
        public IEnumerable<string> Test { get; set; }
        public object TestValue => Test;
    }

    [YamlSerializable]
    public class CollectionClass : InterfaceLists<ICollection<string>>
    {
        public ICollection<string> Test { get; set; }
        public object TestValue => Test;
    }

    [YamlSerializable]
    public class ListClass : InterfaceLists<IList<string>>
    {
        public IList<string> Test { get; set; }
        public object TestValue => Test;
    }

    [YamlSerializable]
    public class ReadOnlyCollectionClass : InterfaceLists<IReadOnlyCollection<string>>
    {
        public IReadOnlyCollection<string> Test { get; set; }
        public object TestValue => Test;
    }

    [YamlSerializable]
    public class ReadOnlyListClass : InterfaceLists<IReadOnlyList<string>>
    {
        public IReadOnlyList<string> Test { get; set; }
        public object TestValue => Test;
    }

    [YamlSerializable]
    public class ReadOnlyDictionaryClass
    {
        public IReadOnlyDictionary<string, string> Test { get; set; }
    }

    public interface InterfaceLists<TType> : InterfaceLists where TType : IEnumerable
    {
        TType Test { get; set; }
    }

    public interface InterfaceLists
    {
        object TestValue { get; }
    }
}
