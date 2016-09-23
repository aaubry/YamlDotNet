//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) Antoine Aubry and contributors

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using FakeItEasy;
using FluentAssertions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.ObjectFactories;

namespace YamlDotNet.Test.Serialization
{
    public class SerializationTests : SerializationTestHelper
    {
        #region Test Cases

        private static readonly string[] TrueStrings = { "true", "y", "yes", "on" };
        private static readonly string[] FalseStrings = { "false", "n", "no", "off" };

        public static IEnumerable<Object[]> DeserializeScalarBoolean_TestCases
        {
            get
            {
                foreach (var trueString in SerializationTests.TrueStrings)
                {
                    yield return new Object[] { trueString, true };
                    yield return new Object[] { trueString.ToUpper(), true };
                }

                foreach (var falseString in SerializationTests.FalseStrings)
                {
                    yield return new Object[] { falseString, false };
                    yield return new Object[] { falseString.ToUpper(), false };
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
            var stream = Yaml.StreamFrom("02-scalar-in-imp-doc.yaml");

            var result = Deserializer.Deserialize(stream);

            result.Should().Be("a scalar");
        }

        [Theory]
        [MemberData("DeserializeScalarBoolean_TestCases")]
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

        [Fact]
        public void RoundtripEnums()
        {
            var flags = EnumExample.One | EnumExample.Two;

            var result = DoRoundtripFromObjectTo<EnumExample>(flags);

            result.Should().Be(flags);
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
        public void DeserializeCustomTags()
        {
            var stream = Yaml.StreamFrom("tags.yaml");

            DeserializerBuilder.WithTagMapping("tag:yaml.org,2002:point", typeof(Point));
            var result = Deserializer.Deserialize(stream);

            result.Should().BeOfType<Point>().And
                .Subject.As<Point>()
                .ShouldBeEquivalentTo(new { X = 10, Y = 20 }, o => o.ExcludingMissingMembers());
        }

        [Fact]
        public void DeserializeExplicitType()
        {
            var text = Yaml.StreamFrom("explicit-type.template").TemplatedOn<Simple>();

            var result = Deserializer.Deserialize<Simple>(UsingReaderFor(text));

            result.aaa.Should().Be("bbb");
        }

        [Fact]
        public void DeserializeConvertible()
        {
            var text = Yaml.StreamFrom("convertible.template").TemplatedOn<Convertible>();

            var result = Deserializer.Deserialize<Simple>(UsingReaderFor(text));

            result.aaa.Should().Be("[hello, world]");
        }

        [Fact]
        public void DeserializationOfObjectsHandlesForwardReferences()
        {
            var text = Lines(
                "Nothing: *forward",
                "MyString: &forward ForwardReference");

            var result = Deserializer.Deserialize<Example>(UsingReaderFor(text));

            result.ShouldBeEquivalentTo(
                new { Nothing = "ForwardReference", MyString = "ForwardReference" }, o => o.ExcludingMissingMembers());
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

            var result = DoRoundtripFromObjectTo<Example>(obj, SerializerBuilder.EnsureRoundtrip().Build());

            result.ShouldBeEquivalentTo(obj);
        }

        [Fact]
        public void RoundtripObjectWithDefaults()
        {
            var obj = new Example();

            var result = DoRoundtripFromObjectTo<Example>(obj, SerializerBuilder.EnsureRoundtrip().EmitDefaults().Build());

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

            var attribute = new YamlMemberAttribute();
            attribute.Alias = "fourthOverride";

            var serializer = new SerializerBuilder()
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

            var result = DoRoundtripFromObjectTo<InheritanceExample>(obj, SerializerBuilder.EnsureRoundtrip().Build());

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

            var result = DoRoundtripFromObjectTo<InheritanceExample>(obj, SerializerBuilder.EnsureRoundtrip().Build());

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
            var stream = Yaml.StreamFrom("guid.yaml");
            var result = Deserializer.Deserialize<Guid>(stream);

            result.Should().Be(new Guid("9462790d5c44468985425e2dd38ebd98"));
        }

        [Fact]
        public void DeserializationOfOrderedProperties()
        {
            var stream = Yaml.StreamFrom("ordered-properties.yaml");

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
            var stream = Yaml.StreamFrom("list.yaml");

            var result = Deserializer.Deserialize<String[]>(stream);

            result.Should().Equal(new[] { "one", "two", "three" });
        }

        [Fact]
        public void DeserializeList()
        {
            var stream = Yaml.StreamFrom("list.yaml");

            var result = Deserializer.Deserialize(stream);

            result.Should().BeAssignableTo<IList>().And
                .Subject.As<IList>().Should().Equal(new[] { "one", "two", "three" });
        }

        [Fact]
        public void DeserializeExplicitList()
        {
            var stream = Yaml.StreamFrom("list-explicit.yaml");

            var result = Deserializer.Deserialize(stream);

            result.Should().BeAssignableTo<IList<int>>().And
                .Subject.As<IList<int>>().Should().Equal(3, 4, 5);
        }

        [Fact]
        public void DeserializationOfGenericListsHandlesForwardReferences()
        {
            var text = Lines(
                "- *forward",
                "- &forward ForwardReference");

            var result = Deserializer.Deserialize<string[]>(UsingReaderFor(text));

            result.Should().Equal(new[] { "ForwardReference", "ForwardReference" });
        }

        [Fact]
        public void DeserializationOfNonGenericListsHandlesForwardReferences()
        {
            var text = Lines(
                "- *forward",
                "- &forward ForwardReference");

            var result = Deserializer.Deserialize<ArrayList>(UsingReaderFor(text));

            result.Should().Equal(new[] { "ForwardReference", "ForwardReference" });
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
            var stream = Yaml.StreamFrom("dictionary.yaml");

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
            var stream = Yaml.StreamFrom("dictionary-explicit.yaml");

            var result = Deserializer.Deserialize(stream);

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
        public void DeserializationOfGenericDictionariesHandlesForwardReferences()
        {
            var text = Lines(
                "key1: *forward",
                "*forwardKey: ForwardKeyValue",
                "*forward: *forward",
                "key2: &forward ForwardReference",
                "key3: &forwardKey key4");

            var result = Deserializer.Deserialize<Dictionary<string, string>>(UsingReaderFor(text));

            result.Should().Equal(new Dictionary<string, string> {
                { "ForwardReference", "ForwardReference" },
                { "key1", "ForwardReference" },
                { "key2", "ForwardReference" },
                { "key4", "ForwardKeyValue" },
                { "key3", "key4" }
            });
        }

        [Fact]
        public void DeserializationOfNonGenericDictionariesHandlesForwardReferences()
        {
            var text = Lines(
                "key1: *forward",
                "*forwardKey: ForwardKeyValue",
                "*forward: *forward",
                "key2: &forward ForwardReference",
                "key3: &forwardKey key4");

            var result = Deserializer.Deserialize<Hashtable>(UsingReaderFor(text));

            result.Should().BeEquivalentTo(
                Entry("ForwardReference", "ForwardReference"),
                Entry("key1", "ForwardReference"),
                Entry("key2", "ForwardReference"),
                Entry("key4", "ForwardKeyValue"),
                Entry("key3", "key4"));
        }

        [Fact]
        public void DeserializeListOfDictionaries()
        {
            var stream = Yaml.StreamFrom("list-of-dictionaries.yaml");

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

            reader.Expect<StreamStart>();
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

            reader.Expect<StreamStart>();
            var one = Deserializer.Deserialize<Simple>(reader);
            var two = Deserializer.Deserialize<Simple>(reader);
            var three = Deserializer.Deserialize<Simple>(reader);

            reader.Accept<StreamEnd>().Should().BeTrue("reader should have reached StreamEnd");
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

            SerializerBuilder.EmitDefaults().Build().Serialize(writer, obj);
            var serialized = writer.ToString();

            Regex.Matches(serialized, "-").Count.Should().Be(3, "there should have been 3 elements");
        }

        [Fact]
        public void SerializationIncludesKeyWhenEmittingDefaults()
        {
            var writer = new StringWriter();
            var obj = new Example { MyString = null };

            SerializerBuilder.EmitDefaults().Build().Serialize(writer, obj, typeof(Example));

            writer.ToString().Should().Contain("MyString");
        }

        [Fact]
        [Trait("Motive", "Bug fix")]
        public void SerializationIncludesKeyFromAnonymousTypeWhenEmittingDefaults()
        {
            var writer = new StringWriter();
            var obj = new { MyString = (string)null };

            SerializerBuilder.EmitDefaults().Build().Serialize(writer, obj, obj.GetType());

            writer.ToString().Should().Contain("MyString");
        }

        [Fact]
        public void SerializationDoesNotIncludeKeyWhenDisregardingDefaults()
        {
            var writer = new StringWriter();
            var obj = new Example { MyString = null };

            Serializer.Serialize(writer, obj, typeof(Example));

            writer.ToString().Should().NotContain("MyString");
        }

        [Fact]
        public void SerializationOfDefaultsWorkInJson()
        {
            var writer = new StringWriter();
            var obj = new Example { MyString = null };

            SerializerBuilder.EmitDefaults().JsonCompatible().Build().Serialize(writer, obj, typeof(Example));

            writer.ToString().Should().Contain("MyString");
        }

        [Fact]
        // Todo: this is actualy roundtrip
        public void DeserializationOfDefaultsWorkInJson()
        {
            var writer = new StringWriter();
            var obj = new Example { MyString = null };

            SerializerBuilder.EnsureRoundtrip().EmitDefaults().JsonCompatible().Build().Serialize(writer, obj, typeof(Example));
            var result = Deserializer.Deserialize<Example>(UsingReaderFor(writer));

            result.MyString.Should().BeNull();
        }

        [Fact]
        public void SerializationOfOrderedProperties()
        {
            var obj = new OrderExample();
            var writer = new StringWriter();

            Serializer.Serialize(writer, obj);
            var serialized = writer.ToString();

            serialized.Should()
                .Be("Order1: Order1 value\r\nOrder2: Order2 value\r\n".Replace("\r\n", Environment.NewLine), "the properties should be in the right order");
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
                .Be("LiteralString: |-\r\n  Test\r\nDoubleQuotedString: \"Test\"\r\n".Replace("\r\n", Environment.NewLine), "the properties should be specifically styled");
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
                .Be("LiteralString: \"Test\"\r\nDoubleQuotedString: |-\r\n  Test\r\n".Replace("\r\n", Environment.NewLine), "the properties should be specifically styled");
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
                .Be("BaseProperty: Base\r\n".Replace("\r\n", Environment.NewLine), "the derived property should be specifically ignored");
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
                .Be("DerivedProperty: Derived\r\n".Replace("\r\n", Environment.NewLine), "the base property should be specifically ignored");
        }

        [Fact]
        public void SerializationSkipsPropertyWhenUsingDefaultValueAttribute()
        {
            var writer = new StringWriter();
            var obj = new DefaultsExample { Value = DefaultsExample.DefaultValue };

            Serializer.Serialize(writer, obj);
            var serialized = writer.ToString();

            serialized.Should().NotContain("Value");
        }

        [Fact]
        public void SerializationEmitsPropertyWhenUsingEmitDefaultsAndDefaultValueAttribute()
        {
            var writer = new StringWriter();
            var obj = new DefaultsExample { Value = DefaultsExample.DefaultValue };

            SerializerBuilder.EmitDefaults().Build().Serialize(writer, obj);
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
        public void SerializingAGenericDictionaryShouldNotThrowTargetException()
        {
            var obj = new CustomGenericDictionary {
                { "hello", "world" }
            };

            Action action = () => Serializer.Serialize(new StringWriter(), obj);

            action.ShouldNotThrow<TargetException>();
        }

        [Fact]
        public void SerializaionUtilizeNamingConventions()
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

            DeserializerBuilder.WithNamingConvention(convention);
            Deserializer.Deserialize<NameConvention>(UsingReaderFor(text));

            A.CallTo(() => convention.Apply("FirstTest")).MustHaveHappened();
            A.CallTo(() => convention.Apply("SecondTest")).MustHaveHappened();
        }

        [Fact]
        public void TypeConverterIsUsedOnListItems()
        {
            var text = Lines(
                "- !<!{type}>",
                "  Left: hello",
                "  Right: world")
                .TemplatedOn<Convertible>();

            var list = Deserializer.Deserialize<List<string>>(UsingReaderFor(text));

            list
                .Should().NotBeNull()
                .And.ContainSingle(c => c.Equals("[hello, world]"));
        }

        [Fact]
        public void BackreferencesAreMergedWithMappings()
        {
            var stream = Yaml.StreamFrom("backreference.yaml");

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
                    #<< : [ *BIG, *LEFT, *SMALL ]    # This does not work because, in the current implementation,
                                                     # later keys override former keys. This could be fixed, but that
                                                     # is not trivial because the deserializer allows aliases to refer to
                                                     # an anchor that is defined later in the document, and the way it is
                                                     # implemented, the value is assigned later when the anchored value is
                                                     # deserialized.
                    << : [ *SMALL, *LEFT, *BIG ]
                    x: 1
                    label: center/big
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
                .WithNamingConvention(new CamelCaseNamingConvention())
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
            var obj = new Dictionary<string, object>();
            obj["property_one"] = new GenericTestDictionary<string, object>();
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

        [Theory, MemberData("SpecialFloats")]
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
                        new FloatTestCase("double.Epsilon", double.Epsilon, "4.9406564584124654E-324"),
                        new FloatTestCase("double.MinValue", double.MinValue, "-1.7976931348623157E+308"),
                        new FloatTestCase("double.MaxValue", double.MaxValue, "1.7976931348623157E+308"),

                        new FloatTestCase("float.NaN", float.NaN, ".nan"),
                        new FloatTestCase("float.PositiveInfinity", float.PositiveInfinity, ".inf"),
                        new FloatTestCase("float.NegativeInfinity", float.NegativeInfinity, "-.inf"),
                        new FloatTestCase("float.Epsilon", float.Epsilon, "1.40129846E-45"),
                        new FloatTestCase("float.MinValue", float.MinValue, "-3.40282347E+38"),
                        new FloatTestCase("float.MaxValue", float.MaxValue, "3.40282347E+38")
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
                .EmitDefaults()
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

#pragma warning disable 0618  // 'Deserializer.Deserializer(IObjectFactory, INamingConvention, bool, YamlAttributeOverrides)' is obsolete: 'Please use DeserializerBuilder to customize the Deserializer. This constructor will be removed in future releases.'  YamlDotNet.Test C:\Users\aau02\work\YamlDotNet\YamlDotNet.Test\Serialization\SerializationTests.cs  1268    Active
        [Fact]
        public void AttributeOverridesAndNamingConventionDoNotConflictInLegacyConfiguration()
        {
            var overrides = new YamlAttributeOverrides();
            overrides.Add<Foo>(f => f.IsRequired, new YamlMemberAttribute
            {
                Alias = "Required"
            });

            var namingConvention = new CamelCaseNamingConvention();

            var serializer = new Serializer(namingConvention: namingConvention, overrides: overrides);
            var yaml = serializer.Serialize(new Foo { IsRequired = true });
            Assert.Contains("required: true", yaml);

            var deserializer = new Deserializer(namingConvention: namingConvention, overrides: overrides);
            var deserializedFoo = deserializer.Deserialize<Foo>(yaml);
            Assert.True(deserializedFoo.IsRequired);
        }
#pragma warning restore 0618  // 'Deserializer.Deserializer(IObjectFactory, INamingConvention, bool, YamlAttributeOverrides)' is obsolete: 'Please use DeserializerBuilder to customize the Deserializer. This constructor will be removed in future releases.'  YamlDotNet.Test C:\Users\aau02\work\YamlDotNet\YamlDotNet.Test\Serialization\SerializationTests.cs  1268    Active

        [Fact]
        public void AttributeOverridesAndNamingConventionDoNotConflict()
        {
            var namingConvention = new CamelCaseNamingConvention();

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
                var comment = parser.Allow<Comment>();
                if (comment != null)
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

            ulong parsed = new Deserializer().Deserialize<ulong>(yaml);
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

            long parsed = new Deserializer().Deserialize<long>(yaml);
            Assert.Equal(value, parsed);
        }
    }
}
