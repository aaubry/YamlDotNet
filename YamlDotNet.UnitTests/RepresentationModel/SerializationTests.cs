using System;
using System.Drawing;
using NUnit.Framework;
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
	[TestFixture]
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

		}

		[Test]
		public void Roundtrip()
		{
			YamlSerializer serializer = new YamlSerializer(typeof(X), YamlSerializerModes.Roundtrip);

			using (StringWriter buffer = new StringWriter())
			{
				X original = new X();
				serializer.Serialize(buffer, original);

				Console.WriteLine(buffer.ToString());

				X copy = (X)serializer.Deserialize(new StringReader(buffer.ToString()));

				foreach (var property in typeof(X).GetProperties(BindingFlags.Public | BindingFlags.Instance))
				{
					if (property.CanRead && property.CanWrite)
					{
						Assert.AreEqual(
							property.GetValue(original, null),
							property.GetValue(copy, null),
							string.Format("Property '{0}' is incorrect", property.Name)
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


		[Test]
		public void CircularReference()
		{
			YamlSerializer serializer = new YamlSerializer(typeof(Y), YamlSerializerModes.Roundtrip);

			using (StringWriter buffer = new StringWriter())
			{
				Y original = new Y();
				original.Child = new Y
             	{
             		Child = original,
             		Child2 = original
             	};

				serializer.Serialize(buffer, original);

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

		[Test]
		public void ExplicitType()
		{
			YamlSerializer serializer = new YamlSerializer();
			object result = serializer.Deserialize(YamlFile("explicitType.yaml"));
			
			Assert.IsTrue(typeof(Z).IsAssignableFrom(result.GetType()), "The deserializer should have used the correct type.");
			Assert.AreEqual("bbb", ((Z)result).aaa, "The property has the wrong value.");
		}

		[Test]
		public void DeserializeDictionary()
		{
			YamlSerializer serializer = new YamlSerializer();
			object result = serializer.Deserialize(YamlFile("dictionary.yaml"));

			Assert.IsTrue(typeof(IDictionary<object, object>).IsAssignableFrom(result.GetType()), "The deserialized object has the wrong type.");

			IDictionary<object, object> dictionary = (IDictionary<object, object>)result;
			Assert.AreEqual("value1", dictionary["key1"]);
			Assert.AreEqual("value2", dictionary["key2"]);
		}

		[Test]
		public void DeserializeExplicitDictionary()
		{
			YamlSerializer serializer = new YamlSerializer();
			object result = serializer.Deserialize(YamlFile("dictionaryExplicit.yaml"));

			Assert.IsTrue(typeof(IDictionary<string, int>).IsAssignableFrom(result.GetType()), "The deserialized object has the wrong type.");

			IDictionary<string, int> dictionary = (IDictionary<string, int>)result;
			Assert.AreEqual(1, dictionary["key1"]);
			Assert.AreEqual(2, dictionary["key2"]);
		}

		[Test]
		public void DeserializeListOfDictionaries()
		{
			var serializer = new YamlSerializer<List<Dictionary<string, string>>>();
			object result = serializer.Deserialize(YamlFile("listOfDictionaries.yaml"));

			Assert.IsInstanceOfType(typeof(List<Dictionary<string, string>>), result, "The deserialized object has the wrong type.");

			var list = (List<Dictionary<string, string>>)result;
			Assert.AreEqual("conn1", list[0]["connection"]);
			Assert.AreEqual("path1", list[0]["path"]);
			Assert.AreEqual("conn2", list[1]["connection"]);
			Assert.AreEqual("path2", list[1]["path"]);
		}

		[Test]
		public void DeserializeList() {
			YamlSerializer serializer = new YamlSerializer();
			object result = serializer.Deserialize(YamlFile("list.yaml"));

			Assert.IsTrue(typeof(IList).IsAssignableFrom(result.GetType()), "The deserialized object has the wrong type.");

			IList list = (IList)result;
			Assert.AreEqual("one", list[0]);
			Assert.AreEqual("two", list[1]);
			Assert.AreEqual("three", list[2]);
		}

		[Test]
		public void DeserializeExplicitList() {
			YamlSerializer serializer = new YamlSerializer();
			object result = serializer.Deserialize(YamlFile("listExplicit.yaml"));

			Assert.IsTrue(typeof(IList<int>).IsAssignableFrom(result.GetType()), "The deserialized object has the wrong type.");

			IList<int> list = (IList<int>)result;
			Assert.AreEqual(3, list[0]);
			Assert.AreEqual(4, list[1]);
			Assert.AreEqual(5, list[2]);
		}
		
		[Test]
		public void RoundtripList()
		{
			YamlSerializer serializer = new YamlSerializer(typeof(List<int>), YamlSerializerModes.Roundtrip);

			using (StringWriter buffer = new StringWriter())
			{
				List<int> original = new List<int>();
				original.Add(2);
				original.Add(4);
				original.Add(6);
				serializer.Serialize(buffer, original);

				Console.WriteLine(buffer.ToString());

				List<int> copy = (List<int>)serializer.Deserialize(new StringReader(buffer.ToString()));

				Assert.AreEqual(original.Count, copy.Count, "The lists do not have the same number of items.");
				
				for(int i = 0; i < original.Count; ++i) {
					Assert.AreEqual(original[i], copy[i]);
				}
			}
		}

		[Test]
		public void Overrides()
		{
			DeserializationOptions options = new DeserializationOptions();
			options.Overrides.Add(typeof(Z), "aaa", (t, reader) => ((Z)t).aaa = reader.Expect<Scalar>().Value.ToUpperInvariant());

			YamlSerializer serializer = new YamlSerializer();
			object result = serializer.Deserialize(YamlFile("explicitType.yaml"), options);
			
			Assert.IsTrue(typeof(Z).IsAssignableFrom(result.GetType()), "The deserializer should have used the correct type.");
			Assert.AreEqual("BBB", ((Z)result).aaa, "The property has the wrong value.");
		}

		[Test]
		public void Enums()
		{
			YamlSerializer<StringFormatFlags> serializer = new YamlSerializer<StringFormatFlags>();

			StringFormatFlags flags = StringFormatFlags.NoClip | StringFormatFlags.NoFontFallback;

			StringWriter buffer = new StringWriter();
			serializer.Serialize(buffer, flags);

			StringFormatFlags deserialized = serializer.Deserialize(new StringReader(buffer.ToString()));

			Assert.AreEqual(flags, deserialized, "The value is incorrect.");
		}

		[Test]
		public void CustomTags()
		{
			DeserializationOptions options = new DeserializationOptions();
			options.Mappings.Add("tag:yaml.org,2002:point", typeof(Point));

			YamlSerializer serializer = new YamlSerializer();
			object result = serializer.Deserialize(YamlFile("tags.yaml"), options);

			Assert.AreEqual(typeof(Point), result.GetType(), "The deserializer should have used the correct type.");

			Point value = (Point)result;
			Assert.AreEqual(10, value.X, "The property X has the wrong value.");
			Assert.AreEqual(20, value.Y, "The property Y has the wrong value.");
		}

		//[Test]
		//public void DeserializeConvertible()
		//{
		//    YamlSerializer<Z> serializer = new YamlSerializer<Z>();
		//    object result = serializer.Deserialize(YamlFile("convertible.yaml"));

		//    Assert.IsTrue(typeof(Z).IsAssignableFrom(result.GetType()), "The deserializer should have used the correct type.");
		//    Assert.AreEqual("[hello, world]", ((Z)result).aaa, "The property has the wrong value.");
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
				Assert.AreEqual(CultureInfo.InvariantCulture, provider);

				return string.Format(provider, "[{0}, {1}]", Left, Right);
			}

			public object ToType(Type conversionType, IFormatProvider provider)
			{
				Assert.AreEqual(typeof(string), conversionType);
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

		//[Test]
		//public void DeserializeTypeConverter()
		//{
		//    YamlSerializer<Z> serializer = new YamlSerializer<Z>();
		//    object result = serializer.Deserialize(YamlFile("converter.yaml"));

		//    Assert.IsTrue(typeof(Z).IsAssignableFrom(result.GetType()), "The deserializer should have used the correct type.");
		//    Assert.AreEqual("[hello, world]", ((Z)result).aaa, "The property has the wrong value.");
		//}

		[Test]
		public void RoundtripDictionary()
		{
			Dictionary<string, string> entries = new Dictionary<string, string>
			{
				{ "key1", "value1" },
				{ "key2", "value2" },
				{ "key3", "value3" },
			};

			var serializer = YamlSerializer.Create(entries, YamlSerializerModes.Roundtrip | YamlSerializerModes.DisableAliases);

			StringWriter buffer = new StringWriter();
			serializer.Serialize(buffer, entries);

			Console.WriteLine(buffer.ToString());

			var deserialized = serializer.Deserialize(new StringReader(buffer.ToString()));

			foreach(var pair in deserialized)
			{
				Assert.AreEqual(entries[pair.Key], pair.Value);
			}
		}
	}
}
