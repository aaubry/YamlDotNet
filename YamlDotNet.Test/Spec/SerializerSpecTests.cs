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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Xunit;
using Xunit.Sdk;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test.Spec
{
    public sealed class SerializerSpecTests
    {
        internal sealed class SerializerSpecTestsData : SpecTestsData
        {
            protected override List<string> IgnoredSuites { get; } = ignoredSuites;
        }

        private static readonly List<string> ignoredSuites = new List<string>
        {
            //TODO: research why these are ignored
            "26DV", "27NA", "2AUY", "2JQS", "2LFX", "2SXE", "2XXW", "33X3", "35KP", "36F6", "3GZX", "3MYT", "3R3P", "3UYS", "4ABK",
            "4CQQ", "4FJ6", "4GC6", "4MUZ", "4Q9F", "4QFQ", "4UYU", "4V8U", "4ZYM", "52DL", "565N", "57H4", "5BVJ", "5GBF", "5MUD",
            "5TYM", "5WE3", "6BFJ", "6CK3", "6FWR", "6HB6", "6JQW", "6JWB", "6KGN", "6LVF", "6M2F", "6PBE", "6SLA", "6WLZ", "6XDY",
            "6ZKB", "735Y", "74H7", "753E", "77H8", "7BMT", "7BUB", "7FWL", "7T8X", "7TMG", "7W2P", "7Z25", "7ZZ5", "87E4", "8CWC",
            "8G76", "8KB6", "8MK2", "8UDB", "8XYN", "93WF", "96L6", "98YD", "9BXH", "9DXL", "9KAX", "9MMW", "9SA2", "9SHH", "9U5K",
            "9WXW", "9YRD", "A6F9", "AVM7", "BEC7", "BU8L", "C2DT", "C4HZ", "CC74", "CN3R", "CPZ3", "CUP7", "D83L", "DBG4", "DFF7",
            "DK3J", "E76Z", "EHF6", "EX5H", "F2C7", "F3CP", "F6MC", "F8F9", "FH7J", "FP8R", "FRK4", "FTA2", "G4RS", "G5U8", "H3Z8",
            "HMK4", "HMQ5", "HS5T", "HWV9", "J3BT", "J7PZ", "J9HZ", "JDH8", "JHB9", "JS2J", "JTV5", "K3WX", "K54U", "K858", "KK5P",
            "KSS4", "KZN9", "L94M", "LE5A", "LP6E", "LQZ7", "M29M", "M5C3", "M7A3", "M7NX", "M9B4", "MJS9", "MYW6", "MZX3", "NAT4",
            "NB6Z", "NHX8", "NJ66", "NP9H", "P2AD", "P76L", "PRH3", "PUW8", "PW8X", "Q88A", "Q8AD", "QT73", "R4YG", "R52L", "RTP8",
            "RZP5", "RZT7", "S3PD", "S4JQ", "S4T7", "S7BG", "SKE5", "SSW6", "T4YY", "T5N4", "U3C3", "U3XV", "U9NS", "UGM3", "UT92",
            "V55R", "W42U", "W4TN", "W5VH", "WZ62", "X38W", "X8DW", "XLQ9", "XV9V", "XW4D", "Y2GN", "Z67P", "Z9M4", "ZH7C", "ZWK4",

            // TODO: resolve these, it will end up resolving some of the git hub issues on yamldotnet
            "4WA9",
            "58MP",
            "5T43",
            "652Z",
            "6CA3",
            "CFD4",
            "JR7V",
            "L383",
            "M6YH",
            "NKF9",
            "YJV2"
        };

        private static readonly List<string> knownFalsePositives = new List<string>
        {
            // no false-positives known as of https://github.com/yaml/yaml-test-suite/releases/tag/data-2020-02-11
        };

        private readonly IDeserializer deserializer = new DeserializerBuilder().Build();
        private readonly ISerializer serializer = new SerializerBuilder().Build();

        [Theory, ClassData(typeof(SerializerSpecTestsData))]
        public void ConformsWithYamlSpec(string name, string description, string inputFile, string outputFile, bool error)
        {
            var expectedResult = File.ReadAllText(outputFile);
            using var writer = new StringWriter();
            try
            {
                using var reader = File.OpenText(inputFile);
                var subject = deserializer.Deserialize(reader);
                serializer.Serialize(writer, subject);
            }
            catch (Exception ex)
            {
                Assert.True(error, $"Unexpected spec failure ({name}).\n{description}\nExpected:\n{expectedResult}\nActual:\n[Writer Output]\n{writer}\n[Exception]\n{ex}");

                if (error)
                {
                    Debug.Assert(!knownFalsePositives.Contains(name), $"Spec test '{name}' passed but present in '{nameof(knownFalsePositives)}' list. Consider removing it from the list.");
                }

                return;
            }

            try
            {
                Assert.Equal(expectedResult, writer.ToString(), ignoreLineEndingDifferences: true);
                Debug.Assert(!ignoredSuites.Contains(name), $"Spec test '{name}' passed but present in '{nameof(ignoredSuites)}' list. Consider removing it from the list.");
            }
            catch (EqualException)
            {
                // In some cases, YamlDotNet's parser/scanner is unexpectedly *not* erroring out.
                // Throw, if it is not a known case.

                if (!(error && knownFalsePositives.Contains(name)))
                {
                    throw;
                }
            }
        }
    }
}
