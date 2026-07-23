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

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using YamlDotNet.Benchmark;

// Profiling driver mode (for a CPU sampling profiler such as ultra) — a steady-state loop that
// bypasses the BenchmarkDotNet harness. See ProfilingDriver for usage.
if (args.Length > 0 && args[0].Equals("profile", StringComparison.OrdinalIgnoreCase))
{
    ProfilingDriver.Run(args);
    return;
}

// Shared config for every benchmark: add MemoryDiagnoser for allocation tracking. DefaultConfig
// already emits a GitHub-flavoured markdown export, so before/after tables can be pasted straight
// into a PR. Run on a quiet machine so the numbers stay clean:
//   dotnet run -c Release --project YamlDotNet.Benchmark -f net10.0 -- --filter '*'
var config = DefaultConfig.Instance
    .AddDiagnoser(MemoryDiagnoser.Default);

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
