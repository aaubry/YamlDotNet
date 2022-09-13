using BenchmarkDotNet.Running;
using YamlDotNet.Benchmark;

BenchmarkSwitcher.FromAssembly(typeof(YamlStreamBenchmark).Assembly).Run(args);
