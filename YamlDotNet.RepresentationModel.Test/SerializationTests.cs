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
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Core.Test;
using YamlDotNet.RepresentationModel.Serialization;
using YamlDotNet.RepresentationModel.Serialization.NamingConventions;

namespace YamlDotNet.RepresentationModel.Test
{
	public class SerializationTests : YamlTest
	{
		[Fact]
		public void Roundtrip()
		{
			var buffer = new StringWriter();
			var serializer = new Serializer(SerializationOptions.Roundtrip);

			var original = new X();
			serializer.Serialize(buffer, original);

			Dump.WriteLine(buffer);

			var deserializer = new Deserializer();
			var copy = deserializer.Deserialize<X>(new StringReader(buffer.ToString()));

			foreach (var property in typeof(X).GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (property.CanRead && property.CanWrite)
				{
					Assert.Equal(
						property.GetValue(original, null),
						property.GetValue(copy, null));
				}
			}
		}

		[Fact]
		public void RoundtripWithDefaults()
		{
			var buffer = new StringWriter();
			var serializer = new Serializer(SerializationOptions.Roundtrip | SerializationOptions.EmitDefaults);

			var original = new X();
			serializer.Serialize(buffer, original);

			Dump.WriteLine(buffer);

			var deserializer = new Deserializer();
			var copy = deserializer.Deserialize<X>(new StringReader(buffer.ToString()));

			foreach (var property in typeof(X).GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (property.CanRead && property.CanWrite)
				{
					Assert.Equal(
						property.GetValue(original, null),
						property.GetValue(copy, null));
				}
			}
		}

		[Fact]
		public void CircularReference()
		{
			var serializer = new Serializer(SerializationOptions.Roundtrip);

			var buffer = new StringWriter();
			var original = new Y();
			original.Child = new Y {
				Child = original,
				Child2 = original
			};

			serializer.Serialize(buffer, original, typeof(Y));

			Dump.WriteLine(buffer);
		}

		private class Y
		{
			public Y Child { get; set; }
			public Y Child2 { get; set; }
		}

		[Fact]
		public void DeserializeScalar()
		{
			var sut = new Deserializer();
			var result = sut.Deserialize(YamlFile("test2.yaml"), typeof(object));

			Assert.Equal("a scalar", result);
		}

		[Fact]
		public void DeserializeExplicitType()
		{
			var serializer = new Deserializer();
			object result = serializer.Deserialize(YamlFile("explicitType.yaml"), typeof(object));

			Assert.True(typeof(Z).IsAssignableFrom(result.GetType()));
			Assert.Equal("bbb", ((Z)result).aaa);
		}

		[Fact]
		public void DeserializeDictionary()
		{
			var serializer = new Deserializer();
			var result = serializer.Deserialize(YamlFile("dictionary.yaml"));

			Assert.True(typeof(IDictionary<object, object>).IsAssignableFrom(result.GetType()), "The deserialized object has the wrong type.");

			var dictionary = (IDictionary<object, object>)result;
			Assert.Equal("value1", dictionary["key1"]);
			Assert.Equal("value2", dictionary["key2"]);
		}

		[Fact]
		public void DeserializeExplicitDictionary()
		{
			var serializer = new Deserializer();
			object result = serializer.Deserialize(YamlFile("dictionaryExplicit.yaml"));

			Assert.True(typeof(IDictionary<string, int>).IsAssignableFrom(result.GetType()), "The deserialized object has the wrong type.");

			var dictionary = (IDictionary<string, int>)result;
			Assert.Equal(1, dictionary["key1"]);
			Assert.Equal(2, dictionary["key2"]);
		}

		[Fact]
		public void DeserializeListOfDictionaries()
		{
			var serializer = new Deserializer();
			var result = serializer.Deserialize(YamlFile("listOfDictionaries.yaml"), typeof(List<Dictionary<string, string>>));

			Assert.IsType<List<Dictionary<string, string>>>(result);

			var list = (List<Dictionary<string, string>>)result;
			Assert.Equal("conn1", list[0]["connection"]);
			Assert.Equal("path1", list[0]["path"]);
			Assert.Equal("conn2", list[1]["connection"]);
			Assert.Equal("path2", list[1]["path"]);
		}

		[Fact]
		public void DeserializeList()
		{
			var serializer = new Deserializer();
			var result = serializer.Deserialize(YamlFile("list.yaml"));

			Assert.True(typeof(IList).IsAssignableFrom(result.GetType()));

			var list = (IList)result;
			Assert.Equal("one", list[0]);
			Assert.Equal("two", list[1]);
			Assert.Equal("three", list[2]);
		}

		[Fact]
		public void DeserializeExplicitList()
		{
			var serializer = new Deserializer();
			var result = serializer.Deserialize(YamlFile("listExplicit.yaml"));

			Assert.True(typeof(IList<int>).IsAssignableFrom(result.GetType()));

			var list = (IList<int>)result;
			Assert.Equal(3, list[0]);
			Assert.Equal(4, list[1]);
			Assert.Equal(5, list[2]);
		}

		[Fact]
		public void DeserializeEnumerable()
		{
			var serializer = new Serializer();
			var buffer = new StringWriter();
			var z = new[] { new Z { aaa = "Yo" }};
			serializer.Serialize(buffer, z);

			var deserializer = new Deserializer();
			var result = (IEnumerable<Z>)deserializer.Deserialize(new StringReader(buffer.ToString()), typeof(IEnumerable<Z>));
			Assert.Equal(1, result.Count());
			Assert.Equal("Yo", result.First().aaa);
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

			var copy = (List<int>) deserializer.Deserialize(new StringReader(buffer.ToString()), typeof(List<int>));

			Assert.Equal(original.Count, copy.Count);

			for (int i = 0; i < original.Count; ++i)
			{
				Assert.Equal(original[i], copy[i]);
			}
		}

		[Fact]
		public void DeserializeArray()
		{
			var serializer = new Deserializer();
			var result = serializer.Deserialize(YamlFile("list.yaml"), typeof(String[]));

			Assert.True(result is String[]);

			var array = (String[])result;
			Assert.Equal("one", array[0]);
			Assert.Equal("two", array[1]);
			Assert.Equal("three", array[2]);
		}

		[Fact]
		public void Enums()
		{
			var serializer = new Serializer();
			var deserializer = new Deserializer();

			var flags = StringFormatFlags.NoClip | StringFormatFlags.NoFontFallback;

			var buffer = new StringWriter();
			serializer.Serialize(buffer, flags);

			var deserialized = (StringFormatFlags)deserializer.Deserialize(new StringReader(buffer.ToString()), typeof(StringFormatFlags));

			Assert.Equal(flags, deserialized);
		}

		[Fact]
		public void CustomTags()
		{
			var deserializer = new Deserializer();
			deserializer.RegisterTagMapping("tag:yaml.org,2002:point", typeof(Point));
			var result = deserializer.Deserialize(YamlFile("tags.yaml"));

			Assert.Equal(typeof(Point), result.GetType());

			var value = (Point)result;
			Assert.Equal(10, value.X);
			Assert.Equal(20, value.Y);
		}

		[Fact]
		public void DeserializeConvertible()
		{
			var serializer = new Deserializer();
			var result = serializer.Deserialize(YamlFile("convertible.yaml"), typeof(Z));

			Assert.True(typeof(Z).IsAssignableFrom(result.GetType()));
			Assert.Equal("[hello, world]", ((Z)result).aaa);
		}

		// Todo: these two classes aren't used in any tests
		public class Converter : System.ComponentModel.TypeConverter
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
				var parts = ((string)value).Split(' ');
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
			public string Left
			{
				get;
				set;
			}

			public string Right
			{
				get;
				set;
			}

			public string ToString(IFormatProvider provider) {
				Assert.Equal(CultureInfo.InvariantCulture, provider);
				return string.Format(provider, "[{0}, {1}]", Left, Right);
			}

			public object ToType(Type conversionType, IFormatProvider provider) {
				Assert.Equal(typeof(string), conversionType);
				return ToString(provider);
			}

			#region not implemented IConvertible Members

			public TypeCode GetTypeCode()
			{
				throw new NotImplementedException();
			}

			public bool ToBoolean(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public byte ToByte(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public char ToChar(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public DateTime ToDateTime(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public decimal ToDecimal(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public double ToDouble(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public short ToInt16(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public int ToInt32(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public long ToInt64(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public sbyte ToSByte(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public float ToSingle(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public ushort ToUInt16(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public uint ToUInt32(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			public ulong ToUInt64(IFormatProvider provider)
			{
				throw new NotImplementedException();
			}

			#endregion
		}

		[Fact]
		public void RoundtripWithTypeConverter()
		{
			var buffer = new StringWriter();
			var x = new SomeCustomType("Yo");
			var serializer = new Serializer(SerializationOptions.Roundtrip);
			serializer.RegisterTypeConverter(new CustomTypeConverter());
			serializer.Serialize(buffer, x);

			Dump.WriteLine(buffer);

			var deserializer = new Deserializer();
			deserializer.RegisterTypeConverter(new CustomTypeConverter());

			var copy = deserializer.Deserialize<SomeCustomType>(new StringReader(buffer.ToString()));
			Assert.Equal("Yo", copy.Value);
		}

		class SomeCustomType {
			// Test specifically with no parameterless, supposed to fail unless a type converter is specified
			public SomeCustomType(string value) { Value = value; }
			public string Value;
		}

		public class CustomTypeConverter : IYamlTypeConverter {
			public bool Accepts(Type type) {
				return type == typeof(SomeCustomType);
			}

			public object ReadYaml(IParser parser, Type type) {
				var value = ((Scalar)parser.Current).Value;
				parser.MoveNext();
				return new SomeCustomType(value);
			}

			public void WriteYaml(IEmitter emitter, object value, Type type) {
				emitter.Emit(new Scalar(((SomeCustomType)value).Value));
			}
		}

		[Fact]
		public void RoundtripDictionary()
		{
			var entries = new Dictionary<string, string>
			{
				{ "key1", "value1" },
				{ "key2", "value2" },
				{ "key3", "value3" },
			};

			var buffer = new StringWriter();
			var serializer = new Serializer();
			serializer.Serialize(buffer, entries);

			Dump.WriteLine(buffer);

			var deserializer = new Deserializer();
			var deserialized = deserializer.Deserialize<Dictionary<string, string>>(new StringReader(buffer.ToString()));

			foreach (var pair in deserialized)
			{
				Assert.Equal(entries[pair.Key], pair.Value);
			}
		}

		[Fact]
		public void SerializeAnonymousType()
		{
			var data = new { Key = 3 };

			var serializer = new Serializer();

			var buffer = new StringWriter();
			serializer.Serialize(buffer, data);

			Dump.WriteLine(buffer);

			var deserializer = new Deserializer();
			var parsed = deserializer.Deserialize<Dictionary<string, string>>(new StringReader(buffer.ToString()));

			Assert.NotNull(parsed);
			Assert.Equal(1, parsed.Count);
		}

		[Fact]
		public void SerializationIncludesNullWhenAsked_BugFix()
		{
			var serializer = new Serializer(SerializationOptions.EmitDefaults);
			
			var buffer = new StringWriter();
			var original = new { MyString = (string) null };
			serializer.Serialize(buffer, original, original.GetType());

			Dump.WriteLine(buffer);
			Assert.True(buffer.ToString().Contains("MyString"));
		}

		[Fact]
		public void SerializationIncludesNullWhenAsked()
		{
			var serializer = new Serializer(SerializationOptions.EmitDefaults);

			var buffer = new StringWriter();
			var original = new X { MyString = null };
			serializer.Serialize(buffer, original, typeof(X));

			Dump.WriteLine(buffer);
			Assert.True(buffer.ToString().Contains("MyString"));
		}

		[Fact]
		public void SerializationDoesNotIncludeNullWhenNotAsked()
		{
			var buffer = new StringWriter();
			var original = new X { MyString = null };
			var serializer = new Serializer();

			serializer.Serialize(buffer, original, typeof(X));

			Dump.WriteLine(buffer);
			Assert.False(buffer.ToString().Contains("MyString"));
		}

		[Fact]
		public void SerializationOfNullWorksInJson()
		{
			var serializer = new Serializer(SerializationOptions.EmitDefaults | SerializationOptions.JsonCompatible);

			var buffer = new StringWriter();
			var original = new X { MyString = null };
			serializer.Serialize(buffer, original, typeof(X));

			Dump.WriteLine(buffer);
			Assert.True(buffer.ToString().Contains("MyString"));
		}

		[Fact]
		public void DeserializationOfNullWorksInJson()
		{
			var serializer = new Serializer(
				SerializationOptions.EmitDefaults | SerializationOptions.JsonCompatible | SerializationOptions.Roundtrip);
			var deserializer = new Deserializer();

			var buffer = new StringWriter();
			var original = new X { MyString = null };
			serializer.Serialize(buffer, original, typeof(X));

			Dump.WriteLine(buffer);

			var copy = (X) deserializer.Deserialize(new StringReader(buffer.ToString()), typeof(X));

			Assert.Null(copy.MyString);
		}

		[Fact]
		public void SerializationRespectsYamlIgnoreAttribute()
		{
			var serializer = new Serializer();
			var deserializer = new Deserializer();

			var buffer = new StringWriter();
			var orig = new ContainsIgnore { IgnoreMe = "Some Text" };
			serializer.Serialize(buffer, orig);

			Dump.WriteLine(buffer);
			
			var copy = (ContainsIgnore) deserializer.Deserialize(new StringReader(buffer.ToString()), typeof(ContainsIgnore));
			
			Assert.Null(copy.IgnoreMe);
		}

		class ContainsIgnore
		{
			[YamlIgnore]
			public String IgnoreMe { get; set; }
		}

		[Fact]
		public void SerializeArrayOfIdenticalObjects()
		{
			var obj1 = new Z { aaa = "abc" };

			var objects = new[] { obj1, obj1, obj1 };

			var result = SerializeThenDeserialize(objects);

			Assert.NotNull(result);
			Assert.Equal(3, result.Length);
			Assert.Equal(obj1.aaa, result[0].aaa);
			Assert.Equal(obj1.aaa, result[1].aaa);
			Assert.Equal(obj1.aaa, result[2].aaa);
			Assert.Same(result[0], result[1]);
			Assert.Same(result[1], result[2]);
		}

		[Fact]
		public void SerializeUsingCamelCaseNaming()
		{
			var obj = new { foo = "bar", moreFoo = "More bar", evenMoreFoo = "Awesome" };

			var result = SerializeWithNaming(obj, new CamelCaseNamingConvention());

			Assert.Contains("foo: bar", result);
			Assert.Contains("moreFoo: More bar", result);
			Assert.Contains("evenMoreFoo: Awesome", result);
		}

		[Fact]
		public void SerializeUsingPascalCaseNaming()
		{
			var obj = new { foo = "bar", moreFoo = "More bar", evenMoreFoo = "Awesome" };

			var result = SerializeWithNaming(obj, new PascalCaseNamingConvention());

			Dump.WriteLine(result);
			Assert.Contains("Foo: bar", result);
			Assert.Contains("MoreFoo: More bar", result);
			Assert.Contains("EvenMoreFoo: Awesome", result);
		}


		[Fact]
		public void SerializeUsingHyphenation()
		{
			var obj = new { foo = "bar", moreFoo = "More bar", EvenMoreFoo = "Awesome" };

			var result = SerializeWithNaming(obj, new HyphenatedNamingConvention());

			Assert.Contains("foo: bar", result);
			Assert.Contains("more-foo: More bar", result);
			Assert.Contains("even-more-foo: Awesome", result);
		}

		private string SerializeWithNaming<T>(T input, INamingConvention naming)
		{
			var serializer = new Serializer(namingConvention: naming);
			var writer = new StringWriter();
			serializer.Serialize(writer, input, typeof(T));
			return writer.ToString();
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

		public class Z {
			public string aaa { get; set; }
		}

		private void DeserializeUsingNamingConvention(string yaml, INamingConvention convention)
		{
			var serializer = new Deserializer(namingConvention: convention);

			var result = serializer.Deserialize<ConventionTest>(YamlText(yaml));

			Assert.Equal("First", result.FirstTest);
			Assert.Equal("Second", result.SecondTest);
			Assert.Equal("Third", result.ThirdTest);
			Assert.Equal("Fourth", result.AliasTest);
		}

		// Todo: are these needed? Naming convention classes are tested elsewhere
		[Fact]
		public void DeserializeUsingCamelCaseNamingConvention()
		{
			DeserializeUsingNamingConvention(@"
				firstTest: First
				secondTest: Second
				thirdTest: Third
				fourthTest: Fourth
			", new CamelCaseNamingConvention());
		}

		[Fact]
		public void DeserializeUsingHyphenatedNamingConvention()
		{
			DeserializeUsingNamingConvention(@"
				first-test: First
				second-test: Second
				third-test: Third
				fourthTest: Fourth
			", new HyphenatedNamingConvention());
		}

		[Fact]
		public void DeserializeUsingPascalCaseNamingConvention()
		{
			DeserializeUsingNamingConvention(@"
				FirstTest: First
				SecondTest: Second
				ThirdTest: Third
				fourthTest: Fourth
			", new PascalCaseNamingConvention());
		}

		[Fact]
		public void DeserializeUsingUnderscoredNamingConvention()
		{
			DeserializeUsingNamingConvention(@"
				first_test: First
				second_test: Second
				third_test: Third
				fourthTest: Fourth
			", new UnderscoredNamingConvention());
		}

		[Fact]
		public void RoundtripAlias()
		{
			var input = new ConventionTest { AliasTest = "Fourth" };
			var serializer = new Serializer();
			var writer = new StringWriter();
			serializer.Serialize(writer, input, input.GetType());
			var serialized = writer.ToString();

			// Ensure serialisation is correct
			Assert.Equal("fourthTest: Fourth", serialized.TrimEnd('\r', '\n'));

			var deserializer = new Deserializer();
			var output = deserializer.Deserialize<ConventionTest>(new StringReader(serialized));

			// Ensure round-trip retains value
			Assert.Equal(input.AliasTest, output.AliasTest);
		}

		private class ConventionTest {
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
			var input = new HasDefaults { Value = HasDefaults.DefaultValue };
			var serializer = new Serializer();
			var writer = new StringWriter();

			serializer.Serialize(writer, input);
			var serialized = writer.ToString();

			Dump.WriteLine(serialized);
			Assert.False(serialized.Contains("Value"));
		}

		[Fact]
		public void DefaultValueAttributeIsIgnoredWhenPresentWithEmitDefaults()
		{
			var input = new HasDefaults { Value = HasDefaults.DefaultValue };
			var serializer = new Serializer(SerializationOptions.EmitDefaults);
			var writer = new StringWriter();

			serializer.Serialize(writer, input);
			var serialized = writer.ToString();

			Dump.WriteLine(serialized);
			Assert.True(serialized.Contains("Value"));
		}

		[Fact]
		public void DefaultValueAttributeIsIgnoredWhenValueIsDifferent()
		{
			var input = new HasDefaults { Value = "non-default" };
			var serializer = new Serializer();
			var writer = new StringWriter();

			serializer.Serialize(writer, input);
			var serialized = writer.ToString();

			Dump.WriteLine(serialized);

			Assert.True(serialized.Contains("Value"));
		}

		public class HasDefaults {
			public const string DefaultValue = "myDefault";

			[DefaultValue(DefaultValue)]
			public string Value { get; set; }
		}

		[Fact]
		public void NullValuesInListsAreAlwaysEmittedWithoutEmitDefaults()
		{
			var input = new[] { "foo", null, "bar" };
			var serializer = new Serializer();
			var writer = new StringWriter();

			serializer.Serialize(writer, input);
			var serialized = writer.ToString();

			Dump.WriteLine(serialized);
			Assert.Equal(3, Regex.Matches(serialized, "-").Count);
		}

		[Fact]
		public void NullValuesInListsAreAlwaysEmittedWithEmitDefaults()
		{
			var input = new[] { "foo", null, "bar" };
			var serializer = new Serializer(SerializationOptions.EmitDefaults);
			var writer = new StringWriter();

			serializer.Serialize(writer, input);
			var serialized = writer.ToString();

			Dump.WriteLine(serialized);
			Assert.Equal(3, Regex.Matches(serialized, "-").Count);
		}

		[Fact]
		public void DeserializeTwoDocuments()
		{
			var yaml = @"---
Name: Andy
---
Name: Brad
...";
			var serializer = new Deserializer();
			var reader = new EventReader(new Parser(new StringReader(yaml)));

			reader.Expect<StreamStart>();

			var andy = serializer.Deserialize<Person>(reader);
			Assert.NotNull(andy);
			Assert.Equal("Andy", andy.Name);

			var brad = serializer.Deserialize<Person>(reader);
			Assert.NotNull(brad);
			Assert.Equal("Brad", brad.Name);
		}

		[Fact]
		public void DeserializeManyDocuments()
		{
			var yaml = @"---
Name: Andy
---
Name: Brad
---
Name: Charles
...";
			var serializer = new Deserializer();
			var reader = new EventReader(new Parser(new StringReader(yaml)));

			reader.Allow<StreamStart>();

			var people = new List<Person>();
			while (!reader.Accept<StreamEnd>())
			{
				var person = serializer.Deserialize<Person>(reader);
				people.Add(person);
			}

			Assert.Equal(3, people.Count);
			Assert.Equal("Andy", people[0].Name);
			Assert.Equal("Brad", people[1].Name);
			Assert.Equal("Charles", people[2].Name);
		}

		public class Person {
			public string Name { get; set; }
		}

		[Fact]
		public void DeserializeEmptyDocument()
		{
			var deserializer = new Deserializer();
			var array = (int[])deserializer.Deserialize(new StringReader(""), typeof(int[]));
			Assert.Null(array);
		}

		[Fact]
		public void SerializeGenericDictionaryShouldNotThrowTargetException()
		{
			var serializer = new Serializer();

			var buffer = new StringWriter();
			serializer.Serialize(buffer, new OnlyGenericDictionary
			{
				{ "hello", "world" },
			});
		}

		private class OnlyGenericDictionary : IDictionary<string, string>
		{
			private readonly Dictionary<string, string> _dictionary = new Dictionary<string, string>();

			#region IDictionary<string,string> Members

			public void Add(string key, string value)
			{
				_dictionary.Add(key, value);
			}

			public bool ContainsKey(string key)
			{
				throw new NotImplementedException();
			}

			public ICollection<string> Keys
			{
				get { throw new NotImplementedException(); }
			}

			public bool Remove(string key)
			{
				throw new NotImplementedException();
			}

			public bool TryGetValue(string key, out string value)
			{
				throw new NotImplementedException();
			}

			public ICollection<string> Values
			{
				get { throw new NotImplementedException(); }
			}

			public string this[string key]
			{
				get
				{
					throw new NotImplementedException();
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			#endregion

			#region ICollection<KeyValuePair<string,string>> Members

			public void Add(KeyValuePair<string, string> item)
			{
				throw new NotImplementedException();
			}

			public void Clear()
			{
				throw new NotImplementedException();
			}

			public bool Contains(KeyValuePair<string, string> item)
			{
				throw new NotImplementedException();
			}

			public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
			{
				throw new NotImplementedException();
			}

			public int Count
			{
				get { throw new NotImplementedException(); }
			}

			public bool IsReadOnly
			{
				get { throw new NotImplementedException(); }
			}

			public bool Remove(KeyValuePair<string, string> item)
			{
				throw new NotImplementedException();
			}

			#endregion

			#region IEnumerable<KeyValuePair<string,string>> Members

			public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
			{
				return _dictionary.GetEnumerator();
			}

			#endregion

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator()
			{
				return _dictionary.GetEnumerator();
			}

			#endregion
		}

		[Fact]
		public void ForwardReferencesWorkInGenericLists()
		{
			var deserializer = new Deserializer();

			var result = deserializer.Deserialize<string[]>(YamlText(@"
				- *forward
				- &forward ForwardReference
			"));

			Assert.Equal(2, result.Length);
			Assert.Equal("ForwardReference", result[0]);
			Assert.Equal("ForwardReference", result[1]);
		}

		[Fact]
		public void ForwardReferencesWorkInNonGenericLists()
		{
			var deserializer = new Deserializer();

			var result = deserializer.Deserialize<ArrayList>(YamlText(@"
				- *forward
				- &forward ForwardReference
			"));

			Assert.Equal(2, result.Count);
			Assert.Equal("ForwardReference", result[0]);
			Assert.Equal("ForwardReference", result[1]);
		}

		[Fact]
		public void ForwardReferencesWorkInGenericDictionaries()
		{
			var deserializer = new Deserializer();

			var result = deserializer.Deserialize<Dictionary<string, string>>(YamlText(@"
				key1: *forward
				*forwardKey: ForwardKeyValue
				*forward: *forward
				key2: &forward ForwardReference
				key3: &forwardKey key4
			"));

			Assert.Equal(5, result.Count);
			Assert.Equal("ForwardReference", result["ForwardReference"]);
			Assert.Equal("ForwardReference", result["key1"]);
			Assert.Equal("ForwardReference", result["key2"]);
			Assert.Equal("ForwardKeyValue", result["key4"]);
			Assert.Equal("key4", result["key3"]);
		}

		[Fact]
		public void ForwardReferencesWorkInNonGenericDictionaries()
		{
			var deserializer = new Deserializer();

			var result = deserializer.Deserialize<Hashtable>(YamlText(@"
				key1: *forward
				*forwardKey: ForwardKeyValue
				*forward: *forward
				key2: &forward ForwardReference
				key3: &forwardKey key4
			"));

			Assert.Equal(5, result.Count);
			Assert.Equal("ForwardReference", result["ForwardReference"]);
			Assert.Equal("ForwardReference", result["key1"]);
			Assert.Equal("ForwardReference", result["key2"]);
			Assert.Equal("ForwardKeyValue", result["key4"]);
			Assert.Equal("key4", result["key3"]);
		}

		[Fact]
		public void ForwardReferencesWorkInObjects()
		{
			var deserializer = new Deserializer();

			var result = deserializer.Deserialize<X>(YamlText(@"
				Nothing: *forward
				MyString: &forward ForwardReference
			"));

			Assert.Equal("ForwardReference", result.Nothing);
			Assert.Equal("ForwardReference", result.MyString);
		}

		[Fact]
		public void UndefinedForwardReferencesFail()
		{
			var deserializer = new Deserializer();

			Assert.Throws<AnchorNotFoundException>(() =>
				deserializer.Deserialize<X>(YamlText(@"
					Nothing: *forward
					MyString: ForwardReference
				"))
			);
		}

		private class X
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

