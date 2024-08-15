// This file is part of YamlDotNet - A .NET library for YAML.
// Copyright (c) Antoine Aubry and contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using YamlDotNet.Serialization;

namespace YamlDotNet.Benchmark;

[MemoryDiagnoser]
[MediumRunJob(RuntimeMoniker.Net80)]
[MediumRunJob(RuntimeMoniker.Net47)]
public class SerializationBenchmarks
{
    public class SampleRecord
    {
        public SampleRecord(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; private set; }
        public string Description { get; private set; }
    }

    private readonly IReadOnlyCollection<SampleRecord> configs = Enumerable.Range(0, 10_000).Select(i => new SampleRecord("MyName", "MyDescription")).ToList();
    private readonly ISerializer serializer = new SerializerBuilder().DisableAliases().Build();

    [Benchmark]
    public string Serializer()
    {
        return serializer.Serialize(configs);
    }
}
