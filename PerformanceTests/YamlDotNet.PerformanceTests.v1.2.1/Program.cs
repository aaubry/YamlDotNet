using System;
using YamlDotNet.PerformanceTests.Lib;
using YamlDotNet.RepresentationModel.Serialization;
using System.IO;

namespace YamlDotNet.PerformanceTests.v1_2_1
{
	public class Program : ISerializerAdapter
	{
		public static void Main()
		{
			var runner = new PerformanceTestRunner();
			runner.Run(new Program());
		}

		private readonly Serializer _serializer = new Serializer();

		public void Serialize (TextWriter writer, object graph)
		{
			_serializer.Serialize (writer, graph);
		}
	}
}
