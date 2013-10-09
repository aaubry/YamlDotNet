using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Xunit;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test.Serialization
{
	public class SerializationTests2
	{
		public enum MyEnum
		{
			A,
			B,
		}

		public class MyObject
		{
			public MyObject()
			{
				ArrayContent = new int[2];
			}

			public string String { get; set; }

			public sbyte SByte { get; set; }

			public byte Byte { get; set; }

			public short Int16 { get; set; }

			public ushort UInt16 { get; set; }

			public int Int32 { get; set; }

			public uint UInt32 { get; set; }

			public long Int64 { get; set; }

			public ulong UInt64 { get; set; }

			public float Float { get; set; }

			public double Double { get; set; }

			public MyEnum Enum { get; set; }

			public bool Bool { get; set; }

			public bool BoolFalse { get; set; }

			public string A0Anchor { get; set; }

			public string A1Alias { get; set; }

			public int[] Array { get; set; }

			public int[] ArrayContent { get; private set; }
		}

		[Fact]
		public void TestSimpleObjectAndPrimitive()
		{
			var text = @"!MyObject
A0Anchor: &o1 Test
A1Alias: *o1
Array: [1, 2, 3]
ArrayContent: [1, 2]
Bool: true
BoolFalse: false
Byte: 2
Double: 6.6
Enum: B
Float: 5.5
Int16: 3
Int32: 5
Int64: 7
SByte: 1
String: This is a test
UInt16: 4
UInt32: 6
UInt64: 8
".Trim();

			var settings = new SerializerSettings();
			settings.RegisterTagMapping("MyObject", typeof (MyObject));
			SerialRoundTrip(settings, text);
		}

		public class MyObjectAndCollection
		{
			public MyObjectAndCollection()
			{
				Values = new List<string>();
			}

			public string Name { get; set; }

			public List<string> Values { get; set; }
		}


		/// <summary>
		/// Tests the serialization of an object that contains a property with list
		/// </summary>
		[Fact]
		public void TestObjectWithCollection()
		{
			var text = @"!MyObjectAndCollection
Name: Yes
Values: [a, b, c]
".Trim();

			var settings = new SerializerSettings();
			settings.RegisterTagMapping("MyObjectAndCollection", typeof (MyObjectAndCollection));
			SerialRoundTrip(settings, text);
		}

		public class MyCustomCollectionWithProperties : List<string>
		{
			public string Name { get; set; }

			public int Value { get; set; }
		}

		/// <summary>
		/// Tests the serialization of a custom collection with some custom properties.
		/// In this specific case, the collection cannot be serialized as a simple list
		/// so the serializer is serializing the list as a YAML mapping, using the mapping
		/// to store the usual propertis and using the special member '~Items' to serialzie 
		/// the real content of the list
		/// </summary>
		[Fact]
		public void TestCustomCollectionWithProperties()
		{
			var text = @"!MyCustomCollectionWithProperties
Name: Yes
Value: 1
~Items:
  - a
  - b
  - c
".Trim();

			var settings = new SerializerSettings() {LimitFlowSequence = 0};
			settings.RegisterTagMapping("MyCustomCollectionWithProperties", typeof (MyCustomCollectionWithProperties));
			SerialRoundTrip(settings, text);
		}


		public class MyCustomDictionaryWithProperties : Dictionary<string, bool>
		{
			public string Name { get; set; }

			public int Value { get; set; }
		}


		/// <summary>
		/// Tests the serialization of a custom dictionary with some custom properties.
		/// In this specific case, the dictionary cannot be serialized as a simple mapping
		/// so the serializer is serializing the dictionary as a YAML !!map, using the mapping
		/// to store the usual propertis and using the special member '~Items' to serialize 
		/// the real content of the dictionary as a sub YAML !!map
		/// </summary>
		[Fact]
		public void TestCustomDictionaryWithProperties()
		{
			var text = @"!MyCustomDictionaryWithProperties
Name: Yes
Value: 1
~Items:
  a: true
  b: false
  c: true
".Trim();

			var settings = new SerializerSettings() {LimitFlowSequence = 0};
			settings.RegisterTagMapping("MyCustomDictionaryWithProperties", typeof (MyCustomDictionaryWithProperties));
			SerialRoundTrip(settings, text);
		}

		public class MyCustomClassWithSpecialMembers
		{
			public MyCustomClassWithSpecialMembers()
			{
				StringListByContent = new List<string>();
				StringMapbyContent = new Dictionary<string, object>();
				ListByContent = new List<string>();
			}

			public string Name { get; set; }

			public int Value { get; set; }

			/// <summary>
			/// Gets or sets the basic list.
			/// </summary>
			/// <value>The basic list.</value>
			public IList BasicList { get; set; }

			/// <summary>
			/// For this property, the deserializer is instantiating
			/// automatically a default List&lt;string&gtl instance.
			/// </summary>
			public IList<string> StringList { get; set; }

			/// <summary>
			/// For this property, the deserializer is using the actual
			/// value of the list stored in this instance instead of 
			/// creating a new List&lt;T&gtl instance.
			/// </summary>
			public List<string> StringListByContent { get; private set; }

			/// <summary>
			/// Gets or sets the basic map.
			/// </summary>
			/// <value>The basic map.</value>
			public IDictionary BasicMap { get; set; }

			/// <summary>
			/// Idem as for <see cref="StringList"/> but for dictionary.
			/// </summary>
			/// <value>The string map.</value>
			public IDictionary<string, object> StringMap { get; set; }


			/// <summary>
			/// Idem as for <see cref="StringListByContent"/> but for dictionary.
			/// </summary>
			/// <value>The content of the string mapby.</value>
			public Dictionary<string, object> StringMapbyContent { get; private set; }

			/// <summary>
			/// For this property, the deserializer is using the actual
			/// value of the list stored in this instance instead of 
			/// creating a new List&lt;T&gtl instance.
			/// </summary>
			/// <value>The content of the list by.</value>
			public IList ListByContent { get; private set; }
		}

		/// <summary>
		/// Tests the serialization of a custom dictionary with some custom properties.
		/// In this specific case, the dictionary cannot be serialized as a simple mapping
		/// so the serializer is serializing the dictionary as a YAML !!map, using the mapping
		/// to store the usual propertis and using the special member '~Items' to serialize 
		/// the real content of the dictionary as a sub YAML !!map
		/// </summary>
		[Fact]
		public void TestMyCustomClassWithSpecialMembers()
		{
			var text = @"!MyCustomClassWithSpecialMembers
BasicList:
  - 1
  - 2
BasicMap:
  a: 1
  b: 2
ListByContent:
  - a
  - b
Name: Yes
StringList:
  - 1
  - 2
StringListByContent:
  - 3
  - 4
StringMap:
  c: yes
  d: 3
StringMapbyContent:
  e: 4
  f: no
Value: 0
".Trim();

			var settings = new SerializerSettings() {LimitFlowSequence = 0};
			settings.RegisterTagMapping("MyCustomClassWithSpecialMembers", typeof (MyCustomClassWithSpecialMembers));
			SerialRoundTrip(settings, text);
		}


		/// <summary>
		/// Tests the serialization of ordered members.
		/// </summary>
		public class ClassMemberOrder
		{
			public ClassMemberOrder()
			{
				First = 1;
				Second = 2;
				BeforeName = 3;
				Name = 4;
				NameAfter = 5;
			}

			/// <summary>
			/// Sets an explicit order
			/// </summary>
			[YamlMember(1)]
			public int Second { get; set; }

			/// <summary>
			/// Sets an explicit order
			/// </summary>
			[YamlMember(0)]
			public int First { get; set; }

			/// <summary>
			/// This property will be sorted after 
			/// the explicit order by alphabetical order
			/// </summary>
			/// <value>The name after.</value>
			public int NameAfter { get; set; }

			/// <summary>
			/// This property will be sorted after 
			/// the explicit order by alphabetical order
			/// </summary>
			public int Name { get; set; }

			/// <summary>
			/// Sets an explicit order
			/// </summary>
			[YamlMember(2)]
			public int BeforeName { get; set; }
		}

		/// <summary>
		/// Tests the serialization of ordered members in the class <see cref="ClassMemberOrder"/>.
		/// </summary>
		[Fact]
		public void TestClassMemberOrder()
		{
			var settings = new SerializerSettings() { LimitFlowSequence = 0 };
			settings.RegisterTagMapping("ClassMemberOrder", typeof(ClassMemberOrder));
			SerialRoundTrip(settings, new ClassMemberOrder());
		}

		private void SerialRoundTrip(SerializerSettings settings, string text)
		{
			var serializer = new Serializer(settings);
			// not working yet, scalar read/write are not yet implemented
			Console.WriteLine("Text to serialize:");
			Console.WriteLine("------------------");
			Console.WriteLine(text);
			var value = serializer.Deserialize(text);

			Console.WriteLine();

			var stringWriter = new StringWriter();
			serializer.Serialize(stringWriter, value);
			var text2 = stringWriter.ToString().Trim();
			Console.WriteLine("Text deserialized:");
			Console.WriteLine("------------------");
			Console.WriteLine(text2);

			Assert.Equal(text, text2);
		}

		private void SerialRoundTrip(SerializerSettings settings, object value)
		{
			var serializer = new Serializer(settings);

			var text = serializer.Serialize(value);
			Console.WriteLine("Text serialize:");
			Console.WriteLine("------------------");
			Console.WriteLine(text);

			// not working yet, scalar read/write are not yet implemented
			Console.WriteLine("Text to deserialize/serialize:");
			Console.WriteLine("------------------");
			var text2 = serializer.Serialize(serializer.Deserialize(text));
			Console.WriteLine(text2);

			Assert.Equal(text, text2);
		}

	}
}