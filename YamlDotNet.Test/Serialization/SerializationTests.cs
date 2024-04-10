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
using System.ComponentModel;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Common;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Callbacks;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.ObjectFactories;
using YamlDotNet.Test.Analyzers.StaticGenerator;

namespace YamlDotNet.Test.Serialization
{
    public class SerializationTests : SerializationTestHelper
    {
        #region Test Cases

        private static readonly string[] TrueStrings = { "true", "y", "yes", "on" };
        private static readonly string[] FalseStrings = { "false", "n", "no", "off" };

        public static IEnumerable<object[]> DeserializeScalarBoolean_TestCases
        {
            get
            {
                foreach (var trueString in TrueStrings)
                {
                    yield return new object[] { trueString, true };
                    yield return new object[] { trueString.ToUpper(), true };
                }

                foreach (var falseString in FalseStrings)
                {
                    yield return new object[] { falseString, false };
                    yield return new object[] { falseString.ToUpper(), false };
                }
            }
        }

        #endregion

        [Fact]
        public void DeserializeEmptyDocument()
        {
            var emptyText = string.Empty;

            var array = Deserializer.Deserialize<int[]>(UsingReaderFor(emptyText));

            array.Should().BeNull();
        }

        [Fact]
        public void DeserializeScalar()
        {
            var stream = Yaml.ReaderFrom("02-scalar-in-imp-doc.yaml");

            var result = Deserializer.Deserialize(stream);

            result.Should().Be("a scalar");
        }

        [Theory]
        [MemberData(nameof(DeserializeScalarBoolean_TestCases))]
        public void DeserializeScalarBoolean(string value, bool expected)
        {
            var result = Deserializer.Deserialize<bool>(UsingReaderFor(value));

            result.Should().Be(expected);
        }

        [Fact]
        public void DeserializeScalarBooleanThrowsWhenInvalid()
        {
            Action action = () => Deserializer.Deserialize<bool>(UsingReaderFor("not-a-boolean"));

            action.ShouldThrow<YamlException>().WithInnerException<FormatException>();
        }

        [Fact]
        public void DeserializeScalarZero()
        {
            var result = Deserializer.Deserialize<int>(UsingReaderFor("0"));

            result.Should().Be(0);
        }

        [Fact]
        public void DeserializeScalarDecimal()
        {
            var result = Deserializer.Deserialize<int>(UsingReaderFor("+1_234_567"));

            result.Should().Be(1234567);
        }

        [Fact]
        public void DeserializeScalarBinaryNumber()
        {
            var result = Deserializer.Deserialize<int>(UsingReaderFor("-0b1_0010_1001_0010"));

            result.Should().Be(-4754);
        }

        [Fact]
        public void DeserializeScalarOctalNumber()
        {
            var result = Deserializer.Deserialize<int>(UsingReaderFor("+071_352"));

            result.Should().Be(29418);
        }

        [Fact]
        public void DeserializeNullableScalarOctalNumber()
        {
            var result = Deserializer.Deserialize<int?>(UsingReaderFor("+071_352"));

            result.Should().Be(29418);
        }

        [Fact]
        public void DeserializeScalarHexNumber()
        {
            var result = Deserializer.Deserialize<int>(UsingReaderFor("-0x_0F_B9"));

            result.Should().Be(-0xFB9);
        }

        [Fact]
        public void DeserializeScalarLongBase60Number()
        {
            var result = Deserializer.Deserialize<long>(UsingReaderFor("99_:_58:47:3:6_2:10"));

            result.Should().Be(77744246530L);
        }

        [Theory]
        [InlineData(EnumExample.One)]
        [InlineData(EnumExample.One | EnumExample.Two)]
        public void RoundtripEnums(EnumExample value)
        {
            var result = DoRoundtripFromObjectTo<EnumExample>(value);

            result.Should().Be(value);
        }

        [Theory]
        [InlineData(EnumExample.One)]
        [InlineData(EnumExample.One | EnumExample.Two)]
        [InlineData(null)]
        public void RoundtripNullableEnums(EnumExample? value)
        {
            var result = DoRoundtripFromObjectTo<EnumExample?>(value);

            result.Should().Be(value);
        }

        [Fact]
        public void RoundtripNullableStructWithValue()
        {
            var value = new StructExample { Value = 2 };

            var result = DoRoundtripFromObjectTo<StructExample?>(value);

            result.Should().Be(value);
        }

        [Fact]
        public void RoundtripNullableStructWithoutValue()
        {
            var result = DoRoundtripFromObjectTo<StructExample?>(null);

            result.Should().Be(null);
        }

        [Fact]
        public void SerializeCircularReference()
        {
            var obj = new CircularReference();
            obj.Child1 = new CircularReference
            {
                Child1 = obj,
                Child2 = obj
            };

            Action action = () => SerializerBuilder.EnsureRoundtrip().Build().Serialize(new StringWriter(), obj, typeof(CircularReference));

            action.ShouldNotThrow();
        }

        [Fact]
        public void DeserializeIncompleteDirective()
        {
            Action action = () => Deserializer.Deserialize<object>(UsingReaderFor("%Y"));

            action.ShouldThrow<SyntaxErrorException>()
                .WithMessage("While scanning a directive, found unexpected end of stream.");
        }

        [Fact]
        public void DeserializeSkippedReservedDirective()
        {
            Action action = () => Deserializer.Deserialize<object>(UsingReaderFor("%Y "));

            action.ShouldNotThrow();
        }

        [Fact]
        public void DeserializeCustomTags()
        {
            var stream = Yaml.ReaderFrom("tags.yaml");

            DeserializerBuilder.WithTagMapping("tag:yaml.org,2002:point", typeof(Point));
            var result = Deserializer.Deserialize(stream);

            result.Should().BeOfType<Point>().And
                .Subject.As<Point>()
                .ShouldBeEquivalentTo(new { X = 10, Y = 20 }, o => o.ExcludingMissingMembers());
        }

        [Fact]
        public void DeserializeWithGapsBetweenKeys()
        {
            var yamlReader = new StringReader(@"Text: >
  Some Text.

Value: foo");
            var result = Deserializer.Deserialize(yamlReader);

            result.Should().NotBeNull();
        }

        [Fact]
        public void SerializeCustomTags()
        {
            var expectedResult = Yaml.ReaderFrom("tags.yaml").ReadToEnd().NormalizeNewLines();
            SerializerBuilder
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
                .WithTagMapping(new TagName("tag:yaml.org,2002:point"), typeof(Point));

            var point = new Point(10, 20);
            var result = Serializer.Serialize(point);

            result.Should().Be(expectedResult);
        }

        [Fact]
        public void SerializeWithCRLFNewLine()
        {
            var expectedResult = Yaml
                .ReaderFrom("list.yaml")
                .ReadToEnd()
                .NormalizeNewLines()
                .Replace(Environment.NewLine, "\r\n");

            var list = new string[] { "one", "two", "three" };
            var result = SerializerBuilder
                .WithNewLine("\r\n")
                .Build()
                .Serialize(list);

            result.Should().Be(expectedResult);
        }

        [Fact]
        public void SerializeWithLFNewLine()
        {
            var expectedResult = Yaml
                .ReaderFrom("list.yaml")
                .ReadToEnd()
                .NormalizeNewLines()
                .Replace(Environment.NewLine, "\n");

            var list = new string[] { "one", "two", "three" };
            var result = SerializerBuilder
                .WithNewLine("\n")
                .Build()
                .Serialize(list);

            result.Should().Be(expectedResult);
        }

        [Fact]
        public void SerializeWithCRNewLine()
        {
            var expectedResult = Yaml
                .ReaderFrom("list.yaml")
                .ReadToEnd()
                .NormalizeNewLines()
                .Replace(Environment.NewLine, "\r");

            var list = new string[] { "one", "two", "three" };
            var result = SerializerBuilder
                .WithNewLine("\r")
                .Build()
                .Serialize(list);

            result.Should().Be(expectedResult);
        }

        [Fact]
        public void DeserializeExplicitType()
        {
            var text = Yaml.ReaderFrom("explicit-type.template").TemplatedOn<Simple>();

            var result = new DeserializerBuilder()
                .WithTagMapping("!Simple", typeof(Simple))
                .Build()
                .Deserialize<Simple>(UsingReaderFor(text));

            result.aaa.Should().Be("bbb");
        }

        [Fact]
        public void DeserializeConvertible()
        {
            var text = Yaml.ReaderFrom("convertible.template").TemplatedOn<Convertible>();

            var result = new DeserializerBuilder()
                .WithTagMapping("!Convertible", typeof(Convertible))
                .Build()
                .Deserialize<Simple>(UsingReaderFor(text));

            result.aaa.Should().Be("[hello, world]");
        }

        [Fact]
        public void DeserializationFailsForUndefinedForwardReferences()
        {
            var text = Lines(
                "Nothing: *forward",
                "MyString: ForwardReference");

            Action action = () => Deserializer.Deserialize<Example>(UsingReaderFor(text));

            action.ShouldThrow<AnchorNotFoundException>();
        }

        [Fact]
        public void RoundtripObject()
        {
            var obj = new Example();

            var result = DoRoundtripFromObjectTo<Example>(
                obj,
                new SerializerBuilder()
                    .WithTagMapping("!Example", typeof(Example))
                    .EnsureRoundtrip()
                    .Build(),
                new DeserializerBuilder()
                    .WithTagMapping("!Example", typeof(Example))
                    .Build()
            );

            result.ShouldBeEquivalentTo(obj);
        }

        [Fact]
        public void RoundtripObjectWithDefaults()
        {
            var obj = new Example();

            var result = DoRoundtripFromObjectTo<Example>(
                obj,
                new SerializerBuilder()
                    .WithTagMapping("!Example", typeof(Example))
                    .EnsureRoundtrip()
                    .Build(),
                new DeserializerBuilder()
                    .WithTagMapping("!Example", typeof(Example))
                    .Build()
            );

            result.ShouldBeEquivalentTo(obj);
        }

        [Fact]
        public void RoundtripAnonymousType()
        {
            var data = new { Key = 3 };

            var result = DoRoundtripFromObjectTo<Dictionary<string, string>>(data);

            result.Should().Equal(new Dictionary<string, string> {
                { "Key", "3" }
            });
        }

        [Fact]
        public void RoundtripWithYamlTypeConverter()
        {
            var obj = new MissingDefaultCtor("Yo");

            SerializerBuilder
                .EnsureRoundtrip()
                .WithTypeConverter(new MissingDefaultCtorConverter());

            DeserializerBuilder
                .WithTypeConverter(new MissingDefaultCtorConverter());

            var result = DoRoundtripFromObjectTo<MissingDefaultCtor>(obj, Serializer, Deserializer);

            result.Value.Should().Be("Yo");
        }

        [Fact]
        public void RoundtripAlias()
        {
            var writer = new StringWriter();
            var input = new NameConvention { AliasTest = "Fourth" };

            SerializerBuilder
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults);

            Serializer.Serialize(writer, input, input.GetType());
            var text = writer.ToString();

            // Todo: use RegEx once FluentAssertions 2.2 is released
            text.TrimEnd('\r', '\n').Should().Be("fourthTest: Fourth");

            var output = Deserializer.Deserialize<NameConvention>(UsingReaderFor(text));

            output.AliasTest.Should().Be(input.AliasTest);
        }

        [Fact]
        public void RoundtripAliasOverride()
        {
            var writer = new StringWriter();
            var input = new NameConvention { AliasTest = "Fourth" };

            var attribute = new YamlMemberAttribute
            {
                Alias = "fourthOverride"
            };

            var serializer = new SerializerBuilder()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
                .WithAttributeOverride<NameConvention>(nc => nc.AliasTest, attribute)
                .Build();

            serializer.Serialize(writer, input, input.GetType());
            var text = writer.ToString();

            // Todo: use RegEx once FluentAssertions 2.2 is released
            text.TrimEnd('\r', '\n').Should().Be("fourthOverride: Fourth");

            DeserializerBuilder.WithAttributeOverride<NameConvention>(n => n.AliasTest, attribute);
            var output = Deserializer.Deserialize<NameConvention>(UsingReaderFor(text));

            output.AliasTest.Should().Be(input.AliasTest);
        }

        [Fact]
        // Todo: is the assert on the string necessary?
        public void RoundtripDerivedClass()
        {
            var obj = new InheritanceExample
            {
                SomeScalar = "Hello",
                RegularBase = new Derived { BaseProperty = "foo", DerivedProperty = "bar" }
            };

            var result = DoRoundtripFromObjectTo<InheritanceExample>(
                obj,
                new SerializerBuilder()
                    .WithTagMapping("!InheritanceExample", typeof(InheritanceExample))
                    .WithTagMapping("!Derived", typeof(Derived))
                    .EnsureRoundtrip()
                    .Build(),
                new DeserializerBuilder()
                    .WithTagMapping("!InheritanceExample", typeof(InheritanceExample))
                    .WithTagMapping("!Derived", typeof(Derived))
                    .Build()
            );

            result.SomeScalar.Should().Be("Hello");
            result.RegularBase.Should().BeOfType<Derived>().And
                .Subject.As<Derived>().ShouldBeEquivalentTo(new { ChildProp = "bar" }, o => o.ExcludingMissingMembers());
        }

        [Fact]
        public void RoundtripDerivedClassWithSerializeAs()
        {
            var obj = new InheritanceExample
            {
                SomeScalar = "Hello",
                BaseWithSerializeAs = new Derived { BaseProperty = "foo", DerivedProperty = "bar" }
            };

            var result = DoRoundtripFromObjectTo<InheritanceExample>(
                obj,
                new SerializerBuilder()
                    .WithTagMapping("!InheritanceExample", typeof(InheritanceExample))
                    .EnsureRoundtrip()
                    .Build(),
                new DeserializerBuilder()
                    .WithTagMapping("!InheritanceExample", typeof(InheritanceExample))
                    .Build()
            );

            result.BaseWithSerializeAs.Should().BeOfType<Base>().And
                .Subject.As<Base>().ShouldBeEquivalentTo(new { ParentProp = "foo" }, o => o.ExcludingMissingMembers());
        }

        [Fact]
        public void RoundtripInterfaceProperties()
        {
            AssumingDeserializerWith(new LambdaObjectFactory(t =>
            {
                if (t == typeof(InterfaceExample)) { return new InterfaceExample(); }
                else if (t == typeof(IDerived)) { return new Derived(); }
                return null;
            }));

            var obj = new InterfaceExample
            {
                Derived = new Derived { BaseProperty = "foo", DerivedProperty = "bar" }
            };

            var result = DoRoundtripFromObjectTo<InterfaceExample>(obj);

            result.Derived.Should().BeOfType<Derived>().And
                .Subject.As<IDerived>().ShouldBeEquivalentTo(new { BaseProperty = "foo", DerivedProperty = "bar" }, o => o.ExcludingMissingMembers());
        }

        [Fact]
        public void DeserializeGuid()
        {
            var stream = Yaml.ReaderFrom("guid.yaml");
            var result = Deserializer.Deserialize<Guid>(stream);

            result.Should().Be(new Guid("9462790d5c44468985425e2dd38ebd98"));
        }

        [Fact]
        public void DeserializationOfOrderedProperties()
        {
            var stream = Yaml.ReaderFrom("ordered-properties.yaml");

            var orderExample = Deserializer.Deserialize<OrderExample>(stream);

            orderExample.Order1.Should().Be("Order1 value");
            orderExample.Order2.Should().Be("Order2 value");
        }

        [Fact]
        public void DeserializeEnumerable()
        {
            var obj = new[] { new Simple { aaa = "bbb" } };

            var result = DoRoundtripFromObjectTo<IEnumerable<Simple>>(obj);

            result.Should().ContainSingle(item => "bbb".Equals(item.aaa));
        }

        [Fact]
        public void DeserializeArray()
        {
            var stream = Yaml.ReaderFrom("list.yaml");

            var result = Deserializer.Deserialize<string[]>(stream);

            result.Should().Equal(new[] { "one", "two", "three" });
        }

        [Fact]
        public void DeserializeList()
        {
            var stream = Yaml.ReaderFrom("list.yaml");

            var result = Deserializer.Deserialize(stream);

            result.Should().BeAssignableTo<IList>().And
                .Subject.As<IList>().Should().Equal(new[] { "one", "two", "three" });
        }

        [Fact]
        public void DeserializeExplicitList()
        {
            var stream = Yaml.ReaderFrom("list-explicit.yaml");

            var result = new DeserializerBuilder()
                .WithTagMapping("!List", typeof(List<int>))
                .Build()
                .Deserialize(stream);

            result.Should().BeAssignableTo<IList<int>>().And
                .Subject.As<IList<int>>().Should().Equal(3, 4, 5);
        }

        [Fact]
        public void RoundtripList()
        {
            var obj = new List<int> { 2, 4, 6 };

            var result = DoRoundtripOn<List<int>>(obj, SerializerBuilder.EnsureRoundtrip().Build());

            result.Should().Equal(obj);
        }

        [Fact]
        public void RoundtripArrayWithTypeConversion()
        {
            var obj = new object[] { 1, 2, "3" };

            var result = DoRoundtripFromObjectTo<int[]>(obj);

            result.Should().Equal(1, 2, 3);
        }

        [Fact]
        public void RoundtripArrayOfIdenticalObjects()
        {
            var z = new Simple { aaa = "bbb" };
            var obj = new[] { z, z, z };

            var result = DoRoundtripOn<Simple[]>(obj);

            result.Should().HaveCount(3).And.OnlyContain(x => z.aaa.Equals(x.aaa));
            result[0].Should().BeSameAs(result[1]).And.BeSameAs(result[2]);
        }

        [Fact]
        public void DeserializeDictionary()
        {
            var stream = Yaml.ReaderFrom("dictionary.yaml");

            var result = Deserializer.Deserialize(stream);

            result.Should().BeAssignableTo<IDictionary<object, object>>().And.Subject
                .As<IDictionary<object, object>>().Should().Equal(new Dictionary<object, object> {
                    { "key1", "value1" },
                    { "key2", "value2" }
                });
        }

        [Fact]
        public void DeserializeExplicitDictionary()
        {
            var stream = Yaml.ReaderFrom("dictionary-explicit.yaml");

            var result = new DeserializerBuilder()
                .WithTagMapping("!Dictionary", typeof(Dictionary<string, int>))
                .Build()
                .Deserialize(stream);

            result.Should().BeAssignableTo<IDictionary<string, int>>().And.Subject
                .As<IDictionary<string, int>>().Should().Equal(new Dictionary<string, int> {
                    { "key1", 1 },
                    { "key2", 2 }
                });
        }

        [Fact]
        public void RoundtripDictionary()
        {
            var obj = new Dictionary<string, string> {
                { "key1", "value1" },
                { "key2", "value2" },
                { "key3", "value3" }
            };

            var result = DoRoundtripFromObjectTo<Dictionary<string, string>>(obj);

            result.Should().Equal(obj);
        }

        [Fact]
        public void DeserializeListOfDictionaries()
        {
            var stream = Yaml.ReaderFrom("list-of-dictionaries.yaml");

            var result = Deserializer.Deserialize<List<Dictionary<string, string>>>(stream);

            result.ShouldBeEquivalentTo(new[] {
                new Dictionary<string, string> {
                    { "connection", "conn1" },
                    { "path", "path1" }
                },
                new Dictionary<string, string> {
                    { "connection", "conn2" },
                    { "path", "path2" }
                }}, opt => opt.WithStrictOrderingFor(root => root));
        }

        [Fact]
        public void DeserializeTwoDocuments()
        {
            var reader = ParserFor(Lines(
                "---",
                "aaa: 111",
                "---",
                "aaa: 222",
                "..."));

            reader.Consume<StreamStart>();
            var one = Deserializer.Deserialize<Simple>(reader);
            var two = Deserializer.Deserialize<Simple>(reader);

            one.ShouldBeEquivalentTo(new { aaa = "111" });
            two.ShouldBeEquivalentTo(new { aaa = "222" });
        }

        [Fact]
        public void DeserializeThreeDocuments()
        {
            var reader = ParserFor(Lines(
                "---",
                "aaa: 111",
                "---",
                "aaa: 222",
                "---",
                "aaa: 333",
                "..."));

            reader.Consume<StreamStart>();
            var one = Deserializer.Deserialize<Simple>(reader);
            var two = Deserializer.Deserialize<Simple>(reader);
            var three = Deserializer.Deserialize<Simple>(reader);

            reader.Accept<StreamEnd>(out var _).Should().BeTrue("reader should have reached StreamEnd");
            one.ShouldBeEquivalentTo(new { aaa = "111" });
            two.ShouldBeEquivalentTo(new { aaa = "222" });
            three.ShouldBeEquivalentTo(new { aaa = "333" });
        }

        [Fact]
        public void SerializeGuid()
        {
            var guid = new Guid("{9462790D-5C44-4689-8542-5E2DD38EBD98}");

            var writer = new StringWriter();

            Serializer.Serialize(writer, guid);
            var serialized = writer.ToString();
            Regex.IsMatch(serialized, "^" + guid.ToString("D")).Should().BeTrue("serialized content should contain the guid, but instead contained: " + serialized);
        }

        [Fact]
        public void SerializeNullObject()
        {
#nullable enable
            object? obj = null;

            var writer = new StringWriter();

            Serializer.Serialize(writer, obj);
            var serialized = writer.ToString();
            serialized.Should().Be("--- " + writer.NewLine);
#nullable restore
        }

        [Fact]
        public void SerializationOfNullInListsAreAlwaysEmittedWithoutUsingEmitDefaults()
        {
            var writer = new StringWriter();
            var obj = new[] { "foo", null, "bar" };

            Serializer.Serialize(writer, obj);
            var serialized = writer.ToString();

            Regex.Matches(serialized, "-").Count.Should().Be(3, "there should have been 3 elements");
        }

        [Fact]
        public void SerializationOfNullInListsAreAlwaysEmittedWhenUsingEmitDefaults()
        {
            var writer = new StringWriter();
            var obj = new[] { "foo", null, "bar" };

            SerializerBuilder.Build().Serialize(writer, obj);
            var serialized = writer.ToString();

            Regex.Matches(serialized, "-").Count.Should().Be(3, "there should have been 3 elements");
        }

        [Fact]
        public void SerializationIncludesKeyWhenEmittingDefaults()
        {
            var writer = new StringWriter();
            var obj = new Example { MyString = null };

            SerializerBuilder.Build().Serialize(writer, obj, typeof(Example));

            writer.ToString().Should().Contain("MyString");
        }

        [Fact]
        [Trait("Motive", "Bug fix")]
        public void SerializationIncludesKeyFromAnonymousTypeWhenEmittingDefaults()
        {
            var writer = new StringWriter();
            var obj = new { MyString = (string)null };

            SerializerBuilder.Build().Serialize(writer, obj, obj.GetType());

            writer.ToString().Should().Contain("MyString");
        }

        [Fact]
        public void SerializationDoesNotIncludeKeyWhenDisregardingDefaults()
        {
            var writer = new StringWriter();
            var obj = new Example { MyString = null };

            SerializerBuilder
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults);

            Serializer.Serialize(writer, obj, typeof(Example));

            writer.ToString().Should().NotContain("MyString");
        }

        [Fact]
        public void SerializationOfDefaultsWorkInJson()
        {
            var writer = new StringWriter();
            var obj = new Example { MyString = null };

            SerializerBuilder.JsonCompatible().Build().Serialize(writer, obj, typeof(Example));

            writer.ToString().Should().Contain("MyString");
        }

        [Fact]
        public void SerializationOfLongKeysWorksInJson()
        {
            var writer = new StringWriter();
            var obj = new Dictionary<string, string>
            {
                { new string('x', 3000), "extremely long key" }
            };

            SerializerBuilder.JsonCompatible().Build().Serialize(writer, obj, typeof(Dictionary<string, string>));

            writer.ToString().Should().NotContain("?");
        }

        [Fact]
        public void SerializationOfAnchorWorksInJson()
        {
            var deserializer = new DeserializerBuilder().Build();
            var yamlObject = deserializer.Deserialize(Yaml.ReaderForText(@"
x: &anchor1
  z:
    v: 1
y:
  k: *anchor1"));

            var serializer = new SerializerBuilder()
                .JsonCompatible()
                .Build();

            serializer.Serialize(yamlObject).Trim().Should()
                .BeEquivalentTo(@"{""x"": {""z"": {""v"": ""1""}}, ""y"": {""k"": {""z"": {""v"": ""1""}}}}");
        }

        [Fact]
        // Todo: this is actually roundtrip
        public void DeserializationOfDefaultsWorkInJson()
        {
            var writer = new StringWriter();
            var obj = new Example { MyString = null };

            SerializerBuilder.EnsureRoundtrip().JsonCompatible().Build().Serialize(writer, obj, typeof(Example));
            var result = Deserializer.Deserialize<Example>(UsingReaderFor(writer));

            result.MyString.Should().BeNull();
        }

        [Fact]
        public void NullsRoundTrip()
        {
            var writer = new StringWriter();
            var obj = new Example { MyString = null };

            SerializerBuilder.EnsureRoundtrip().Build().Serialize(writer, obj, typeof(Example));
            var result = Deserializer.Deserialize<Example>(UsingReaderFor(writer));

            result.MyString.Should().BeNull();
        }

        [Theory]
        [InlineData(typeof(SByteEnum))]
        [InlineData(typeof(ByteEnum))]
        [InlineData(typeof(Int16Enum))]
        [InlineData(typeof(UInt16Enum))]
        [InlineData(typeof(Int32Enum))]
        [InlineData(typeof(UInt32Enum))]
        [InlineData(typeof(Int64Enum))]
        [InlineData(typeof(UInt64Enum))]
        public void DeserializationOfEnumWorksInJson(Type enumType)
        {
            var defaultEnumValue = 0;
            var nonDefaultEnumValue = Enum.GetValues(enumType).GetValue(1);

            var jsonSerializer = SerializerBuilder.EnsureRoundtrip().JsonCompatible().Build();
            var jsonSerializedEnum = jsonSerializer.Serialize(nonDefaultEnumValue);

            nonDefaultEnumValue.Should().NotBe(defaultEnumValue);
            jsonSerializedEnum.Should().Contain($"\"{nonDefaultEnumValue}\"");
        }

        [Fact]
        public void SerializationOfOrderedProperties()
        {
            var obj = new OrderExample();
            var writer = new StringWriter();

            Serializer.Serialize(writer, obj);
            var serialized = writer.ToString();

            serialized.Should()
                .Be("Order1: Order1 value\r\nOrder2: Order2 value\r\n".NormalizeNewLines(), "the properties should be in the right order");
        }

        [Fact]
        public void SerializationRespectsYamlIgnoreAttribute()
        {

            var writer = new StringWriter();
            var obj = new IgnoreExample();

            Serializer.Serialize(writer, obj);
            var serialized = writer.ToString();

            serialized.Should().NotContain("IgnoreMe");
        }

        [Fact]
        public void SerializationRespectsYamlIgnoreAttributeOfDerivedClasses()
        {

            var writer = new StringWriter();
            var obj = new IgnoreExampleDerived();

            Serializer.Serialize(writer, obj);
            var serialized = writer.ToString();

            serialized.Should().NotContain("IgnoreMe");
        }

        [Fact]
        public void SerializationRespectsYamlIgnoreOverride()
        {

            var writer = new StringWriter();
            var obj = new Simple();

            var ignore = new YamlIgnoreAttribute();
            var serializer = new SerializerBuilder()
                .WithAttributeOverride<Simple>(s => s.aaa, ignore)
                .Build();

            serializer.Serialize(writer, obj);
            var serialized = writer.ToString();

            serialized.Should().NotContain("aaa");
        }

        [Fact]
        public void SerializationRespectsScalarStyle()
        {
            var writer = new StringWriter();
            var obj = new ScalarStyleExample();

            Serializer.Serialize(writer, obj);
            var serialized = writer.ToString();

            serialized.Should()
                .Be("LiteralString: |-\r\n  Test\r\nDoubleQuotedString: \"Test\"\r\n".NormalizeNewLines(), "the properties should be specifically styled");
        }

        [Fact]
        public void SerializationRespectsScalarStyleOverride()
        {
            var writer = new StringWriter();
            var obj = new ScalarStyleExample();

            var serializer = new SerializerBuilder()
                .WithAttributeOverride<ScalarStyleExample>(e => e.LiteralString, new YamlMemberAttribute { ScalarStyle = ScalarStyle.DoubleQuoted })
                .WithAttributeOverride<ScalarStyleExample>(e => e.DoubleQuotedString, new YamlMemberAttribute { ScalarStyle = ScalarStyle.Literal })
                .Build();

            serializer.Serialize(writer, obj);
            var serialized = writer.ToString();

            serialized.Should()
                .Be("LiteralString: \"Test\"\r\nDoubleQuotedString: |-\r\n  Test\r\n".NormalizeNewLines(), "the properties should be specifically styled");
        }

        [Fact]
        public void SerializationRespectsDefaultScalarStyle()
        {
            var writer = new StringWriter();
            var obj = new MixedFormatScalarStyleExample(new string[] { "01", "0.1", "myString" });

            var serializer = new SerializerBuilder().WithDefaultScalarStyle(ScalarStyle.SingleQuoted).Build();

            serializer.Serialize(writer, obj);

            var yaml = writer.ToString();

            var expected = Yaml.Text(@"
                Data:
                - '01'
                - '0.1'
                - 'myString'
            ");

            Assert.Equal(expected.NormalizeNewLines(), yaml.NormalizeNewLines().TrimNewLines());
        }

        [Fact]
        public void SerializationDerivedAttributeOverride()
        {
            var writer = new StringWriter();
            var obj = new Derived { DerivedProperty = "Derived", BaseProperty = "Base" };

            var ignore = new YamlIgnoreAttribute();
            var serializer = new SerializerBuilder()
                .WithAttributeOverride<Derived>(d => d.DerivedProperty, ignore)
                .Build();

            serializer.Serialize(writer, obj);
            var serialized = writer.ToString();

            serialized.Should()
                .Be("BaseProperty: Base\r\n".NormalizeNewLines(), "the derived property should be specifically ignored");
        }

        [Fact]
        public void SerializationBaseAttributeOverride()
        {
            var writer = new StringWriter();
            var obj = new Derived { DerivedProperty = "Derived", BaseProperty = "Base" };

            var ignore = new YamlIgnoreAttribute();
            var serializer = new SerializerBuilder()
                .WithAttributeOverride<Base>(b => b.BaseProperty, ignore)
                .Build();

            serializer.Serialize(writer, obj);
            var serialized = writer.ToString();

            serialized.Should()
                .Be("DerivedProperty: Derived\r\n".NormalizeNewLines(), "the base property should be specifically ignored");
        }

        [Fact]
        public void SerializationSkipsPropertyWhenUsingDefaultValueAttribute()
        {
            var writer = new StringWriter();
            var obj = new DefaultsExample { Value = DefaultsExample.DefaultValue };

            SerializerBuilder
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults);

            Serializer.Serialize(writer, obj);
            var serialized = writer.ToString();

            serialized.Should().NotContain("Value");
        }

        [Fact]
        public void SerializationEmitsPropertyWhenUsingEmitDefaultsAndDefaultValueAttribute()
        {
            var writer = new StringWriter();
            var obj = new DefaultsExample { Value = DefaultsExample.DefaultValue };

            SerializerBuilder.Build().Serialize(writer, obj);
            var serialized = writer.ToString();

            serialized.Should().Contain("Value");
        }

        [Fact]
        public void SerializationEmitsPropertyWhenValueDifferFromDefaultValueAttribute()
        {
            var writer = new StringWriter();
            var obj = new DefaultsExample { Value = "non-default" };

            Serializer.Serialize(writer, obj);
            var serialized = writer.ToString();

            serialized.Should().Contain("Value");
        }

        [Fact]
        public void SerializationHandlesException()
        {
            var obj = new ThrowingPropertyExample();
            var serializer = new SerializerBuilder()
                    .WithExceptionHandler((e, o, p) =>
                        $"Exception of type {e.GetType().FullName} was thrown in property {p} " +
                        "of " + (ReferenceEquals(o, obj) ? "expected" : "unexpected") + " object")
                .Build();
            var writer = new StringWriter();

            serializer.Serialize(writer, obj);
            var serialized = writer.ToString();

            serialized.Should().Be(
                "Value: Exception of type System.InvalidOperationException was thrown in property Value of expected object\r\n"
                    .NormalizeNewLines());
        }

        [Fact]
        public void SerializationDoesntHandleExceptionByDefault()
        {
            var writer = new StringWriter();
            var obj = new ThrowingPropertyExample();

            Assert.Throws<TargetInvocationException>(() => Serializer.Serialize(writer, obj));
        }

        [Fact]
        public void SerializingAGenericDictionaryShouldNotThrowTargetException()
        {
            var obj = new CustomGenericDictionary {
                { "hello", "world" }
            };

            Action action = () => Serializer.Serialize(new StringWriter(), obj);

            action.ShouldNotThrow<TargetException>();
        }

        [Fact]
        public void SerializationUtilizeNamingConventions()
        {
            var convention = A.Fake<INamingConvention>();
            A.CallTo(() => convention.Apply(A<string>._)).ReturnsLazily((string x) => x);
            var obj = new NameConvention { FirstTest = "1", SecondTest = "2" };

            var serializer = new SerializerBuilder()
                .WithNamingConvention(convention)
                .Build();

            serializer.Serialize(new StringWriter(), obj);

            A.CallTo(() => convention.Apply("FirstTest")).MustHaveHappened();
            A.CallTo(() => convention.Apply("SecondTest")).MustHaveHappened();
        }

        [Fact]
        public void DeserializationUtilizeNamingConventions()
        {
            var convention = A.Fake<INamingConvention>();
            A.CallTo(() => convention.Apply(A<string>._)).ReturnsLazily((string x) => x);
            var text = Lines(
                "FirstTest: 1",
                "SecondTest: 2");

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(convention)
                .Build();

            deserializer.Deserialize<NameConvention>(UsingReaderFor(text));

            A.CallTo(() => convention.Apply("FirstTest")).MustHaveHappened();
            A.CallTo(() => convention.Apply("SecondTest")).MustHaveHappened();
        }

        [Fact]
        public void TypeConverterIsUsedOnListItems()
        {
            var text = Lines(
                "- !{type}",
                "  Left: hello",
                "  Right: world")
                .TemplatedOn<Convertible>();

            var list = new DeserializerBuilder()
                .WithTagMapping("!Convertible", typeof(Convertible))
                .Build()
                .Deserialize<List<string>>(UsingReaderFor(text));

            list
                .Should().NotBeNull()
                .And.ContainSingle(c => c.Equals("[hello, world]"));
        }

        [Fact]
        public void BackreferencesAreMergedWithMappings()
        {
            var stream = Yaml.ReaderFrom("backreference.yaml");

            var parser = new MergingParser(new Parser(stream));
            var result = Deserializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(parser);

            var alias = result["alias"];
            alias.Should()
                .Contain("key1", "value1", "key1 should be inherited from the backreferenced mapping")
                .And.Contain("key2", "Overriding key2", "key2 should be overriden by the actual mapping")
                .And.Contain("key3", "value3", "key3 is defined in the actual mapping");
        }

        [Fact]
        public void MergingDoesNotProduceDuplicateAnchors()
        {
            var parser = new MergingParser(Yaml.ParserForText(@"
                anchor: &default
                  key1: &myValue value1
                  key2: value2
                alias:
                  <<: *default
                  key2: Overriding key2
                  key3: value3
                useMyValue:
                  key: *myValue
            "));
            var result = Deserializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(parser);

            var alias = result["alias"];
            alias.Should()
                .Contain("key1", "value1", "key1 should be inherited from the backreferenced mapping")
                .And.Contain("key2", "Overriding key2", "key2 should be overriden by the actual mapping")
                .And.Contain("key3", "value3", "key3 is defined in the actual mapping");

            result["useMyValue"].Should()
                .Contain("key", "value1", "key should be copied");
        }

        [Fact]
        public void ExampleFromSpecificationIsHandledCorrectly()
        {
            var parser = new MergingParser(Yaml.ParserForText(@"
                obj:
                  - &CENTER { x: 1, y: 2 }
                  - &LEFT { x: 0, y: 2 }
                  - &BIG { r: 10 }
                  - &SMALL { r: 1 }

                # All the following maps are equal:
                results:
                  - # Explicit keys
                    x: 1
                    y: 2
                    r: 10
                    label: center/big

                  - # Merge one map
                    << : *CENTER
                    r: 10
                    label: center/big

                  - # Merge multiple maps
                    << : [ *CENTER, *BIG ]
                    label: center/big

                  - # Override
                    << : [ *BIG, *LEFT, *SMALL ]
                    x: 1
                    label: center/big
            "));

            var result = Deserializer.Deserialize<Dictionary<string, List<Dictionary<string, string>>>>(parser);

            var index = 0;
            foreach (var mapping in result["results"])
            {
                mapping.Should()
                    .Contain("x", "1", "'x' should be '1' in result #{0}", index)
                    .And.Contain("y", "2", "'y' should be '2' in result #{0}", index)
                    .And.Contain("r", "10", "'r' should be '10' in result #{0}", index)
                    .And.Contain("label", "center/big", "'label' should be 'center/big' in result #{0}", index);

                ++index;
            }
        }

        [Fact]
        public void MergeNestedReferenceCorrectly()
        {
            var parser = new MergingParser(Yaml.ParserForText(@"
                base1: &level1
                  key: X
                  level: 1
                base2: &level2
                  <<: *level1
                  key: Y
                  level: 2
                derived1:
                  <<: *level1
                  key: D1
                derived2:
                  <<: *level2
                  key: D2
                derived3:
                  <<: [ *level1, *level2 ]
                  key: D3
            "));

            var result = Deserializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(parser);

            result["derived1"].Should()
                .Contain("key", "D1", "key should be overriden by the actual mapping")
                .And.Contain("level", "1", "level should be inherited from the backreferenced mapping");

            result["derived2"].Should()
                .Contain("key", "D2", "key should be overriden by the actual mapping")
                .And.Contain("level", "2", "level should be inherited from the backreferenced mapping");

            result["derived3"].Should()
                .Contain("key", "D3", "key should be overriden by the actual mapping")
                .And.Contain("level", "1", "level should be inherited from the backreferenced mapping");
        }

        [Fact]
        public void IgnoreExtraPropertiesIfWanted()
        {
            var text = Lines("aaa: hello", "bbb: world");
            DeserializerBuilder.IgnoreUnmatchedProperties();
            var actual = Deserializer.Deserialize<Simple>(UsingReaderFor(text));
            actual.aaa.Should().Be("hello");
        }

        [Fact]
        public void DontIgnoreExtraPropertiesIfWanted()
        {
            var text = Lines("aaa: hello", "bbb: world");
            var actual = Record.Exception(() => Deserializer.Deserialize<Simple>(UsingReaderFor(text)));
            Assert.IsType<YamlException>(actual);
            ((YamlException)actual).Start.Column.Should().Be(1);
            ((YamlException)actual).Start.Line.Should().Be(2);
            ((YamlException)actual).Start.Index.Should().Be(12);
            ((YamlException)actual).End.Column.Should().Be(4);
            ((YamlException)actual).End.Line.Should().Be(2);
            ((YamlException)actual).End.Index.Should().Be(15);
            ((YamlException)actual).Message.Should().Be("Property 'bbb' not found on type 'YamlDotNet.Test.Serialization.Simple'.");
        }

        [Fact]
        public void IgnoreExtraPropertiesIfWantedBefore()
        {
            var text = Lines("bbb: [200,100]", "aaa: hello");
            DeserializerBuilder.IgnoreUnmatchedProperties();
            var actual = Deserializer.Deserialize<Simple>(UsingReaderFor(text));
            actual.aaa.Should().Be("hello");
        }

        [Fact]
        public void IgnoreExtraPropertiesIfWantedNamingScheme()
        {
            var text = Lines(
                    "scratch: 'scratcher'",
                    "deleteScratch: false",
                    "notScratch: 9443",
                    "notScratch: 192.168.1.30",
                    "mappedScratch:",
                    "- '/work/'"
                );

            DeserializerBuilder
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties();

            var actual = Deserializer.Deserialize<SimpleScratch>(UsingReaderFor(text));
            actual.Scratch.Should().Be("scratcher");
            actual.DeleteScratch.Should().Be(false);
            actual.MappedScratch.Should().ContainInOrder(new[] { "/work/" });
        }

        [Fact]
        public void InvalidTypeConversionsProduceProperExceptions()
        {
            var text = Lines("- 1", "- two", "- 3");

            var sut = new Deserializer();
            var exception = Assert.Throws<YamlException>(() => sut.Deserialize<List<int>>(UsingReaderFor(text)));

            Assert.Equal(2, exception.Start.Line);
            Assert.Equal(3, exception.Start.Column);
        }

        [Theory]
        [InlineData("blah")]
        [InlineData("hello=world")]
        [InlineData("+190:20:30")]
        [InlineData("x:y")]
        public void ValueAllowedAfterDocumentStartToken(string text)
        {
            var value = Lines("--- " + text);

            var sut = new Deserializer();
            var actual = sut.Deserialize<string>(UsingReaderFor(value));

            Assert.Equal(text, actual);
        }

        [Fact]
        public void MappingDisallowedAfterDocumentStartToken()
        {
            var value = Lines("--- x: y");

            var sut = new Deserializer();
            var exception = Assert.Throws<SemanticErrorException>(() => sut.Deserialize<string>(UsingReaderFor(value)));

            Assert.Equal(1, exception.Start.Line);
            Assert.Equal(6, exception.Start.Column);
        }

        [Fact]
        public void SerializeDynamicPropertyAndApplyNamingConvention()
        {
            dynamic obj = new ExpandoObject();
            obj.property_one = new ExpandoObject();
            ((IDictionary<string, object>)obj.property_one).Add("new_key_here", "new_value");

            var mockNamingConvention = A.Fake<INamingConvention>();
            A.CallTo(() => mockNamingConvention.Apply(A<string>.Ignored)).Returns("xxx");

            var serializer = new SerializerBuilder()
                .WithNamingConvention(mockNamingConvention)
                .Build();

            var writer = new StringWriter();
            serializer.Serialize(writer, obj);

            writer.ToString().Should().Contain("xxx: new_value");
        }

        [Fact]
        public void SerializeGenericDictionaryPropertyAndDoNotApplyNamingConvention()
        {
            var obj = new Dictionary<string, object>
            {
                ["property_one"] = new GenericTestDictionary<string, object>()
            };

            ((IDictionary<string, object>)obj["property_one"]).Add("new_key_here", "new_value");

            var mockNamingConvention = A.Fake<INamingConvention>();
            A.CallTo(() => mockNamingConvention.Apply(A<string>.Ignored)).Returns("xxx");

            var serializer = new SerializerBuilder()
                .WithNamingConvention(mockNamingConvention)
                .Build();

            var writer = new StringWriter();
            serializer.Serialize(writer, obj);

            writer.ToString().Should().Contain("new_key_here: new_value");
        }

        [Theory, MemberData(nameof(SpecialFloats))]
        public void SpecialFloatsAreHandledCorrectly(FloatTestCase testCase)
        {
            var buffer = new StringWriter();
            Serializer.Serialize(buffer, testCase.Value);

            var firstLine = buffer.ToString().Split('\r', '\n')[0];
            Assert.Equal(testCase.ExpectedTextRepresentation, firstLine);

            var deserializer = new Deserializer();
            var deserializedValue = deserializer.Deserialize(new StringReader(buffer.ToString()), testCase.Value.GetType());

            Assert.Equal(testCase.Value, deserializedValue);
        }

        [Theory]
        [InlineData(TestEnum.True)]
        [InlineData(TestEnum.False)]
        [InlineData(TestEnum.ABC)]
        [InlineData(TestEnum.Null)]
        public void RoundTripSpecialEnum(object testValue)
        {
            var test = new TestEnumTestCase { TestEnum = (TestEnum)testValue };
            var serializer = new SerializerBuilder().WithQuotingNecessaryStrings().Build();
            var deserializer = new DeserializerBuilder().Build();
            var serialized = serializer.Serialize(test);
            var actual = deserializer.Deserialize<TestEnumTestCase>(serialized);
            Assert.Equal(testValue, actual.TestEnum);
        }

        [Fact]
        public void EmptyStringsAreQuoted()
        {
            var serializer = new SerializerBuilder().WithQuotingNecessaryStrings().Build();
            var o = new { test = string.Empty };
            var result = serializer.Serialize(o);
            var expected = $"test: \"\"{Environment.NewLine}";
            Assert.Equal(expected, result);
        }

        public enum TestEnum
        {
            True,
            False,
            ABC,
            Null
        }

        public class TestEnumTestCase
        {
            public TestEnum TestEnum { get; set; }
        }

        public class FloatTestCase
        {
            private readonly string description;
            public object Value { get; private set; }
            public string ExpectedTextRepresentation { get; private set; }

            public FloatTestCase(string description, object value, string expectedTextRepresentation)
            {
                this.description = description;
                Value = value;
                ExpectedTextRepresentation = expectedTextRepresentation;
            }

            public override string ToString()
            {
                return description;
            }
        }

        public static IEnumerable<object[]> SpecialFloats
        {
            get
            {
                return
                    new[]
                    {
                        new FloatTestCase("double.NaN", double.NaN, ".nan"),
                        new FloatTestCase("double.PositiveInfinity", double.PositiveInfinity, ".inf"),
                        new FloatTestCase("double.NegativeInfinity", double.NegativeInfinity, "-.inf"),
                        new FloatTestCase("double.Epsilon", double.Epsilon,  double.Epsilon.ToString("G", CultureInfo.InvariantCulture)),
                        new FloatTestCase("double.26.67", 26.67D, "26.67"),

                        new FloatTestCase("float.NaN", float.NaN, ".nan"),
                        new FloatTestCase("float.PositiveInfinity", float.PositiveInfinity, ".inf"),
                        new FloatTestCase("float.NegativeInfinity", float.NegativeInfinity, "-.inf"),
                        new FloatTestCase("float.Epsilon", float.Epsilon, float.Epsilon.ToString("G", CultureInfo.InvariantCulture)),
                        new FloatTestCase("float.26.67", 26.67F, "26.67"),

#if NETCOREAPP3_1_OR_GREATER
                        new FloatTestCase("double.MinValue", double.MinValue, double.MinValue.ToString("G", CultureInfo.InvariantCulture)),
                        new FloatTestCase("double.MaxValue", double.MaxValue, double.MaxValue.ToString("G", CultureInfo.InvariantCulture)),
                        new FloatTestCase("float.MinValue", float.MinValue, float.MinValue.ToString("G", CultureInfo.InvariantCulture)),
                        new FloatTestCase("float.MaxValue", float.MaxValue, float.MaxValue.ToString("G", CultureInfo.InvariantCulture)),
#endif
                    }
                    .Select(tc => new object[] { tc });
            }
        }

        [Fact]
        public void NegativeIntegersCanBeDeserialized()
        {
            var deserializer = new Deserializer();

            var value = deserializer.Deserialize<int>(Yaml.ReaderForText(@"
                '-123'
            "));
            Assert.Equal(-123, value);
        }

        [Fact]
        public void GenericDictionaryThatDoesNotImplementIDictionaryCanBeDeserialized()
        {
            var sut = new Deserializer();
            var deserialized = sut.Deserialize<GenericTestDictionary<string, string>>(Yaml.ReaderForText(@"
                a: 1
                b: 2
            "));

            Assert.Equal("1", deserialized["a"]);
            Assert.Equal("2", deserialized["b"]);
        }

        [Fact]
        public void GenericListThatDoesNotImplementIListCanBeDeserialized()
        {
            var sut = new Deserializer();
            var deserialized = sut.Deserialize<GenericTestList<string>>(Yaml.ReaderForText(@"
                - a
                - b
            "));

            Assert.Contains("a", deserialized);
            Assert.Contains("b", deserialized);
        }

        [Fact]
        public void GuidsShouldBeQuotedWhenSerializedAsJson()
        {
            var sut = new SerializerBuilder()
                .JsonCompatible()
                .Build();

            var yamlAsJson = new StringWriter();
            sut.Serialize(yamlAsJson, new
            {
                id = Guid.Empty
            });

            Assert.Contains("\"00000000-0000-0000-0000-000000000000\"", yamlAsJson.ToString());
        }

        public class Foo
        {
            public bool IsRequired { get; set; }
        }

        [Fact]
        public void AttributeOverridesAndNamingConventionDoNotConflict()
        {
            var namingConvention = CamelCaseNamingConvention.Instance;

            var yamlMember = new YamlMemberAttribute
            {
                Alias = "Required"
            };

            var serializer = new SerializerBuilder()
                .WithNamingConvention(namingConvention)
                .WithAttributeOverride<Foo>(f => f.IsRequired, yamlMember)
                .Build();

            var yaml = serializer.Serialize(new Foo { IsRequired = true });
            Assert.Contains("required: true", yaml);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(namingConvention)
                .WithAttributeOverride<Foo>(f => f.IsRequired, yamlMember)
                .Build();

            var deserializedFoo = deserializer.Deserialize<Foo>(yaml);
            Assert.True(deserializedFoo.IsRequired);
        }

        [Fact]
        public void YamlConvertiblesAreAbleToEmitAndParseComments()
        {
            var serializer = new Serializer();
            var yaml = serializer.Serialize(new CommentWrapper<string> { Comment = "A comment", Value = "The value" });

            var deserializer = new Deserializer();
            var parser = new Parser(new Scanner(new StringReader(yaml), skipComments: false));
            var parsed = deserializer.Deserialize<CommentWrapper<string>>(parser);

            Assert.Equal("A comment", parsed.Comment);
            Assert.Equal("The value", parsed.Value);
        }

        public class CommentWrapper<T> : IYamlConvertible
        {
            public string Comment { get; set; }
            public T Value { get; set; }

            public void Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
            {
                if (parser.TryConsume<Comment>(out var comment))
                {
                    Comment = comment.Value;
                }

                Value = (T)nestedObjectDeserializer(typeof(T));
            }

            public void Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
            {
                if (!string.IsNullOrEmpty(Comment))
                {
                    emitter.Emit(new Comment(Comment, false));
                }

                nestedObjectSerializer(Value, typeof(T));
            }
        }

        [Theory]
        [InlineData(uint.MinValue)]
        [InlineData(uint.MaxValue)]
        [InlineData(0x8000000000000000UL)]
        public void DeserializationOfUInt64Succeeds(ulong value)
        {
            var yaml = new Serializer().Serialize(value);
            Assert.Contains(value.ToString(), yaml);

            var parsed = new Deserializer().Deserialize<ulong>(yaml);
            Assert.Equal(value, parsed);
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(0L)]
        public void DeserializationOfInt64Succeeds(long value)
        {
            var yaml = new Serializer().Serialize(value);
            Assert.Contains(value.ToString(), yaml);

            var parsed = new Deserializer().Deserialize<long>(yaml);
            Assert.Equal(value, parsed);
        }

        public class AnchorsOverwritingTestCase
        {
            public List<string> a { get; set; }
            public List<string> b { get; set; }
            public List<string> c { get; set; }
            public List<string> d { get; set; }
        }

        [Fact]
        public void DeserializationOfStreamWithDuplicateAnchorsSucceeds()
        {
            var yaml = Yaml.ParserForResource("anchors-overwriting.yaml");
            var serializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();
            var deserialized = serializer.Deserialize<AnchorsOverwritingTestCase>(yaml);
            Assert.NotNull(deserialized);
        }

        private sealed class AnchorPrecedence
        {
            internal sealed class AnchorPrecedenceNested
            {
                public string b1 { get; set; }
                public Dictionary<string, string> b2 { get; set; }
            }

            public string a { get; set; }
            public AnchorPrecedenceNested b { get; set; }
            public string c { get; set; }
        }

        [Fact]
        public void DeserializationWithDuplicateAnchorsSucceeds()
        {
            var sut = new Deserializer();
            var deserialized = sut.Deserialize<AnchorPrecedence>(@"
a: &anchor1 test0
b:
  b1: &anchor1 test1
  b2:
    b21:  &anchor1 test2
c:  *anchor1");

            Assert.Equal("test0", deserialized.a);
            Assert.Equal("test1", deserialized.b.b1);
            Assert.Contains("b21", deserialized.b.b2.Keys);
            Assert.Equal("test2", deserialized.b.b2["b21"]);
            Assert.Equal("test2", deserialized.c);
        }

        [Fact]
        public void SerializeExceptionWithStackTrace()
        {
            var ex = GetExceptionWithStackTrace();
            var serializer = new SerializerBuilder()
                .WithTypeConverter(new MethodInfoConverter())
                .Build();
            var yaml = serializer.Serialize(ex);
            Assert.Contains("GetExceptionWithStackTrace", yaml);
        }

        private class MethodInfoConverter : IYamlTypeConverter
        {
            public bool Accepts(Type type)
            {
                return typeof(MethodInfo).IsAssignableFrom(type);
            }

            public object ReadYaml(IParser parser, Type type)
            {
                throw new NotImplementedException();
            }

            public void WriteYaml(IEmitter emitter, object value, Type type)
            {
                var method = (MethodInfo)value;
                emitter.Emit(new Scalar(string.Format("{0}.{1}", method.DeclaringType.FullName, method.Name)));
            }
        }

        static Exception GetExceptionWithStackTrace()
        {
            try
            {
                throw new ArgumentNullException("foo");
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        [Fact]
        public void RegisteringATypeConverterPreventsTheTypeFromBeingVisited()
        {
            var serializer = new SerializerBuilder()
                .WithTypeConverter(new NonSerializableTypeConverter())
                .Build();

            var yaml = serializer.Serialize(new NonSerializableContainer
            {
                Value = new NonSerializable { Text = "hello" },
            });

            var deserializer = new DeserializerBuilder()
                .WithTypeConverter(new NonSerializableTypeConverter())
                .Build();

            var result = deserializer.Deserialize<NonSerializableContainer>(yaml);

            Assert.Equal("hello", result.Value.Text);
        }

        [Fact]
        public void NamingConventionIsNotAppliedBySerializerWhenApplyNamingConventionsIsFalse()
        {
            var sut = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yaml = sut.Serialize(new NamingConventionDisabled { NoConvention = "value" });

            Assert.Contains("NoConvention", yaml);
        }

        [Fact]
        public void NamingConventionIsNotAppliedByDeserializerWhenApplyNamingConventionsIsFalse()
        {
            var sut = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yaml = "NoConvention: value";

            var parsed = sut.Deserialize<NamingConventionDisabled>(yaml);

            Assert.Equal("value", parsed.NoConvention);
        }

        [Fact]
        public void TypesAreSerializable()
        {
            var sut = new SerializerBuilder()
                .Build();

            var yaml = sut.Serialize(typeof(string));

            Assert.Contains(typeof(string).AssemblyQualifiedName, yaml);
        }

        [Fact]
        public void TypesAreDeserializable()
        {
            var sut = new DeserializerBuilder()
                .Build();

            var type = sut.Deserialize<Type>(typeof(string).AssemblyQualifiedName);

            Assert.Equal(typeof(string), type);
        }

        [Fact]
        public void TypesAreConvertedWhenNeededFromScalars()
        {
            var sut = new DeserializerBuilder()
                .WithTagMapping("!dbl", typeof(DoublyConverted))
                .Build();

            var result = sut.Deserialize<int>("!dbl hello");

            Assert.Equal(5, result);
        }

        [Fact]
        public void TypesAreConvertedWhenNeededInsideLists()
        {
            var sut = new DeserializerBuilder()
                .WithTagMapping("!dbl", typeof(DoublyConverted))
                .Build();

            var result = sut.Deserialize<List<int>>("- !dbl hello");

            Assert.Equal(5, result[0]);
        }

        [Fact]
        public void TypesAreConvertedWhenNeededInsideDictionary()
        {
            var sut = new DeserializerBuilder()
                .WithTagMapping("!dbl", typeof(DoublyConverted))
                .Build();

            var result = sut.Deserialize<Dictionary<int, int>>("!dbl hello: !dbl you");

            Assert.True(result.ContainsKey(5));
            Assert.Equal(3, result[5]);
        }

        [Fact]
        public void InfiniteRecursionIsDetected()
        {
            var sut = new SerializerBuilder()
                .DisableAliases()
                .Build();

            var recursionRoot = new
            {
                Nested = new[]
                {
                    new Dictionary<string, object>()
                }
            };

            recursionRoot.Nested[0].Add("loop", recursionRoot);

            var exception = Assert.Throws<MaximumRecursionLevelReachedException>(() => sut.Serialize(recursionRoot));
        }

        [Fact]
        public void TuplesAreSerializable()
        {
            var sut = new SerializerBuilder()
                .Build();

            var yaml = sut.Serialize(new[]
            {
                Tuple.Create(1, "one"),
                Tuple.Create(2, "two"),
            });

            var expected = Yaml.Text(@"
                - Item1: 1
                  Item2: one
                - Item1: 2
                  Item2: two
            ");

            Assert.Equal(expected.NormalizeNewLines(), yaml.NormalizeNewLines().TrimNewLines());
        }

        [Fact]
        public void ValueTuplesAreSerializableWithoutMetadata()
        {
            var sut = new SerializerBuilder()
                .Build();

            var yaml = sut.Serialize(new[]
            {
                (num: 1, txt: "one"),
                (num: 2, txt: "two"),
            });

            var expected = Yaml.Text(@"
                - Item1: 1
                  Item2: one
                - Item1: 2
                  Item2: two
            ");

            Assert.Equal(expected.NormalizeNewLines(), yaml.NormalizeNewLines().TrimNewLines());
        }

        [Fact]
        public void AnchorNameWithTrailingColonReferencedInKeyCanBeDeserialized()
        {
            var sut = new Deserializer();
            var deserialized = sut.Deserialize<GenericTestDictionary<string, string>>(Yaml.ReaderForText(@"
                a: &::::scaryanchor:::: anchor "" value ""
                *::::scaryanchor::::: 2
                myvalue: *::::scaryanchor::::
            "));

            Assert.Equal(@"anchor "" value """, deserialized["a"]);
            Assert.Equal("2", deserialized[@"anchor "" value """]);
            Assert.Equal(@"anchor "" value """, deserialized["myvalue"]);
        }

        [Fact]
        public void AliasBeforeAnchorCannotBeDeserialized()
        {
            var sut = new Deserializer();
            Action action = () => sut.Deserialize<GenericTestDictionary<string, string>>(@"
a: *anchor1
b: &anchor1 test0
c: *anchor1");

            action.ShouldThrow<AnchorNotFoundException>();
        }

        [Fact]
        public void AnchorWithAllowedCharactersCanBeDeserialized()
        {
            var sut = new Deserializer();
            var deserialized = sut.Deserialize<GenericTestDictionary<string, string>>(Yaml.ReaderForText(@"
                a: &@nchor<>""@-_123$>>>😁🎉🐻🍔end some value
                myvalue: my *@nchor<>""@-_123$>>>😁🎉🐻🍔end test
                interpolated value: *@nchor<>""@-_123$>>>😁🎉🐻🍔end
            "));

            Assert.Equal("some value", deserialized["a"]);
            Assert.Equal(@"my *@nchor<>""@-_123$>>>😁🎉🐻🍔end test", deserialized["myvalue"]);
            Assert.Equal("some value", deserialized["interpolated value"]);
        }

        [Fact]
        public void SerializationNonPublicPropertiesAreIgnored()
        {
            var sut = new SerializerBuilder().Build();
            var yaml = sut.Serialize(new NonPublicPropertiesExample());
            Assert.Equal("Public: public", yaml.TrimNewLines());
        }

        [Fact]
        public void SerializationNonPublicPropertiesAreIncluded()
        {
            var sut = new SerializerBuilder().IncludeNonPublicProperties().Build();
            var yaml = sut.Serialize(new NonPublicPropertiesExample());

            var expected = Yaml.Text(@"
                Public: public
                Internal: internal
                Protected: protected
                Private: private
            ");

            Assert.Equal(expected.NormalizeNewLines(), yaml.NormalizeNewLines().TrimNewLines());
        }

        [Fact]
        public void DeserializationNonPublicPropertiesAreIgnored()
        {
            var sut = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
            var deserialized = sut.Deserialize<NonPublicPropertiesExample>(Yaml.ReaderForText(@"
                Public: public2
                Internal: internal2
                Protected: protected2
                Private: private2
            "));

            Assert.Equal("public2,internal,protected,private", deserialized.ToString());
        }

        [Fact]
        public void DeserializationNonPublicPropertiesAreIncluded()
        {
            var sut = new DeserializerBuilder().IncludeNonPublicProperties().Build();
            var deserialized = sut.Deserialize<NonPublicPropertiesExample>(Yaml.ReaderForText(@"
                Public: public2
                Internal: internal2
                Protected: protected2
                Private: private2
            "));

            Assert.Equal("public2,internal2,protected2,private2", deserialized.ToString());
        }

        [Fact]
        public void SerializationNonPublicFieldsAreIgnored()
        {
            var sut = new SerializerBuilder().Build();
            var yaml = sut.Serialize(new NonPublicFieldsExample());
            Assert.Equal("Public: public", yaml.TrimNewLines());
        }

        [Fact]
        public void DeserializationNonPublicFieldsAreIgnored()
        {
            var sut = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
            var deserialized = sut.Deserialize<NonPublicFieldsExample>(Yaml.ReaderForText(@"
                Public: public2
                Internal: internal2
                Protected: protected2
                Private: private2
            "));

            Assert.Equal("public2,internal,protected,private", deserialized.ToString());
        }

        [Fact]
        public void ShouldNotIndentSequences()
        {
            var sut = new SerializerBuilder()
                .Build();

            var yaml = sut.Serialize(new
            {
                first = "first",
                items = new[]
                {
                    "item1",
                    "item2"
                },
                nested = new[]
                {
                    new
                    {
                        name = "name1",
                        more = new[]
                        {
                            "nested1",
                            "nested2"
                        }
                    }
                }
            });

            var expected = Yaml.Text(@"
                first: first
                items:
                - item1
                - item2
                nested:
                - name: name1
                  more:
                  - nested1
                  - nested2
            ");

            Assert.Equal(expected.NormalizeNewLines(), yaml.NormalizeNewLines().TrimNewLines());
        }

        [Fact]
        public void ShouldIndentSequences()
        {
            var sut = new SerializerBuilder()
                .WithIndentedSequences()
                .Build();

            var yaml = sut.Serialize(new
            {
                first = "first",
                items = new[]
                {
                    "item1",
                    "item2"
                },
                nested = new[]
                {
                    new
                    {
                        name = "name1",
                        more = new[]
                        {
                            "nested1",
                            "nested2"
                        }
                    }
                }
            });

            var expected = Yaml.Text(@"
                first: first
                items:
                  - item1
                  - item2
                nested:
                  - name: name1
                    more:
                      - nested1
                      - nested2
            ");

            Assert.Equal(expected.NormalizeNewLines(), yaml.NormalizeNewLines().TrimNewLines());
        }

        [Fact]
        public void ExampleFromSpecificationIsHandledCorrectlyWithLateDefine()
        {
            var parser = new MergingParser(Yaml.ParserForText(@"
                # All the following maps are equal:
                results:
                  - # Explicit keys
                    x: 1
                    y: 2
                    r: 10
                    label: center/big

                  - # Merge one map
                    << : *CENTER
                    r: 10
                    label: center/big

                  - # Merge multiple maps
                    << : [ *CENTER, *BIG ]
                    label: center/big

                  - # Override
                    << : [ *BIG, *LEFT, *SMALL ]
                    x: 1
                    label: center/big

                obj:
                  - &CENTER { x: 1, y: 2 }
                  - &LEFT { x: 0, y: 2 }
                  - &SMALL { r: 1 }
                  - &BIG { r: 10 }
            "));

            var result = Deserializer.Deserialize<Dictionary<string, List<Dictionary<string, string>>>>(parser);

            int index = 0;
            foreach (var mapping in result["results"])
            {
                mapping.Should()
                    .Contain("x", "1", "'x' should be '1' in result #{0}", index)
                    .And.Contain("y", "2", "'y' should be '2' in result #{0}", index)
                    .And.Contain("r", "10", "'r' should be '10' in result #{0}", index)
                    .And.Contain("label", "center/big", "'label' should be 'center/big' in result #{0}", index);

                ++index;
            }
        }

        public class CycleTestEntity
        {
            public CycleTestEntity Cycle { get; set; }
        }

        [Fact]
        public void SerializeCycleWithAlias()
        {
            var sut = new SerializerBuilder()
                .WithTagMapping("!CycleTag", typeof(CycleTestEntity))
                .Build();

            var entity = new CycleTestEntity();
            entity.Cycle = entity;
            var yaml = sut.Serialize(entity);
            var expected = Yaml.Text(@"&o0 !CycleTag
Cycle: *o0");

            Assert.Equal(expected.NormalizeNewLines(), yaml.NormalizeNewLines().TrimNewLines());
        }

        [Fact]
        public void DeserializeCycleWithAlias()
        {
            var sut = new DeserializerBuilder()
                .WithTagMapping("!CycleTag", typeof(CycleTestEntity))
                .Build();

            var yaml = Yaml.Text(@"&o0 !CycleTag
Cycle: *o0");
            var obj = sut.Deserialize<CycleTestEntity>(yaml);

            Assert.Same(obj, obj.Cycle);
        }

        [Fact]
        public void DeserializeCycleWithoutAlias()
        {
            var sut = new DeserializerBuilder()
                .Build();

            var yaml = Yaml.Text(@"&o0
Cycle: *o0");
            var obj = sut.Deserialize<CycleTestEntity>(yaml);

            Assert.Same(obj, obj.Cycle);
        }

        public static IEnumerable<object[]> Depths => Enumerable.Range(1, 10).Select(i => new[] { (object)i });

        [Theory]
        [MemberData(nameof(Depths))]
        public void DeserializeCycleWithAnchorsWithDepth(int? depth)
        {
            var sut = new DeserializerBuilder()
                .WithTagMapping("!CycleTag", typeof(CycleTestEntity))
                .Build();

            StringBuilder builder = new StringBuilder(@"&o0 !CycleTag");
            builder.AppendLine();
            string indentation;
            for (int i = 0; i < depth - 1; ++i)
            {
                indentation = string.Concat(Enumerable.Repeat("  ", i));
                builder.AppendLine($"{indentation}Cycle: !CycleTag");
            }
            indentation = string.Concat(Enumerable.Repeat("  ", depth.Value - 1));
            builder.AppendLine($"{indentation}Cycle: *o0");
            var yaml = Yaml.Text(builder.ToString());
            var obj = sut.Deserialize<CycleTestEntity>(yaml);
            CycleTestEntity iterator = obj;
            for (int i = 0; i < depth; ++i)
            {
                iterator = iterator.Cycle;
            }
            Assert.Same(obj, iterator);
        }

        [Fact]
        public void RoundtripWindowsNewlines()
        {
            var text = $"Line1{Environment.NewLine}Line2{Environment.NewLine}Line3{Environment.NewLine}{Environment.NewLine}Line4";

            var sut = new SerializerBuilder().Build();
            var dut = new DeserializerBuilder().Build();

            using var writer = new StringWriter { NewLine = Environment.NewLine };
            sut.Serialize(writer, new StringContainer { Text = text });
            var serialized = writer.ToString();

            using var reader = new StringReader(serialized);
            var roundtrippedText = dut.Deserialize<StringContainer>(reader).Text.NormalizeNewLines();
            Assert.Equal(text, roundtrippedText);
        }

        [Theory]
        [InlineData("NULL")]
        [InlineData("Null")]
        [InlineData("null")]
        [InlineData("~")]
        [InlineData("true")]
        [InlineData("false")]
        [InlineData("True")]
        [InlineData("False")]
        [InlineData("TRUE")]
        [InlineData("FALSE")]
        [InlineData("0o77")]
        [InlineData("0x7A")]
        [InlineData("+1e10")]
        [InlineData("1E10")]
        [InlineData("+.inf")]
        [InlineData("-.inf")]
        [InlineData(".inf")]
        [InlineData(".nan")]
        [InlineData(".NaN")]
        [InlineData(".NAN")]
        public void StringsThatMatchKeywordsAreQuoted(string input)
        {
            var serializer = new SerializerBuilder().WithQuotingNecessaryStrings().Build();
            var o = new { text = input };
            var yaml = serializer.Serialize(o);
            Assert.Equal($"text: \"{input}\"{Environment.NewLine}", yaml);
        }

        public static IEnumerable<object[]> Yaml1_1SpecialStringsData = new[]
        {
            "-.inf", "-.Inf", "-.INF", "-0", "-0100_200", "-0b101", "-0x30", "-190:20:30", "-23", "-3.14",
            "._", "._14", ".", ".0", ".1_4", ".14", ".3E-1", ".3e+3", ".inf", ".Inf",
            ".INF", ".nan", ".NaN", ".NAN", "+.inf", "+.Inf", "+.INF", "+0.3e+3", "+0",
            "+0100_200", "+0b100", "+190:20:30", "+23", "+3.14", "~", "0.0", "0", "00", "001.23",
            "0011", "010", "02_0", "07", "0b0", "0b100_101", "0o0", "0o10", "0o7", "0x0",
            "0x10", "0x2_0", "0x42", "0xa", "100_000", "190:20:30.15", "190:20:30", "23", "3.", "3.14", "3.3e+3",
            "85_230.15", "85.230_15e+03", "false", "False", "FALSE", "n", "N", "no", "No", "NO",
            "null", "Null", "NULL", "off", "Off", "OFF", "on", "On", "ON", "true", "True", "TRUE",
            "y", "Y", "yes", "Yes", "YES"
        }.Select(v => new object[] { v }).ToList();

        [Theory]
        [MemberData(nameof(Yaml1_1SpecialStringsData))]
        public void StringsThatMatchYaml1_1KeywordsAreQuoted(string input)
        {
            var serializer = new SerializerBuilder().WithQuotingNecessaryStrings(true).Build();
            var o = new { text = input };
            var yaml = serializer.Serialize(o);
            Assert.Equal($"text: \"{input}\"{Environment.NewLine}", yaml);
        }

        [Fact]
        public void KeysOnConcreteClassDontGetQuoted_TypeStringGetsQuoted()
        {
            var serializer = new SerializerBuilder().WithQuotingNecessaryStrings().Build();
            var deserializer = new DeserializerBuilder().WithAttemptingUnquotedStringTypeDeserialization().Build();
            var yaml = @"
True: null
False: hello
Null: true
";
            var obj = deserializer.Deserialize<ReservedWordsTestClass<string>>(yaml);
            var result = serializer.Serialize(obj);
            obj.True.Should().BeNull();
            obj.False.Should().Be("hello");
            obj.Null.Should().Be("true");
            result.Should().Be($"True: {Environment.NewLine}False: hello{Environment.NewLine}Null: \"true\"{Environment.NewLine}");
        }

        [Fact]
        public void KeysOnConcreteClassDontGetQuoted_TypeBoolDoesNotGetQuoted()
        {
            var serializer = new SerializerBuilder().WithQuotingNecessaryStrings().Build();
            var deserializer = new DeserializerBuilder().WithAttemptingUnquotedStringTypeDeserialization().Build();
            var yaml = @"
True: null
False: hello
Null: true
";
            var obj = deserializer.Deserialize<ReservedWordsTestClass<bool>>(yaml);
            var result = serializer.Serialize(obj);
            obj.True.Should().BeNull();
            obj.False.Should().Be("hello");
            obj.Null.Should().BeTrue();
            result.Should().Be($"True: {Environment.NewLine}False: hello{Environment.NewLine}Null: true{Environment.NewLine}");
        }

        [Fact]
        public void SerializeStateMethodsGetCalledOnce()
        {
            var serializer = new SerializerBuilder().Build();
            var test = new TestState();
            serializer.Serialize(test);

            Assert.Equal(1, test.OnSerializedCallCount);
            Assert.Equal(1, test.OnSerializingCallCount);
        }

        [Fact]
        public void SerializeEnumAsNumber()
        {
            var serializer = new SerializerBuilder().WithYamlFormatter(new YamlFormatter
            {
                FormatEnum = (o, namingConvention) => ((int)o).ToString(),
                PotentiallyQuoteEnums = (_) => false
            }).Build();
            var deserializer = DeserializerBuilder.Build();

            var value = serializer.Serialize(TestEnumAsNumber.Test1);
            Assert.Equal("1", value.TrimNewLines());
            var v = deserializer.Deserialize<TestEnumAsNumber>(value);
            Assert.Equal(TestEnumAsNumber.Test1, v);

            value = serializer.Serialize(TestEnumAsNumber.Test1 | TestEnumAsNumber.Test2);
            Assert.Equal("3", value.TrimNewLines());
            v = deserializer.Deserialize<TestEnumAsNumber>(value);
            Assert.Equal(TestEnumAsNumber.Test1 | TestEnumAsNumber.Test2, v);
        }

        [Fact]
        public void TabsGetQuotedWhenQuoteNecessaryStringsIsOn()
        {
            var serializer = new SerializerBuilder()
                .WithQuotingNecessaryStrings()
                .Build();

            var s = "\t, something";
            var yaml = serializer.Serialize(s);
            var deserializer = new DeserializerBuilder().Build();
            var value = deserializer.Deserialize(yaml);
            Assert.Equal(s, value);
        }

        [Fact]
        public void SpacesGetQuotedWhenQuoteNecessaryStringsIsOn()
        {
            var serializer = new SerializerBuilder()
                .WithQuotingNecessaryStrings()
                .Build();

            var s = " , something";
            var yaml = serializer.Serialize(s);
            var deserializer = new DeserializerBuilder().Build();
            var value = deserializer.Deserialize(yaml);
            Assert.Equal(s, value);
        }

        [Flags]
        private enum TestEnumAsNumber
        {
            Test1 = 1,
            Test2 = 2
        }

        [Fact]
        public void NamingConventionAppliedToEnum()
        {
            var serializer = new SerializerBuilder().WithEnumNamingConvention(CamelCaseNamingConvention.Instance).Build();
            ScalarStyle style = ScalarStyle.Plain;
            var serialized = serializer.Serialize(style);
            Assert.Equal("plain", serialized.RemoveNewLines());
        }

        [Fact]
        public void NamingConventionAppliedToEnumWhenDeserializing()
        {
            var serializer = new DeserializerBuilder().WithEnumNamingConvention(UnderscoredNamingConvention.Instance).Build();
            var yaml = "Double_Quoted";
            ScalarStyle expected = ScalarStyle.DoubleQuoted;
            var actual = serializer.Deserialize<ScalarStyle>(yaml);
            Assert.Equal(expected, actual);
        }

        public class TestState
        {
            public int OnSerializedCallCount { get; set; }
            public int OnSerializingCallCount { get; set; }

            public string Test { get; set; } = string.Empty;

            [OnSerialized]
            public void Serialized() => OnSerializedCallCount++;

            [OnSerializing]
            public void Serializing() => OnSerializingCallCount++;
        }

        public class ReservedWordsTestClass<TNullType>
        {
            public string True { get; set; }
            public string False { get; set; }
            public TNullType Null { get; set; }
        }

        [TypeConverter(typeof(DoublyConvertedTypeConverter))]
        public class DoublyConverted
        {
            public string Value { get; set; }
        }

        public class DoublyConvertedTypeConverter : TypeConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return destinationType == typeof(int);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                return ((DoublyConverted)value).Value.Length;
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                return new DoublyConverted { Value = (string)value };
            }
        }

        public class NamingConventionDisabled
        {
            [YamlMember(ApplyNamingConventions = false)]
            public string NoConvention { get; set; }
        }

        public class NonSerializableContainer
        {
            public NonSerializable Value { get; set; }
        }

        public class NonSerializable
        {
            public string WillThrow { get { throw new Exception(); } }

            public string Text { get; set; }
        }

        public class StringContainer
        {
            public string Text { get; set; }
        }

        public class NonSerializableTypeConverter : IYamlTypeConverter
        {
            public bool Accepts(Type type)
            {
                return typeof(NonSerializable).IsAssignableFrom(type);
            }

            public object ReadYaml(IParser parser, Type type)
            {
                var scalar = parser.Consume<Scalar>();
                return new NonSerializable { Text = scalar.Value };
            }

            public void WriteYaml(IEmitter emitter, object value, Type type)
            {
                emitter.Emit(new Scalar(((NonSerializable)value).Text));
            }
        }
    }
}
