//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2008, 2009, 2010, 2011, 2012 Antoine Aubry
	
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
using System.Drawing;
using Xunit;
using System.IO;
using YamlDotNet.RepresentationModel.Serialization;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Core.Events;
using System.Globalization;
using System.ComponentModel;

namespace YamlDotNet.UnitTests.RepresentationModel
{
	[CLSCompliant(false)]
	public class SerializationTests : YamlTest
	{
		private class X
		{
			private bool myFlag;

			public bool MyFlag
			{
				get
				{
					return myFlag;
				}
				set
				{
					myFlag = value;
				}
			}

			private string nothing;

			public string Nothing
			{
				get
				{
					return nothing;
				}
				set
				{
					nothing = value;
				}
			}

			private int myInt = 1234;

			public int MyInt
			{
				get
				{
					return myInt;
				}
				set
				{
					myInt = value;
				}
			}

			private double myDouble = 6789.1011;

			public double MyDouble
			{
				get
				{
					return myDouble;
				}
				set
				{
					myDouble = value;
				}
			}

			private string myString = "Hello world";

			public string MyString
			{
				get
				{
					return myString;
				}
				set
				{
					myString = value;
				}
			}

			private DateTime myDate = DateTime.Now;

			public DateTime MyDate
			{
				get
				{
					return myDate;
				}
				set
				{
					myDate = value;
				}
			}

			private TimeSpan myTimeSpan = TimeSpan.FromHours(1);

			public TimeSpan MyTimeSpan
			{
				get
				{
					return myTimeSpan;
				}
				set
				{
					myTimeSpan = value;
				}
			}

			private Point myPoint = new Point(100, 200);

			public Point MyPoint
			{
				get
				{
					return myPoint;
				}
				set
				{
					myPoint = value;
				}
			}

			private int? myNullableWithValue = 8;

			public int? MyNullableWithValue
			{
				get { return myNullableWithValue; }
				set { myNullableWithValue = value; }
			}

			private int? myNullableWithoutValue = null;

			public int? MyNullableWithoutValue
			{
				get { return myNullableWithoutValue; }
				set { myNullableWithoutValue = value; }
			}
		}

		[Fact]
		public void Roundtrip()
		{
			var serializer = new Serializer();

			using (StringWriter buffer = new StringWriter())
			{
				X original = new X();
				serializer.Serialize(buffer, original, SerializationOptions.Roundtrip);

				Console.WriteLine(buffer.ToString());

				var deserializer = new YamlSerializer(typeof(X), YamlSerializerModes.Roundtrip);
				X copy = (X)deserializer.Deserialize(new StringReader(buffer.ToString()));

				foreach (var property in typeof(X).GetProperties(BindingFlags.Public | BindingFlags.Instance))
				{
					if (property.CanRead && property.CanWrite)
					{
						Assert.Equal(
							property.GetValue(original, null),
							property.GetValue(copy, null)
						);
					}
				}
			}
		}

		private class Y
		{
			private Y child;

			public Y Child
			{
				get
				{
					return child;
				}
				set
				{
					child = value;
				}
			}

			private Y child2;

			public Y Child2
			{
				get
				{
					return child2;
				}
				set
				{
					child2 = value;
				}
			}
		}


		[Fact]
		public void CircularReference()
		{
			var serializer = new Serializer();

			using (StringWriter buffer = new StringWriter())
			{
				Y original = new Y();
				original.Child = new Y
				{
					Child = original,
					Child2 = original
				};

				serializer.Serialize(buffer, original, typeof(Y), SerializationOptions.Roundtrip);

				Console.WriteLine(buffer.ToString());
			}
		}

		public class Z
		{
			public string aaa
			{
				get;
				set;
			}
		}

		[Fact]
		public void ExplicitType()
		{
			YamlSerializer serializer = new YamlSerializer();
			object result = serializer.Deserialize(YamlFile("explicitType.yaml"));
			
			Assert.True(typeof(Z).IsAssignableFrom(result.GetType()));
			Assert.Equal("bbb", ((Z)result).aaa);
		}

		[Fact]
		public void DeserializeDictionary()
		{
			YamlSerializer serializer = new YamlSerializer();
			object result = serializer.Deserialize(YamlFile("dictionary.yaml"));

			Assert.True(typeof(IDictionary<object, object>).IsAssignableFrom(result.GetType()), "The deserialized object has the wrong type.");

			IDictionary<object, object> dictionary = (IDictionary<object, object>)result;
			Assert.Equal("value1", dictionary["key1"]);
			Assert.Equal("value2", dictionary["key2"]);
		}

		[Fact]
		public void DeserializeExplicitDictionary()
		{
			YamlSerializer serializer = new YamlSerializer();
			object result = serializer.Deserialize(YamlFile("dictionaryExplicit.yaml"));

			Assert.True(typeof(IDictionary<string, int>).IsAssignableFrom(result.GetType()), "The deserialized object has the wrong type.");

			IDictionary<string, int> dictionary = (IDictionary<string, int>)result;
			Assert.Equal(1, dictionary["key1"]);
			Assert.Equal(2, dictionary["key2"]);
		}

		[Fact]
		public void DeserializeListOfDictionaries()
		{
			var serializer = new YamlSerializer<List<Dictionary<string, string>>>();
			object result = serializer.Deserialize(YamlFile("listOfDictionaries.yaml"));

			Assert.IsType<List<Dictionary<string, string>>>(result);

			var list = (List<Dictionary<string, string>>)result;
			Assert.Equal("conn1", list[0]["connection"]);
			Assert.Equal("path1", list[0]["path"]);
			Assert.Equal("conn2", list[1]["connection"]);
			Assert.Equal("path2", list[1]["path"]);
		}

		[Fact]
		public void DeserializeList() {
			YamlSerializer serializer = new YamlSerializer();
			object result = serializer.Deserialize(YamlFile("list.yaml"));

			Assert.True(typeof(IList).IsAssignableFrom(result.GetType()));

			IList list = (IList)result;
			Assert.Equal("one", list[0]);
			Assert.Equal("two", list[1]);
			Assert.Equal("three", list[2]);
		}

		[Fact]
		public void DeserializeExplicitList() {
			YamlSerializer serializer = new YamlSerializer();
			object result = serializer.Deserialize(YamlFile("listExplicit.yaml"));

			Assert.True(typeof(IList<int>).IsAssignableFrom(result.GetType()));

			IList<int> list = (IList<int>)result;
			Assert.Equal(3, list[0]);
			Assert.Equal(4, list[1]);
			Assert.Equal(5, list[2]);
		}
		
		[Fact]
		public void RoundtripList()
		{
			var serializer = new Serializer();
			var deserializer = new YamlSerializer(typeof(List<int>), YamlSerializerModes.Roundtrip);

			using (StringWriter buffer = new StringWriter())
			{
				List<int> original = new List<int>();
				original.Add(2);
				original.Add(4);
				original.Add(6);
				serializer.Serialize(buffer, original, typeof(List<int>), SerializationOptions.Roundtrip);

				Console.WriteLine(buffer.ToString());

				List<int> copy = (List<int>)deserializer.Deserialize(new StringReader(buffer.ToString()));

				Assert.Equal(original.Count, copy.Count);
				
				for(int i = 0; i < original.Count; ++i) {
					Assert.Equal(original[i], copy[i]);
				}
			}
		}

    [Fact]
    public void DeserializeArray() {
      YamlSerializer<String[]> serializer = new YamlSerializer<String[]>();
      object result = serializer.Deserialize(YamlFile("list.yaml"));
      
      Assert.True(result is String[]);
      
      String[] array = (String[])result;
      Assert.Equal("one", array[0]);
      Assert.Equal("two", array[1]);
      Assert.Equal("three", array[2]);
    }

		[Fact]
		public void Overrides()
		{
			DeserializationOptions options = new DeserializationOptions();
			options.Overrides.Add(typeof(Z), "aaa", (t, reader) => ((Z)t).aaa = reader.Expect<Scalar>().Value.ToUpperInvariant());

			YamlSerializer serializer = new YamlSerializer();
			object result = serializer.Deserialize(YamlFile("explicitType.yaml"), options);
			
			Assert.True(typeof(Z).IsAssignableFrom(result.GetType()));
			Assert.Equal("BBB", ((Z)result).aaa);
		}

		[Fact]
		public void Enums()
		{
			var serializer = new Serializer();
			YamlSerializer<StringFormatFlags> deserializer = new YamlSerializer<StringFormatFlags>();

			StringFormatFlags flags = StringFormatFlags.NoClip | StringFormatFlags.NoFontFallback;

			StringWriter buffer = new StringWriter();
			serializer.Serialize(buffer, flags);

			StringFormatFlags deserialized = deserializer.Deserialize(new StringReader(buffer.ToString()));

			Assert.Equal(flags, deserialized);
		}

		[Fact]
		public void CustomTags()
		{
			DeserializationOptions options = new DeserializationOptions();
			options.Mappings.Add("tag:yaml.org,2002:point", typeof(Point));

			YamlSerializer serializer = new YamlSerializer();
			object result = serializer.Deserialize(YamlFile("tags.yaml"), options);

			Assert.Equal(typeof(Point), result.GetType());

			Point value = (Point)result;
			Assert.Equal(10, value.X);
			Assert.Equal(20, value.Y);
		}

		//[Fact]
		//public void DeserializeConvertible()
		//{
		//    YamlSerializer<Z> serializer = new YamlSerializer<Z>();
		//    object result = serializer.Deserialize(YamlFile("convertible.yaml"));

		//    Assert.True(typeof(Z).IsAssignableFrom(result.GetType()));
		//    Assert.Equal("[hello, world]", ((Z)result).aaa, "The property has the wrong value.");
		//}

		public class Converter : TypeConverter
		{
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			{
				return sourceType == typeof(string);
			}

			public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
			{
				string[] parts = ((string)value).Split(' ');
				return new Convertible
				{
					Left = parts[0],
					Right = parts[1]
				};
			}
		}
		
		[CLSCompliant(false)]
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

			#region IConvertible Members

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

		//[Fact]
		//public void DeserializeTypeConverter()
		//{
		//    YamlSerializer<Z> serializer = new YamlSerializer<Z>();
		//    object result = serializer.Deserialize(YamlFile("converter.yaml"));

		//    Assert.True(typeof(Z).IsAssignableFrom(result.GetType()));
		//    Assert.Equal("[hello, world]", ((Z)result).aaa, "The property has the wrong value.");
		//}

		[Fact]
		public void RoundtripDictionary()
		{
			Dictionary<string, string> entries = new Dictionary<string, string>
			{
				{ "key1", "value1" },
				{ "key2", "value2" },
				{ "key3", "value3" },
			};

			var serializer = new Serializer();
			var deserializer = YamlSerializer.Create(entries, YamlSerializerModes.Roundtrip | YamlSerializerModes.DisableAliases);

			StringWriter buffer = new StringWriter();
			serializer.Serialize(buffer, entries);

			Console.WriteLine(buffer.ToString());

			var deserialized = deserializer.Deserialize(new StringReader(buffer.ToString()));

			foreach(var pair in deserialized)
			{
				Assert.Equal(entries[pair.Key], pair.Value);
			}
		}

		[Fact]
		public void SerializeAnonymousType()
		{
			var data = new { Key = 3 };

			var serializer = new Serializer();

			StringWriter buffer = new StringWriter();
			serializer.Serialize(buffer, data);

			Console.WriteLine(buffer.ToString());

			var deserializer = new YamlSerializer<Dictionary<string, string>>();
			var parsed = deserializer.Deserialize(new StringReader(buffer.ToString()));

			Assert.NotNull(parsed);
			Assert.Equal(1, parsed.Count);
		}

		[Fact]
		public void SerializationIncludesNullWhenAsked_BugFix()
		{
			var serializer = new Serializer();

			using (StringWriter buffer = new StringWriter())
			{
				var original = new { MyString = (string)null };
				serializer.Serialize(buffer, original, original.GetType(), SerializationOptions.EmitDefaults);

				Console.WriteLine(buffer.ToString());

				Assert.True(buffer.ToString().Contains("MyString"));
			}
		}

		[Fact]
		public void SerializationIncludesNullWhenAsked()
		{
			var serializer = new Serializer();

			using (StringWriter buffer = new StringWriter())
			{
				X original = new X { MyString = null };
				serializer.Serialize(buffer, original, typeof(X), SerializationOptions.EmitDefaults);

				Console.WriteLine(buffer.ToString());

				Assert.True(buffer.ToString().Contains("MyString"));
			}
		}

		[Fact]
		public void SerializationDoesNotIncludeNullWhenNotAsked()
		{
			var serializer = new Serializer();

			using (StringWriter buffer = new StringWriter())
			{
				X original = new X { MyString = null };
				serializer.Serialize(buffer, original, typeof(X), SerializationOptions.None);

				Console.WriteLine(buffer.ToString());

				Assert.False(buffer.ToString().Contains("MyString"));
			}
		}

		[Fact]
		public void SerializationOfNullWorksInJson()
		{
			var serializer = new Serializer();

			using (StringWriter buffer = new StringWriter())
			{
				X original = new X { MyString = null };
				serializer.Serialize(buffer, original, typeof(X), SerializationOptions.EmitDefaults | SerializationOptions.JsonCompatible);

				Console.WriteLine(buffer.ToString());

				Assert.True(buffer.ToString().Contains("MyString"));
			}
		}

		[Fact]
		public void DeserializationOfNullWorksInJson()
		{
			var serializer = new Serializer();
			YamlSerializer deserializer = new YamlSerializer(typeof(X), YamlSerializerModes.EmitDefaults | YamlSerializerModes.JsonCompatible | YamlSerializerModes.Roundtrip);

			using (StringWriter buffer = new StringWriter())
			{
				X original = new X { MyString = null };
				serializer.Serialize(buffer, original, typeof(X), SerializationOptions.EmitDefaults | SerializationOptions.JsonCompatible | SerializationOptions.Roundtrip);

				Console.WriteLine(buffer.ToString());

				X copy = (X)deserializer.Deserialize(new StringReader(buffer.ToString()));

				Assert.Null(copy.MyString);
			}
		}

		//[Fact]
		//public void DeserializationIgnoresUnknownProperties()
		//{
		//	var serializer = new YamlSerializer(typeof(X));
		//}

		class ContainsIgnore {
			[YamlIgnore]
			public String IgnoreMe { get; set; }
		}

		[Fact]
		public void SerializationRespectsYamlIgnoreAttribute()
		{
			var serializer = new Serializer();
			var deserializer = new YamlSerializer<ContainsIgnore>(YamlSerializerModes.EmitDefaults | YamlSerializerModes.JsonCompatible | YamlSerializerModes.Roundtrip);
      
			using (StringWriter buffer = new StringWriter())
			{
				var orig = new ContainsIgnore { IgnoreMe = "Some Text" };
				serializer.Serialize(buffer, orig);
				Console.WriteLine(buffer.ToString());
				var copy = deserializer.Deserialize(new StringReader(buffer.ToString()));
				Assert.Null(copy.IgnoreMe);
			}
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

		private T SerializeThenDeserialize<T>(T input)
		{
			Serializer serializer = new Serializer();
			TextWriter writer = new StringWriter();
			serializer.Serialize(writer, input, typeof(T));

			string serialized = writer.ToString();
			Console.WriteLine("serialized =\n-----\n{0}", serialized);

			return YamlSerializer.Create(input).Deserialize(new StringReader(serialized));
		}

		private class ConventionTest
		{
			public string FirstTest { get; set; }
			public string SecondTest { get; set; }
			public string ThirdTest { get; set; }
			[YamlAlias("fourthTest")]
			public string AliasTest { get; set; }
		}

		[Fact]
		public void DeserializeUsingConventions()
		{
			var serializer = new YamlSerializer<ConventionTest>();
			var result = serializer.Deserialize(YamlFile("namingConvention.yaml"));

			Assert.Equal("First", result.FirstTest);
			Assert.Equal("Second", result.SecondTest);
			Assert.Equal("Third", result.ThirdTest);
			Assert.Equal("Fourth", result.AliasTest);
		}

		[Fact]
		public void RoundtripAlias()
		{
			var input = new ConventionTest { AliasTest = "Fourth" };
			var serializer = new Serializer();
			var writer = new StringWriter();
			serializer.Serialize(writer, input, input.GetType());
			string serialized = writer.ToString();

			// Ensure serialisation is correct
			Assert.Equal("fourthTest: Fourth", serialized);

			var deserializer = new YamlSerializer<ConventionTest>();
			var output = deserializer.Deserialize(new StringReader(serialized));

			// Ensure round-trip retains value
			Assert.Equal(input.AliasTest, output.AliasTest);
		}
	}
}
