using System;
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
		}


		[Fact]
		public void TestSimpleObjectAndPrimitive()
		{
			var settings = new YamlSerializerSettings();
			settings.TagTypes.AddTagAlias("MyObject", typeof(MyObject));

			var serializer = new YamlSerializer(settings);

			var text = @"!MyObject
Bool: true
BoolFalse: false
Byte: 2
Enum: A
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
";
			// not working yet, scalar read/write are not yet implemented
			var value = serializer.Deserialize(text);
			Console.WriteLine(value);
		}
	}
}