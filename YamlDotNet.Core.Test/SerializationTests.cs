using System;
using System.Collections.Generic;
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

            public string Anchor { get; set; }

            public string Alias { get; set; }

            public int[] Array { get; set; }

			public int[] ArrayContent { get; private set; }
		}


		[Fact]
		public void TestSimpleObjectAndPrimitive()
		{
			var settings = new SerializerSettings();

			var name = typeof (MyObject).FullName;

			//var uri = Uri.EscapeUriString("!" + name);

			settings.TagTypes.AddTagMapping("MyObject", typeof(MyObject));

			var serializer = new Serializer(settings);

			var text = @"!MyObject
Anchor: &o1 Test
Alias: *o1
Bool: true
BoolFalse: false
Byte: 2
Enum: B
Double: 6.6
Float: 5.5
Int16: 3
Int32: 5
Int64: 7
SByte: 1
String: This is a test
UInt16: 4
UInt32: 6
UInt64: 8
Array: [1,2,3]
ArrayContent: [1,2]
";
			// not working yet, scalar read/write are not yet implemented
			var value = serializer.Deserialize(text);
			Console.WriteLine(value);
		}
	}
}