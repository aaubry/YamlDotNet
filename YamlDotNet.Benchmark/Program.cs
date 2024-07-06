using System.Diagnostics;
using YamlDotNet.Benchmark;

//BenchmarkSwitcher.FromAssembly(typeof(YamlStreamBenchmark).Assembly).Run(args);
var x = new BigFileBenchmark();
x.Setup();
Console.WriteLine("Warming up with a single run");
x.LoadLarge();
Console.WriteLine("Done, executing attempts");
while (true)
{
    var stop = new Stopwatch();
    stop.Start();
    x.LoadLarge();
    Console.WriteLine($"{DateTime.Now} - Loaded - Elapsed: ${stop.Elapsed}");
}
