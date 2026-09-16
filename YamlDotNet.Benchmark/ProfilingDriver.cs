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

using System;
using System.Diagnostics;
using YamlDotNet.Serialization;

namespace YamlDotNet.Benchmark;

// Steady-state CPU-profiling driver, kept out of BenchmarkDotNet so a sampling profiler (ultra)
// sees only YamlDotNet work rather than the BDN harness. It reuses a cached serializer /
// deserializer and a single pre-serialized document (mirroring real reuse), then loops the unit of
// work for a fixed wall-clock window so the profile settles into steady state. The default "both"
// mode is naturally deserialize-weighted because the read path dominates the per-iteration cost.
//
// Capture (elevated shell; profile the built exe directly, not `dotnet run`):
//   ultra profile -o baseline --delay 3 -- YamlDotNet.Benchmark.exe profile [both|deser|ser] [seconds]
internal static class ProfilingDriver
{
    public static void Run(string[] args)
    {
        var mode = args.Length > 1 ? args[1].ToLowerInvariant() : "both";
        var seconds = (args.Length > 2 && int.TryParse(args[2], out var s)) ? s : 25;

        var serializer = new SerializerBuilder().Build();
        var deserializer = new DeserializerBuilder().Build();

        var company = SampleModel.CreateCompany(2000);
        var yaml = serializer.Serialize(company);

        // Validate the workload up front so a broken run fails fast (ultra runs the target silently).
        var check = deserializer.Deserialize<Company>(yaml);
        if (check.Departments.Count == 0 || check.Departments[0].Employees.Count == 0)
        {
            throw new InvalidOperationException("Profiling workload produced an empty graph.");
        }

        var stopwatch = Stopwatch.StartNew();
        long iterations = 0;
        long checksum = 0;

        while (stopwatch.Elapsed.TotalSeconds < seconds)
        {
            if (mode != "ser")
            {
                var round = deserializer.Deserialize<Company>(yaml);
                checksum += round.Departments.Count;
            }

            if (mode != "deser")
            {
                var text = serializer.Serialize(company);
                checksum += text.Length;
            }

            iterations++;
        }

        // Guard against dead-code elimination of the loop body.
        if (checksum < 0)
        {
            throw new InvalidOperationException("unreachable");
        }

        Console.Error.WriteLine(
            $"Profiling driver finished: mode={mode}, iterations={iterations}, elapsed={stopwatch.Elapsed.TotalSeconds:F1}s");
    }
}
