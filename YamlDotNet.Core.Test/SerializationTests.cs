using System;
using System.IO;
using Xunit;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test
{
	public class SerializationTests
	{
		public class MyObject
		{
			public string Name { get; set; }

			public int Value { get; set; }
		}


		[Fact]
		public void TestDeserializationSimple()
		{
			var settings = new YamlSerializerSettings();
			settings.TagTypes.AddTagAlias("MyObject", typeof(MyObject));

			var serializer = new YamlSerializer(settings);

			var text = @"!MyObject
Name: This is a test
Value: 1
";
			// not working yet, scalar read/write are not yet implemented
			var value = serializer.Deserialize(text);
			Console.WriteLine(value);
		}
	}
}