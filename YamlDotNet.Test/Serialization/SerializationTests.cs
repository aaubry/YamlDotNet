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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using FakeItEasy;
using FluentAssertions;
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

			Action action = () => RoundtripSerializer.Serialize(new StringWriter(), obj, typeof(CircularReference));

			action.ShouldNotThrow();
		}

		[Fact]
		public void DeserializeCustomTags()
		{
			var stream = Yaml.StreamFrom("tags.yaml");

			Deserializer.RegisterTagMapping("tag:yaml.org,2002:point", typeof(Point));
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

			var result = DoRoundtripFromObjectTo<Example>(obj, RoundtripSerializer);

			result.ShouldBeEquivalentTo(obj);
		}

		[Fact]
		public void RoundtripObjectWithDefaults()
		{
			var obj = new Example();

			var result = DoRoundtripFromObjectTo<Example>(obj, RoundtripEmitDefaultsSerializer);

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

			RoundtripSerializer.RegisterTypeConverter(new MissingDefaultCtorConverter());
			Deserializer.RegisterTypeConverter(new MissingDefaultCtorConverter());
			var result = DoRoundtripFromObjectTo<MissingDefaultCtor>(obj, RoundtripSerializer, Deserializer);

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
		// Todo: is the assert on the string necessary?
		public void RoundtripDerivedClass()
		{
			var obj = new InheritanceExample
			{
				SomeScalar = "Hello",
				RegularBase = new Derived { BaseProperty = "foo", DerivedProperty = "bar" },
			};

			var result = DoRoundtripFromObjectTo<InheritanceExample>(obj, RoundtripSerializer);

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
				BaseWithSerializeAs = new Derived { BaseProperty = "foo", DerivedProperty = "bar" },
			};

			var result = DoRoundtripFromObjectTo<InheritanceExample>(obj, RoundtripSerializer);

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
			TextReader stream = Yaml.StreamFrom("ordered-properties.yaml");

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

			var result = DoRoundtripOn<List<int>>(obj, RoundtripSerializer);

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
				{ "key3", "value3" },
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
			var reader = EventReaderFor(Lines(
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
			var reader = EventReaderFor(Lines(
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
			Dump.WriteLine(writer.ToString());
			Regex.IsMatch(serialized, "^" + guid.ToString("D")).Should().BeTrue("serialized content should contain the guid");
		}

		[Fact]
		public void SerializationOfNullInListsAreAlwaysEmittedWithoutUsingEmitDefaults()
		{
			var writer = new StringWriter();
			var obj = new[] { "foo", null, "bar" };

			Serializer.Serialize(writer, obj);
			var serialized = writer.ToString();
			Dump.WriteLine(serialized);

			Regex.Matches(serialized, "-").Count.Should().Be(3, "there should have been 3 elements");
		}

		[Fact]
		public void SerializationOfNullInListsAreAlwaysEmittedWhenUsingEmitDefaults()
		{
			var writer = new StringWriter();
			var obj = new[] { "foo", null, "bar" };

			EmitDefaultsSerializer.Serialize(writer, obj);
			var serialized = writer.ToString();
			Dump.WriteLine(serialized);

			Regex.Matches(serialized, "-").Count.Should().Be(3, "there should have been 3 elements");
		}

		[Fact]
		public void SerializationIncludesKeyWhenEmittingDefaults()
		{
			var writer = new StringWriter();
			var obj = new Example { MyString = null };

			EmitDefaultsSerializer.Serialize(writer, obj, typeof(Example));
			Dump.WriteLine(writer);

			writer.ToString().Should().Contain("MyString");
		}

		[Fact]
		[Trait("Motive", "Bug fix")]
		public void SerializationIncludesKeyFromAnonymousTypeWhenEmittingDefaults()
		{
			var writer = new StringWriter();
			var obj = new { MyString = (string)null };

			EmitDefaultsSerializer.Serialize(writer, obj, obj.GetType());
			Dump.WriteLine(writer);

			writer.ToString().Should().Contain("MyString");
		}

		[Fact]
		public void SerializationDoesNotIncludeKeyWhenDisregardingDefaults()
		{
			var writer = new StringWriter();
			var obj = new Example { MyString = null };

			Serializer.Serialize(writer, obj, typeof(Example));
			Dump.WriteLine(writer);

			writer.ToString().Should().NotContain("MyString");
		}

		[Fact]
		public void SerializationOfDefaultsWorkInJson()
		{
			var writer = new StringWriter();
			var obj = new Example { MyString = null };

			EmitDefaultsJsonCompatibleSerializer.Serialize(writer, obj, typeof(Example));
			Dump.WriteLine(writer);

			writer.ToString().Should().Contain("MyString");
		}

		[Fact]
		// Todo: this is actualy roundtrip
		public void DeserializationOfDefaultsWorkInJson()
		{
			var writer = new StringWriter();
			var obj = new Example { MyString = null };

			RoundtripEmitDefaultsJsonCompatibleSerializer.Serialize(writer, obj, typeof(Example));
			Dump.WriteLine(writer);
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
			Dump.WriteLine(serialized);

			serialized.Should()
				.Be("Order1: Order1 value\r\nOrder2: Order2 value\r\n", "the properties should be in the right order");
		}

		[Fact]
		public void SerializationRespectsYamlIgnoreAttribute()
		{

			var writer = new StringWriter();
			var obj = new IgnoreExample();

			Serializer.Serialize(writer, obj);
			var serialized = writer.ToString();
			Dump.WriteLine(serialized);

			serialized.Should().NotContain("IgnoreMe");
		}

		[Fact]
		public void SerializationRespectsScalarStyle()
		{
			var writer = new StringWriter();
			var obj = new ScalarStyleExample();

			Serializer.Serialize(writer, obj);
			var serialized = writer.ToString();
			Dump.WriteLine(serialized);

			serialized.Should()
				.Be("LiteralString: |-\r\n  Test\r\nDoubleQuotedString: \"Test\"\r\n", "the properties should be specifically styled");
		}

		[Fact]
		public void SerializationSkipsPropertyWhenUsingDefaultValueAttribute()
		{
			var writer = new StringWriter();
			var obj = new DefaultsExample { Value = DefaultsExample.DefaultValue };

			Serializer.Serialize(writer, obj);
			var serialized = writer.ToString();
			Dump.WriteLine(serialized);

			serialized.Should().NotContain("Value");
		}

		[Fact]
		public void SerializationEmitsPropertyWhenUsingEmitDefaultsAndDefaultValueAttribute()
		{
			var writer = new StringWriter();
			var obj = new DefaultsExample { Value = DefaultsExample.DefaultValue };

			EmitDefaultsSerializer.Serialize(writer, obj);
			var serialized = writer.ToString();
			Dump.WriteLine(serialized);

			serialized.Should().Contain("Value");
		}

		[Fact]
		public void SerializationEmitsPropertyWhenValueDifferFromDefaultValueAttribute()
		{
			var writer = new StringWriter();
			var obj = new DefaultsExample { Value = "non-default" };

			Serializer.Serialize(writer, obj);
			var serialized = writer.ToString();
			Dump.WriteLine(serialized);

			serialized.Should().Contain("Value");
		}

		[Fact]
		public void SerializingAGenericDictionaryShouldNotThrowTargetException()
		{
			var obj = new CustomGenericDictionary {
				{ "hello", "world" },
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

			var serializer = new Serializer(namingConvention: convention);
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

			var deserializer = new Deserializer(namingConvention: convention);
			deserializer.Deserialize<NameConvention>(UsingReaderFor(text));

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
			var result = Deserializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(new EventReader(parser));

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
			var result = Deserializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(new EventReader(parser));

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

			var result = Deserializer.Deserialize<Dictionary<string, List<Dictionary<string, string>>>>(new EventReader(parser));

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
			var des = new Deserializer(ignoreUnmatched: true);
			var actual = des.Deserialize<Simple>(UsingReaderFor(text));
			actual.aaa.Should().Be("hello");
		}

		[Fact]
		public void DontIgnoreExtraPropertiesIfWanted()
		{
			var text = Lines("aaa: hello", "bbb: world");
			var des = new Deserializer(ignoreUnmatched: false);
			var actual = Record.Exception(() => des.Deserialize<Simple>(UsingReaderFor(text)));
			Assert.IsType<YamlException>(actual);
		}

		[Fact]
		public void IgnoreExtraPropertiesIfWantedBefore()
		{
			var text = Lines("bbb: [200,100]", "aaa: hello");
			var des = new Deserializer(ignoreUnmatched: true);
			var actual = des.Deserialize<Simple>(UsingReaderFor(text));
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

			var des = new Deserializer(namingConvention: new CamelCaseNamingConvention(), ignoreUnmatched: true);
			var actual = des.Deserialize<SimpleScratch>(UsingReaderFor(text));
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

            var serializer = new Serializer(namingConvention: mockNamingConvention);
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

            var serializer = new Serializer(namingConvention: mockNamingConvention);
            var writer = new StringWriter();
            serializer.Serialize(writer, obj);

            writer.ToString().Should().Contain("new_key_here: new_value");
        }
        #region Test Dictionary that implements IDictionary<,>, but not IDictionary
        public class GenericTestDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        {
            private readonly Dictionary<TKey, TValue> _dictionary;
            public GenericTestDictionary()
            {
                _dictionary = new Dictionary<TKey, TValue>();
            }
            public void Add(TKey key, TValue value)
            {
                _dictionary.Add(key, value);
            }

            public bool ContainsKey(TKey key)
            {
                return _dictionary.ContainsKey(key);
            }

            public ICollection<TKey> Keys
            {
                get { return _dictionary.Keys; }
            }

            public bool Remove(TKey key)
            {
                return _dictionary.Remove(key);
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                return _dictionary.TryGetValue(key, out value);
            }

            public ICollection<TValue> Values
            {
                get { return _dictionary.Values; }
            }

            public TValue this[TKey key]
            {
                get { return _dictionary[key]; }
                set { _dictionary[key] = value; }
            }

            public void Add(KeyValuePair<TKey, TValue> item)
            {
                ((IDictionary<TKey, TValue>)_dictionary).Add(item);
            }

            public void Clear()
            {
                _dictionary.Clear();
            }

            public bool Contains(KeyValuePair<TKey, TValue> item)
            {
                return ((IDictionary<TKey, TValue>)_dictionary).Contains(item);
            }

            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                ((IDictionary<TKey, TValue>)_dictionary).CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return _dictionary.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(KeyValuePair<TKey, TValue> item)
            {
                return ((IDictionary<TKey, TValue>)_dictionary).Remove(item);
            }

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                return _dictionary.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _dictionary.GetEnumerator();
            }
        }
        #endregion

    }
}
