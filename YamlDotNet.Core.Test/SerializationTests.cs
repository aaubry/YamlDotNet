using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test
{
	public class SerializationTests
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
			settings.TagTypes.AddTagMapping("MyObject", typeof(MyObject));
			SerialRoundTrip(settings, text);
		}

		public class MyObjectAndCollection
		{
			public MyObjectAndCollection()
			{
				Values = new List<string>();
			}

			public string Name { get; set; }

			public List<string>  Values { get; set; }
		}


		[Fact]
		public void TestObjectWithCollection()
		{
			var text = @"!MyObjectAndCollection
Name: Yes
Values: [a, b, c]
".Trim();

			var settings = new SerializerSettings();
			settings.TagTypes.AddTagMapping("MyObjectAndCollection", typeof(MyObjectAndCollection));
			SerialRoundTrip(settings, text);
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
	}
}