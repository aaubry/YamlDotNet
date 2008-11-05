using System;
using System.Drawing;
using NUnit.Framework;
using System.IO;
using YamlDotNet.RepresentationModel.Serialization;
using System.Reflection;
using System.Collections.Generic;

namespace YamlDotNet.UnitTests.RepresentationModel
{
	[TestFixture]
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
			YamlSerializer serializer = new YamlSerializer(typeof(X), YamlSerializerOptions.Roundtrip);

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
			YamlSerializer serializer = new YamlSerializer(typeof(Y), YamlSerializerOptions.Roundtrip);

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
			
			Assert.IsInstanceOfType(typeof(Z), result, "The deserializer should have used the correct type.");
			Assert.AreEqual("bbb", ((Z)result).aaa, "The property has the wrong value.");
		}

		[Test]
		public void DeserializeDictionary()
		{
			YamlSerializer serializer = new YamlSerializer();
			object result = serializer.Deserialize(YamlFile("dictionary.yaml"));

			Assert.IsInstanceOfType(typeof(IDictionary<object, object>), result, "The deserialized object has the wrong type.");

			IDictionary<object, object> dictionary = (IDictionary<object, object>)result;
			Assert.AreEqual("value1", dictionary["key1"]);
			Assert.AreEqual("value2", dictionary["key2"]);
		}

		[Test]
		public void DeserializeExplicitDictionary()
		{
			YamlSerializer serializer = new YamlSerializer();
			object result = serializer.Deserialize(YamlFile("dictionaryExplicit.yaml"));

			Assert.IsInstanceOfType(typeof(IDictionary<string, int>), result, "The deserialized object has the wrong type.");

			IDictionary<string, int> dictionary = (IDictionary<string, int>)result;
			Assert.AreEqual(1, dictionary["key1"]);
			Assert.AreEqual(2, dictionary["key2"]);
		}
	}
}