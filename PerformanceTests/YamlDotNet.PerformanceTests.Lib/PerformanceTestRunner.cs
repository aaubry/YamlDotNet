using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace YamlDotNet.PerformanceTests.Lib
{
	public class PerformanceTestRunner
	{
		private const int _iterations = 1000;

		public void Run(ISerializerAdapter serializer)
		{
			var adapterName = serializer.GetType().Namespace.Split('.').Last();

			var tests = typeof(ISerializationTest).Assembly
            	.GetTypes()
				.Where(t => t.IsClass && typeof(ISerializationTest).IsAssignableFrom(t))
				.Select(t => (ISerializationTest)Activator.CreateInstance(t));

			foreach(var test in tests)
			{
				Console.Write("{0}\t{1}\t", adapterName, test.GetType().Name);

				var graph = test.Graph;

				// Warmup
				RunTest(serializer, graph);

				if(!Stopwatch.IsHighResolution)
				{
					Console.Error.WriteLine("Stopwatch is not high resolution!");
				}

				var timer = Stopwatch.StartNew();
				for(var i = 0; i < _iterations; ++i)
				{
					RunTest(serializer, graph);
				}
				var duration = timer.Elapsed;
				Console.WriteLine("{0}", duration.TotalMilliseconds / _iterations);
			}
		}

		private void RunTest(ISerializerAdapter serializer, object graph)
		{
			var buffer = new StringWriter();
			serializer.Serialize(buffer, graph);
		}
	}
}

