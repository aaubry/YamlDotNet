using System;
using YamlDotNet.Core;
using System.IO;
using YamlDotNet.UnitTests.RepresentationModel;

namespace YamlDotNet.UnitTests
{
	public class Runner
	{
		public static void Main()
		{
			new SerializationTests().DeserializeList();
		}
	}
}