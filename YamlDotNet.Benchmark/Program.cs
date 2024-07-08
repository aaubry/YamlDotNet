using System.Diagnostics;
using YamlDotNet.Benchmark;
using YamlDotNet.Serialization;

var serializer = new SerializerBuilder().JsonCompatible().Build();
var v = new { nan = float.NaN, inf = float.NegativeInfinity, posinf = float.PositiveInfinity, max = float.MaxValue, min = float.MinValue, good = .1234f, good1 = 1, good2 = -.1234, good3= -1 };
var yaml = serializer.Serialize(v);
Console.WriteLine(yaml);
