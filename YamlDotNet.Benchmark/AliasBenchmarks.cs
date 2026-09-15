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

using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using YamlDotNet.Serialization;

namespace YamlDotNet.Benchmark;

// Exercises the anchor/alias path, which the rest of the suite does not touch. The graph holds
// many references to a small set of shared objects, so serialization (with aliases enabled, the
// default) emits anchors + aliases, and deserialization resolves them via the alias value
// deserializer. This isolates the anchor-assignment, anchor-name and alias-resolution hot paths.
public class AliasBenchmarks
{
    private const int ReferenceCount = 5000;

    private readonly ISerializer serializer = new SerializerBuilder().Build();
    private readonly IDeserializer deserializer = new DeserializerBuilder().Build();

    private List<Address> graph = null!;
    private string yaml = "";

    [GlobalSetup]
    public void Setup()
    {
        var shared = new[]
        {
            new Address { Street = "1 Shared Way", City = "Common", State = "CA", Zip = "90001", Country = "US" },
            new Address { Street = "2 Shared Way", City = "Common", State = "CA", Zip = "90002", Country = "US" },
            new Address { Street = "3 Shared Way", City = "Common", State = "CA", Zip = "90003", Country = "US" },
        };

        graph = new List<Address>(ReferenceCount);
        for (var i = 0; i < ReferenceCount; i++)
        {
            graph.Add(shared[i % shared.Length]);
        }

        yaml = serializer.Serialize(graph);
    }

    [Benchmark]
    public string SerializeWithAliases() => serializer.Serialize(graph);

    [Benchmark]
    public List<Address> DeserializeWithAliases() => deserializer.Deserialize<List<Address>>(yaml);
}
