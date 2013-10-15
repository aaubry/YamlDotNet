//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2008, 2009, 2010, 2011, 2012, 2013 Antoine Aubry

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
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Core.Test;
using YamlDotNet.RepresentationModel.Serialization;
using YamlDotNet.RepresentationModel.Serialization.NamingConventions;

// ReSharper disable InconsistentNaming
namespace YamlDotNet.RepresentationModel.Test
{
	public class SerializationTests : YamlTest
	{
		[Fact]
		public void Roundtrip()
		{
			ShouldRountripWithOptions(SerializationOptions.Roundtrip);
		}

		[Fact]
		public void RoundtripWithDefaults()
		{
			ShouldRountripWithOptions(SerializationOptions.Roundtrip | SerializationOptions.EmitDefaults);
		}

		private void ShouldRountripWithOptions(SerializationOptions options)
		{
			var buffer = new StringWriter();
			var serializer = new Serializer(options);

			var original = new X();
			serializer.Serialize(buffer, original);

			Dump.WriteLine(buffer);

			var deserializer = new Deserializer();
			var copy = deserializer.Deserialize<X>(new StringReader(buffer.ToString()));

			copy.ShouldHave().AllProperties().EqualTo(original);
		}

		[Fact]
		public void CircularReference()
		{
			var serializer = new Serializer(SerializationOptions.Roundtrip);
			var buffer = new StringWriter();
			var original = new Y();
			original.Child1 = new Y {
				Child1 = original,
				Child2 = original
			};

			Action action = () => serializer.Serialize(buffer, original, typeof(Y));

			action.ShouldNotThrow();
		}

		public class Y
		{
			public Y Child1 { get; set; }
			public Y Child2 { get; set; }
		}

		[Fact]
		public void DeserializeScalar()
		{
			var deserializer = new Deserializer();

			var result = deserializer.Deserialize(YamlFile("test2.yaml"));

			result.Should().Be("a scalar");
		}

		[Fact]
		public void DeserializeExplicitType()
		{
			var deserializer = new Deserializer();

			var result = deserializer.Deserialize<Z>(YamlFile("explicitType.yaml"));

			result.aaa.Should().Be("bbb");
		}

		[Fact]
		public void DeserializeDictionary()
		{
			var deserializer = new Deserializer();

			var result = deserializer.Deserialize(YamlFile("dictionary.yaml"));

			result.Should().BeAssignableTo<IDictionary<object, object>>().And.Subject
				.As<IDictionary<object, object>>().Should().Equal(new Dictionary<object, object> {
					{ "key1", "value1" },
					{ "key2", "value2" }
				});
		}

		[Fact]
		public void DeserializeExplicitDictionary()
		{
			var deserializer = new Deserializer();

			var result = deserializer.Deserialize(YamlFile("dictionaryExplicit.yaml"));

			result.Should().BeAssignableTo<IDictionary<string, int>>().And.Subject
				.As<IDictionary<string, int>>().Should().Equal(new Dictionary<string, int> {
					{ "key1", 1 },
					{ "key2", 2 }
				});
		}

		[Fact]
		public void DeserializeListOfDictionaries()
		{
			var deserializer = new Deserializer();

			var result = deserializer.Deserialize<List<Dictionary<string, string>>>(YamlFile("listOfDictionaries.yaml"));

			result.ShouldBeEquivalentTo(new [] {
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
		public void DeserializeList()
		{
			var deserializer = new Deserializer();
			
			var result = deserializer.Deserialize(YamlFile("list.yaml"));

			result.Should().BeAssignableTo<IList>().And
				.Subject.As<IList>().Should().Equal(new[] { "one", "two", "three" });
		}

		[Fact]
		public void DeserializeExplicitList()
		{
			var deserializer = new Deserializer();

			var result = deserializer.Deserialize(YamlFile("listExplicit.yaml"));

			result.Should().BeAssignableTo<IList<int>>().And
				.Subject.As<IList<int>>().Should().Equal(3, 4, 5);
		}

		[Fact]
		public void DeserializeEnumerable()
		{
			var serializer = new Serializer();
			var deserializer = new Deserializer();
			var buffer = new StringWriter();
			var z = new[] { new Z { aaa = "Yo" }};

			serializer.Serialize(buffer, z);
			var result = deserializer.Deserialize<IEnumerable<Z>>(new StringReader(buffer.ToString()));

			result.Should().ContainSingle(item => "Yo".Equals(item.aaa));
		}

		[Fact]
		public void RoundtripList()
		{
			var serializer = new Serializer(SerializationOptions.Roundtrip);
			var deserializer = new Deserializer();
			var buffer = new StringWriter();
			var original = new List<int> { 2, 4, 6 };

			serializer.Serialize(buffer, original, typeof(List<int>));
			Dump.WriteLine(buffer);
			var copy = deserializer.Deserialize<List<int>>(new StringReader(buffer.ToString()));

			copy.Should().Equal(original);
		}

		[Fact]
		public void DeserializeArray()
		{
			var deserializer = new Deserializer();

			var result = deserializer.Deserialize<String[]>(YamlFile("list.yaml"));

			result.Should().Equal(new[] { "one", "two", "three" });
		}

		[Fact]
		public void Enums()
		{
			var serializer = new Serializer();
			var deserializer = new Deserializer();
			var buffer = new StringWriter();
			var flags = StringFormatFlags.NoClip | StringFormatFlags.NoFontFallback;

			serializer.Serialize(buffer, flags);
			var result = deserializer.Deserialize<StringFormatFlags>(new StringReader(buffer.ToString()));

			result.Should().Be(flags);
		}

		[Fact]
		public void CustomTags()
		{
			var deserializer = new Deserializer();

			deserializer.RegisterTagMapping("tag:yaml.org,2002:point", typeof(Point));
			var result = deserializer.Deserialize(YamlFile("tags.yaml"));

			result.Should().BeOfType<Point>().And
				.Subject.As<Point>().ShouldHave()
				.SharedProperties().EqualTo(new { X = 10, Y = 20 });
		}

		[Fact]
		public void DeserializeConvertible()
		{
			var deserializer = new Deserializer();

			var result = deserializer.Deserialize<Z>(YamlFile("convertible.yaml"));

			result.aaa.Should().Be("[hello, world]");
		}

		public class Converter : TypeConverter
		{
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			{
				return sourceType == typeof(string);
			}

			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			{
				return false;
			}

			public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
			{
				if (!(value is string))
					throw new InvalidOperationException();
				var parts = (value as string).Split(' ');
				return new Convertible
				{
					Left = parts[0],
					Right = parts[1]
				};
			}
		}

		[TypeConverter(typeof(Converter))]
		public class Convertible : IConvertible
		{
			public string Left { get; set; }
			public string Right { get; set; }

			public string ToString(IFormatProvider provider)
			{
				Assert.Equal(CultureInfo.InvariantCulture, provider);
				return string.Format(provider, "[{0}, {1}]", Left, Right);
			}

			public object ToType(Type conversionType, IFormatProvider provider)
			{
				Assert.Equal(typeof(string), conversionType);
				return ToString(provider);
			}

			#region Unused IConvertible Members

			public TypeCode GetTypeCode()
			{
				throw new NotSupportedException();
			}

			public bool ToBoolean(IFormatProvider provider)
			{
				throw new NotSupportedException();
			}

			public byte ToByte(IFormatProvider provider)
			{
				throw new NotSupportedException();
			}

			public char ToChar(IFormatProvider provider)
			{
				throw new NotSupportedException();
			}

			public DateTime ToDateTime(IFormatProvider provider)
			{
				throw new NotSupportedException();
			}

			public decimal ToDecimal(IFormatProvider provider)
			{
				throw new NotSupportedException();
			}

			public double ToDouble(IFormatProvider provider)
			{
				throw new NotSupportedException();
			}

			public short ToInt16(IFormatProvider provider)
			{
				throw new NotSupportedException();
			}

			public int ToInt32(IFormatProvider provider)
			{
				throw new NotSupportedException();
			}

			public long ToInt64(IFormatProvider provider)
			{
				throw new NotSupportedException();
			}

			public sbyte ToSByte(IFormatProvider provider)
			{
				throw new NotSupportedException();
			}

			public float ToSingle(IFormatProvider provider)
			{
				throw new NotSupportedException();
			}

			public ushort ToUInt16(IFormatProvider provider)
			{
				throw new NotSupportedException();
			}

			public uint ToUInt32(IFormatProvider provider)
			{
				throw new NotSupportedException();
			}

			public ulong ToUInt64(IFormatProvider provider)
			{
				throw new NotSupportedException();
			}

			#endregion
		}

		[Fact]
		public void RoundtripWithTypeConverter()
		{
			var serializer = new Serializer(SerializationOptions.Roundtrip);
			var deserializer = new Deserializer();
			var buffer = new StringWriter();
			var x = new ParameterizedCtor("Yo");

			serializer.RegisterTypeConverter(new ParameterizedCtorConverter());
			serializer.Serialize(buffer, x);
			Dump.WriteLine(buffer);

			deserializer.RegisterTypeConverter(new ParameterizedCtorConverter());
			var copy = deserializer.Deserialize<ParameterizedCtor>(new StringReader(buffer.ToString()));

			copy.Value.Should().Be("Yo");
		}

		// Fails in serialization unless a type converter is specified
		class ParameterizedCtor
		{
			public string Value;
			public ParameterizedCtor(string value) { Value = value; }
		}

		public class ParameterizedCtorConverter : IYamlTypeConverter
		{
			public bool Accepts(Type type)
			{
				return type == typeof(ParameterizedCtor);
			}

			public object ReadYaml(IParser parser, Type type)
			{
				var value = ((Scalar) parser.Current).Value;
				parser.MoveNext();
				return new ParameterizedCtor(value);
			}

			public void WriteYaml(IEmitter emitter, object value, Type type)
			{
				emitter.Emit(new Scalar(((ParameterizedCtor) value).Value));
			}
		}

		[Fact]
		public void RoundtripDictionary()
		{
			var serializer = new Serializer();
			var deserializer = new Deserializer();
			var buffer = new StringWriter();
			var entries = new Dictionary<string, string> {
				{ "key1", "value1" },
				{ "key2", "value2" },
				{ "key3", "value3" },
			};

			serializer.Serialize(buffer, entries);
			Dump.WriteLine(buffer);
			var result = deserializer.Deserialize<Dictionary<string, string>>(new StringReader(buffer.ToString()));

			result.Should().Equal(entries);
		}

		[Fact]
		public void SerializeAnonymousType()
		{
			var serializer = new Serializer();
			var deserializer = new Deserializer();
			var buffer = new StringWriter();
			var data = new { Key = 3 };

			serializer.Serialize(buffer, data);
			Dump.WriteLine(buffer);
			var result = deserializer.Deserialize<Dictionary<string, string>>(new StringReader(buffer.ToString()));

			result.Should().Equal(new Dictionary<string, string> {
				{ "Key", "3" }
			});
		}

		[Fact]
		public void SerializationIncludesNullWhenAsked()
		{
			var serializer = new Serializer(SerializationOptions.EmitDefaults);
			var buffer = new StringWriter();
			var original = new X { MyString = null };

			serializer.Serialize(buffer, original, typeof(X));
			Dump.WriteLine(buffer);

			buffer.ToString().Should().Contain("MyString");
		}

		[Fact]
		[Trait("Motive", "Bug fix")]
		public void SerializationIncludesNullWhenAskedAboutAnonymousType()
		{
			var serializer = new Serializer(SerializationOptions.EmitDefaults);
			var buffer = new StringWriter();
			var original = new { MyString = (string) null };
			
			serializer.Serialize(buffer, original, original.GetType());
			Dump.WriteLine(buffer);

			buffer.ToString().Should().Contain("MyString");
		}

		[Fact]
		public void SerializationDoesNotIncludeNullWhenNotAsked()
		{
			var serializer = new Serializer();
			var buffer = new StringWriter();
			var original = new X { MyString = null };

			serializer.Serialize(buffer, original, typeof(X));
			Dump.WriteLine(buffer);

			buffer.ToString().Should().NotContain("MyString");
		}

		[Fact]
		public void SerializationOfNullWorksInJson()
		{
			var serializer = new Serializer(SerializationOptions.EmitDefaults | SerializationOptions.JsonCompatible);
			var buffer = new StringWriter();
			var original = new X { MyString = null };

			serializer.Serialize(buffer, original, typeof(X));
			Dump.WriteLine(buffer);

			buffer.ToString().Should().Contain("MyString");
		}

		[Fact]
		public void DeserializationOfNullWorksInJson()
		{
			var options = SerializationOptions.EmitDefaults | SerializationOptions.JsonCompatible | SerializationOptions.Roundtrip;
			var serializer = new Serializer(options);
			var deserializer = new Deserializer();
			var buffer = new StringWriter();
			var original = new X { MyString = null };

			serializer.Serialize(buffer, original, typeof(X));
			Dump.WriteLine(buffer);
			var copy = deserializer.Deserialize<X>(new StringReader(buffer.ToString()));

			copy.MyString.Should().BeNull();
		}

		[Fact]
		public void SerializationRespectsYamlIgnoreAttribute()
		{
			var serializer = new Serializer();
			var deserializer = new Deserializer();
			var buffer = new StringWriter();
			var original = new ContainsIgnore { IgnoreMe = "Some Text" };

			serializer.Serialize(buffer, original);
			Dump.WriteLine(buffer);
			var copy = deserializer.Deserialize<ContainsIgnore>(new StringReader(buffer.ToString()));

			copy.IgnoreMe.Should().BeNull();
		}

		public class ContainsIgnore
		{
			[YamlIgnore]
			public String IgnoreMe { get; set; }
		}

		// Todo: is the assert on the string necessary?
		[Fact]
		public void RoundtripWithPolymorphism()
		{
			var serializer = new Serializer(SerializationOptions.Roundtrip);
			var deserializer = new Deserializer();
			var buffer = new StringWriter();

			serializer.Serialize(buffer, new ParentChildContainer {
				SomeScalar = "Hello",
				RegularParent = new Child { ParentProp = "foo", ChildProp = "bar" },
			});
			Dump.WriteLine(buffer);
			var result = deserializer.Deserialize<ParentChildContainer>(new StringReader(buffer.ToString()));

			result.SomeScalar.Should().Be("Hello");
			result.RegularParent.Should().BeOfType<Child>().And
				.Subject.As<Child>().ShouldHave().SharedProperties().EqualTo(new { ChildProp = "bar" });
		}

		[Fact]
		public void RoundtripWithSerializeAs()
		{
			var serializer = new Serializer(SerializationOptions.Roundtrip);
			var deserializer = new Deserializer();

			var buffer = new StringWriter();
			serializer.Serialize(buffer, new ParentChildContainer
			{
				SomeScalar = "Hello",
				ParentWithSerializeAs = new Child { ParentProp = "foo", ChildProp = "bar" },
			});
			Dump.WriteLine(buffer);
			var result = deserializer.Deserialize<ParentChildContainer>(new StringReader(buffer.ToString()));

			result.ParentWithSerializeAs.Should().BeOfType<Parent>().And
				.Subject.As<Parent>().ShouldHave().SharedProperties().EqualTo(new { ParentProp = "foo" });
		}

		public class ParentChildContainer
		{
			public object SomeScalar { get; set; }
			public Parent RegularParent { get; set; }
			[YamlMember(serializeAs: typeof(Parent))]
			public Parent ParentWithSerializeAs { get; set; }
		}

		public class Parent
		{
			public string ParentProp { get; set; }
		}

		public class Child : Parent
		{
			public string ChildProp { get; set; }
		}

		[Fact]
		public void SerializeArrayOfIdenticalObjects()
		{
			var z = new Z { aaa = "abc" };
			var objects = new[] { z, z, z };

			var result = SerializeThenDeserialize(objects);

			result.Should().HaveCount(3).And.OnlyContain(x => z.aaa.Equals(x.aaa));
			result[0].Should().BeSameAs(result[1]).And.BeSameAs(result[2]);
		}

		private T SerializeThenDeserialize<T>(T input)
		{
			var serializer = new Serializer();
			var writer = new StringWriter();
			serializer.Serialize(writer, input, typeof(T));

			var serialized = writer.ToString();
			Dump.WriteLine("serialized =\n-----\n{0}", serialized);

			var deserializer = new Deserializer();
			return deserializer.Deserialize<T>(new StringReader(serialized));
		}

		[Fact]
		public void BoxedArray()
		{
			var serializer = new Serializer();
			var deserializer = new Deserializer();
			var buffer = new StringWriter();

			serializer.Serialize(buffer, new object[] {1, 2, "3"});
			Dump.WriteLine(buffer);
			var result = deserializer.Deserialize<int[]>(new StringReader(buffer.ToString()));

			result.Should().Equal(1, 2, 3);
		}

		[Fact]
		public void SerializeUsingCamelCaseNaming()
		{
			var obj = new { foo = "bar", moreFoo = "More bar", evenMoreFoo = "Awesome" };

			var result = SerializeWithNaming(obj, new CamelCaseNamingConvention());

			result.Should().Contain("foo: bar").And
				.Contain("moreFoo: More bar").And
				.Contain("evenMoreFoo: Awesome");
		}

		[Fact]
		public void SerializeUsingPascalCaseNaming()
		{
			var obj = new { foo = "bar", moreFoo = "More bar", evenMoreFoo = "Awesome" };

			var result = SerializeWithNaming(obj, new PascalCaseNamingConvention());
			Dump.WriteLine(result);

			result.Should().Contain("Foo: bar").And
				.Contain("MoreFoo: More bar").And
				.Contain("EvenMoreFoo: Awesome");
		}

		[Fact]
		public void SerializeUsingHyphenationNaming()
		{
			var obj = new { foo = "bar", moreFoo = "More bar", EvenMoreFoo = "Awesome" };

			var result = SerializeWithNaming(obj, new HyphenatedNamingConvention());

			result.Should().Contain("foo: bar").And
				.Contain("more-foo: More bar").And
				.Contain("even-more-foo: Awesome");
		}

		[Fact]
		public void SerializeUsingUnderscoreNaming()
		{
			var obj = new { foo = "bar", moreFoo = "More bar", EvenMoreFoo = "Awsome" };

			var result = SerializeWithNaming(obj, new UnderscoredNamingConvention());

			result.Should().Contain("foo: bar").And
				.Contain("more_foo: More bar").And
				.Contain("even_more_foo: Awsome");
		}

		private string SerializeWithNaming<T>(T input, INamingConvention naming)
		{
			var serializer = new Serializer(namingConvention: naming);
			var writer = new StringWriter();
			serializer.Serialize(writer, input, typeof(T));
			return writer.ToString();
		}

		public class Z
		{
			public string aaa { get; set; }
		}

		// Todo: are these needed? Naming convention classes are tested elsewhere
		[Fact]
		public void DeserializeUsingCamelCaseNamingConvention()
		{
			DeserializeUsing(new CamelCaseNamingConvention(),
				"firstTest: First",
				"secondTest: Second",
				"thirdTest: Third",
				"fourthTest: Fourth");
		}

		[Fact]
		public void DeserializeUsingHyphenatedNamingConvention()
		{
			DeserializeUsing(new HyphenatedNamingConvention(),
				"first-test: First",
				"second-test: Second",
				"third-test: Third",
				"fourthTest: Fourth");
		}

		[Fact]
		public void DeserializeUsingPascalCaseNamingConvention()
		{
			DeserializeUsing(new PascalCaseNamingConvention(),
				"FirstTest: First",
				"SecondTest: Second",
				"ThirdTest: Third",
				"fourthTest: Fourth");
		}

		[Fact]
		public void DeserializeUsingUnderscoredNamingConvention()
		{
			DeserializeUsing(new UnderscoredNamingConvention(),
				"first_test: First",
				"second_test: Second",
				"third_test: Third",
				"fourthTest: Fourth");
		}

		private void DeserializeUsing(INamingConvention convention, params string[] yaml)
		{
			var deserializer = new Deserializer(namingConvention: convention);

			var reader = new StringReader(Lines(yaml));
			var result = deserializer.Deserialize<ConventionTest>(reader);

			result.ShouldHave().SharedProperties().EqualTo(new {
				FirstTest = "First",
				SecondTest = "Second",
				ThirdTest = "Third",
				AliasTest = "Fourth"
			});
		}

		[Fact]
		public void RoundtripAlias()
		{
			var serializer = new Serializer();
			var deserializer = new Deserializer();
			var writer = new StringWriter();
			var input = new ConventionTest { AliasTest = "Fourth" };

			serializer.Serialize(writer, input, input.GetType());
			var serialized = writer.ToString();

			// Todo: use RegEx once FluentAssertions 2.2 is released
			serialized.TrimEnd('\r', '\n').Should().Be("fourthTest: Fourth");

			var output = deserializer.Deserialize<ConventionTest>(new StringReader(serialized));

			output.AliasTest.Should().Be(input.AliasTest);
		}

		public class ConventionTest
		{
			public string FirstTest { get; set; }
			public string SecondTest { get; set; }
			public string ThirdTest { get; set; }
			[YamlAlias("fourthTest")]
			public string AliasTest { get; set; }
			[YamlIgnore]
			public string fourthTest { get; set; }
		}

		[Fact]
		public void DefaultValueAttributeIsUsedWhenPresentWithoutEmitDefaults()
		{
			var serializer = new Serializer();
			var writer = new StringWriter();
			var input = new HasDefaults { Value = HasDefaults.DefaultValue };

			serializer.Serialize(writer, input);
			var serialized = writer.ToString();
			Dump.WriteLine(serialized);

			serialized.Should().NotContain("Value");
		}

		[Fact]
		public void DefaultValueAttributeIsIgnoredWhenPresentWithEmitDefaults()
		{
			var serializer = new Serializer(SerializationOptions.EmitDefaults);
			var writer = new StringWriter();
			var input = new HasDefaults { Value = HasDefaults.DefaultValue };

			serializer.Serialize(writer, input);
			var serialized = writer.ToString();
			Dump.WriteLine(serialized);

			serialized.Should().Contain("Value");
		}

		[Fact]
		public void DefaultValueAttributeIsIgnoredWhenValueIsDifferent()
		{
			var serializer = new Serializer();
			var writer = new StringWriter();
			var input = new HasDefaults { Value = "non-default" };

			serializer.Serialize(writer, input);
			var serialized = writer.ToString();
			Dump.WriteLine(serialized);

			serialized.Should().Contain("Value");
		}

		public class HasDefaults
		{
			public const string DefaultValue = "myDefault";

			[DefaultValue(DefaultValue)]
			public string Value { get; set; }
		}

		[Fact]
		public void NullValuesInListsAreAlwaysEmittedWithoutEmitDefaults()
		{
			var serializer = new Serializer();
			var writer = new StringWriter();
			var input = new[] { "foo", null, "bar" };

			serializer.Serialize(writer, input);
			var serialized = writer.ToString();
			Dump.WriteLine(serialized);

			Regex.Matches(serialized, "-").Count.Should().Be(3, "there should have been 3 elements");
		}

		[Fact]
		public void NullValuesInListsAreAlwaysEmittedWithEmitDefaults()
		{
			var serializer = new Serializer(SerializationOptions.EmitDefaults);
			var writer = new StringWriter();
			var input = new[] { "foo", null, "bar" };

			serializer.Serialize(writer, input);
			var serialized = writer.ToString();
			Dump.WriteLine(serialized);

			Regex.Matches(serialized, "-").Count.Should().Be(3, "there should have been 3 elements");
		}

		[Fact]
		public void DeserializeTwoDocuments()
		{
			var deserializer = new Deserializer();
			var reader = EventReaderForYaml(Lines(
				"---",
				"Name: Andy",
				"---",
				"Name: Brad",
				"..."));

			reader.Expect<StreamStart>();
			var andy = deserializer.Deserialize<Person>(reader);
			var brad = deserializer.Deserialize<Person>(reader);

			andy.ShouldHave().AllProperties().EqualTo(new { Name = "Andy" });
			brad.ShouldHave().AllProperties().EqualTo(new { Name = "Brad" });
		}

		[Fact]
		public void DeserializeManyDocuments()
		{
			var deserializer = new Deserializer();
			var reader = EventReaderForYaml(Lines(
				"---",
				"Name: Andy",
				"---",
				"Name: Brad",
				"---",
				"Name: Charles",
				"..."));

			reader.Allow<StreamStart>();

			var people = new List<Person>();
			while (!reader.Accept<StreamEnd>())
			{
				var person = deserializer.Deserialize<Person>(reader);
				people.Add(person);
			}

			people.Should().HaveCount(3);
			people[0].ShouldHave().AllProperties().EqualTo(new { Name = "Andy" });
			people[1].ShouldHave().AllProperties().EqualTo(new { Name = "Brad" });
			people[2].ShouldHave().AllProperties().EqualTo(new { Name = "Charles" });
		}

		private static EventReader EventReaderForYaml(string yaml)
		{
			return new EventReader(new Parser(new StringReader(yaml)));
		}

		private string Lines(params string[] lines)
		{
			return string.Join(Environment.NewLine, lines);
		}

		public class Person
		{
			public string Name { get; set; }
		}

		[Fact]
		public void DeserializeEmptyDocument()
		{
			var deserializer = new Deserializer();

			var array = deserializer.Deserialize<int[]>(new StringReader(""));

			array.Should().BeNull();
		}

		[Fact]
		public void SerializeGenericDictionaryShouldNotThrowTargetException()
		{
			var serializer = new Serializer();
			var buffer = new StringWriter();

			Action action = () => serializer.Serialize(buffer, new OnlyGenericDictionary {
				{ "hello", "world" },
			});

			action.ShouldNotThrow<TargetException>();
		}

		private class OnlyGenericDictionary : IDictionary<string, string>
		{
			private readonly Dictionary<string, string> dictionary = new Dictionary<string, string>();

			public void Add(string key, string value)
			{
				dictionary.Add(key, value);
			}

			public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
			{
				return dictionary.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			#region Unused Members

			public bool ContainsKey(string key)
			{
				throw new NotSupportedException();
			}

			public ICollection<string> Keys
			{
				get { throw new NotSupportedException(); }
			}

			public bool Remove(string key)
			{
				throw new NotSupportedException();
			}

			public bool TryGetValue(string key, out string value)
			{
				throw new NotSupportedException();
			}

			public ICollection<string> Values
			{
				get { throw new NotSupportedException(); }
			}

			public string this[string key]
			{
				get { throw new NotSupportedException(); }
				set { throw new NotSupportedException(); }
			}

			public void Add(KeyValuePair<string, string> item)
			{
				throw new NotSupportedException();
			}

			public void Clear()
			{
				throw new NotSupportedException();
			}

			public bool Contains(KeyValuePair<string, string> item)
			{
				throw new NotSupportedException();
			}

			public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
			{
				throw new NotSupportedException();
			}

			public int Count
			{
				get { throw new NotSupportedException(); }
			}

			public bool IsReadOnly
			{
				get { throw new NotSupportedException(); }
			}

			public bool Remove(KeyValuePair<string, string> item)
			{
				throw new NotSupportedException();
			}

			#endregion
		}

		[Fact]
		public void ForwardReferencesWorkInGenericLists()
		{
			var deserializer = new Deserializer();
			var reader = new StringReader(Lines(
				"- *forward",
				"- &forward ForwardReference"));

			var result = deserializer.Deserialize<string[]>(reader);

			result.Should().Equal(new[] { "ForwardReference", "ForwardReference" });
		}

		[Fact]
		public void ForwardReferencesWorkInNonGenericLists()
		{
			var deserializer = new Deserializer();

			var reader = new StringReader(Lines(
				"- *forward",
				"- &forward ForwardReference"));

			var result = deserializer.Deserialize<ArrayList>(reader);

			result.Should().Equal(new[] { "ForwardReference", "ForwardReference" });
		}

		[Fact]
		public void ForwardReferencesWorkInGenericDictionaries()
		{
			var deserializer = new Deserializer();
			var reader = new StringReader(Lines(
				"key1: *forward",
				"*forwardKey: ForwardKeyValue",
				"*forward: *forward",
				"key2: &forward ForwardReference",
				"key3: &forwardKey key4"));

			var result = deserializer.Deserialize<Dictionary<string, string>>(reader);

			result.Should().Equal(new Dictionary<string, string> {
				{ "ForwardReference", "ForwardReference" },
				{ "key1", "ForwardReference" },
				{ "key2", "ForwardReference" },
				{ "key4", "ForwardKeyValue" },
				{ "key3", "key4" }
			});
		}

		[Fact]
		public void ForwardReferencesWorkInNonGenericDictionaries()
		{
			var deserializer = new Deserializer();
			var reader = new StringReader(Lines(
				"key1: *forward",
				"*forwardKey: ForwardKeyValue",
				"*forward: *forward",
				"key2: &forward ForwardReference",
				"key3: &forwardKey key4"));

			var result = deserializer.Deserialize<Hashtable>(reader);

			result.Should().BeEquivalentTo(
				Entry("ForwardReference", "ForwardReference"),
				Entry("key1", "ForwardReference"),
				Entry("key2", "ForwardReference"),
				Entry("key4", "ForwardKeyValue"),
				Entry("key3", "key4"));
		}

		private object Entry(string key, string value)
		{
			return new DictionaryEntry(key, value);
		}

		[Fact]
		public void ForwardReferencesWorkInObjects()
		{
			var deserializer = new Deserializer();
			var reader = new StringReader(Lines(
				"Nothing: *forward",
				"MyString: &forward ForwardReference"));

			var result = deserializer.Deserialize<X>(reader);

			result.ShouldHave().SharedProperties().EqualTo(
				new { Nothing = "ForwardReference", MyString = "ForwardReference" });
		}

		[Fact]
		public void UndefinedForwardReferencesFail()
		{
			var deserializer = new Deserializer();
			var reader = new StringReader(Lines(
					"Nothing: *forward",
					"MyString: ForwardReference"));

			Action action = () => deserializer.Deserialize<X>(reader);

			action.ShouldThrow<AnchorNotFoundException>();
		}

		public class X
		{
			public bool MyFlag { get; set; }
			public string Nothing { get; set; }
			public int MyInt { get; set; }
			public double MyDouble { get; set; }
			public string MyString { get; set; }
			public DateTime MyDate { get; set; }
			public TimeSpan MyTimeSpan { get; set; }
			public Point MyPoint { get; set; }
			public int? MyNullableWithValue { get; set; }
			public int? MyNullableWithoutValue { get; set; }

			public X()
			{
				MyInt = 1234;
				MyDouble = 6789.1011;
				MyString = "Hello world";
				MyDate = DateTime.Now;
				MyTimeSpan = TimeSpan.FromHours(1);
				MyPoint = new Point(100, 200);
				MyNullableWithValue = 8;
			}
		}
	}
}

