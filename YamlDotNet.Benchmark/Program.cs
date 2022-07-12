using BenchmarkDotNet.Running;
using YamlDotNet.Benchmark;
using YamlDotNet.Serialization;

var serializer = new DeserializerBuilder().Build();
var o = serializer.Deserialize(new StringReader("?y: x"));

BenchmarkSwitcher.FromAssembly(typeof(YamlStreamBenchmark).Assembly).Run(args);
