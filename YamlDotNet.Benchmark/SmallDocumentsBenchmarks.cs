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
using System.IO;
using BenchmarkDotNet.Attributes;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace YamlDotNet.Benchmark;

// Parses many small, independent documents with a new parser for each one, the way a test-suite
// or front-matter loader does (test262, for example, has ~50k front-matter blocks of a few hundred
// characters each). At this size the cost of setting up a scanner and parser, and the per-token
// and per-character overheads, are not amortized over a large input the way they are in the other
// benchmarks. Results are reported per document.
public class SmallDocumentsBenchmarks
{
    private const int DocumentCount = 1000;

    private readonly IDeserializer deserializer = new DeserializerBuilder().Build();

    private string[] documents = [];

    [GlobalSetup]
    public void Setup()
    {
        documents = new string[DocumentCount];
        for (var i = 0; i < DocumentCount; i++)
        {
            documents[i] = CreateDocument(i);
        }
    }

    [Benchmark(OperationsPerInvoke = DocumentCount)]
    public int Parse()
    {
        var events = 0;
        foreach (var document in documents)
        {
            var parser = new Parser(new StringReader(document));
            while (parser.MoveNext())
            {
                events++;
            }
        }
        return events;
    }

    [Benchmark(OperationsPerInvoke = DocumentCount)]
    public int DeserializeUntyped()
    {
        var entries = 0;
        foreach (var document in documents)
        {
            entries += deserializer.Deserialize<Dictionary<object, object>>(document).Count;
        }
        return entries;
    }

    // Front-matter-like documents with plain, literal and folded scalars, flow sequences and a
    // nested mapping. Lines are joined with '\n' so the input does not depend on how the source
    // file was checked out.
    private static string CreateDocument(int index)
    {
        switch (index % 4)
        {
            case 0:
                return Lines(
                    "esid: sec-array.prototype.map",
                    $"description: Array.prototype.map - callbackfn is called with three arguments (case {index})",
                    "info: |",
                    "  1. Let O be ? ToObject(this value).",
                    "  2. Let len be ? LengthOfArrayLike(O).",
                    "  3. If IsCallable(callbackfn) is false, throw a TypeError exception.",
                    "includes: [compareArray.js, propertyHelper.js]",
                    "features: [Symbol.iterator, TypedArray]");

            case 1:
                return Lines(
                    "esid: sec-let-and-const-declarations",
                    "description: >",
                    "  Redeclaration of a let binding in the same block throws an early",
                    $"  SyntaxError (case {index}).",
                    "negative:",
                    "  phase: parse",
                    "  type: SyntaxError",
                    "flags: [onlyStrict]",
                    "features: [let]");

            case 2:
                return Lines(
                    $"es5id: 15.2.3.6-4-{index}",
                    "description: |",
                    "  Object.defineProperty - 'O' is an Array, 'name' is the length property of 'O',",
                    "  test the [[Value]] field of 'desc' is an object which has an own valueOf method.",
                    "includes: [propertyHelper.js]");

            default:
                return Lines(
                    "esid: sec-moduleevaluation",
                    $"description: Module namespace object reports its exported bindings (case {index})",
                    "flags: [module, async]",
                    "features: [export-star-as-namespace-from-module, top-level-await]",
                    "includes: [asyncHelpers.js]");
        }
    }

    private static string Lines(params string[] lines) => string.Join("\n", lines) + "\n";
}
