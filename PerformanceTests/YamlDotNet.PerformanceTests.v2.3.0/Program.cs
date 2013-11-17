using System;
using System.IO;
using YamlDotNet.RepresentationModel.Serialization;
using YamlDotNet.PerformanceTests.Lib;

namespace YamlDotNet.PerformanceTests.v2_3_0
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