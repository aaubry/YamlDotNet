//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2013 Antoine Aubry and contributors
    
//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:
    
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
    
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace YamlDotNet.PerformanceTests.Lib
{
	public class PerformanceTestRunner
	{
		private const int _defaultIterations = 10000;

		public void Run(ISerializerAdapter serializer, string[] args)
		{
			var iterations = args.Length > 0
				? int.Parse(args[0])
				: _defaultIterations;

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
				for (var i = 0; i < iterations; ++i)
				{
					RunTest(serializer, graph);
				}
				var duration = timer.Elapsed;
				Console.WriteLine("{0}", duration.TotalMilliseconds / iterations);
			}
		}

		private void RunTest(ISerializerAdapter serializer, object graph)
		{
			var buffer = new StringWriter();
			serializer.Serialize(buffer, graph);
		}
	}
}

