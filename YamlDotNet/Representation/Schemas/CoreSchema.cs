//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) Antoine Aubry and contributors

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System.Collections.Generic;
using YamlDotNet.Core;

namespace YamlDotNet.Representation.Schemas
{
    /// <summary>
    /// Implements the Core schema: <see href="https://yaml.org/spec/1.2/spec.html#id2804923"/>.
    /// </summary>
    public static class CoreSchema
    {
        public static readonly ISchema Complete = new ContextFreeSchema(CreateMatchers(scalarsOnly: false));
        public static readonly ISchema Scalars = new ContextFreeSchema(CreateMatchers(scalarsOnly: true));

        public static class IntegerMapper
        {
            public enum Formats
            {
                Base8,
                Base10,
                Base16,
            }

            public static readonly MultiFormatScalarMapper<Formats> Canonical = new MultiFormatScalarMapper<Formats>(
                YamlTagRepository.Integer,
                new MultiFormatScalarMapperFormat<Formats>(
                    Formats.Base10,
                    "^[-+]?[0-9]+$",
                    s => IntegerParser.ParseBase10(s.Value),
                    IntegerFormatter.FormatBase10
                ),
                new MultiFormatScalarMapperFormat<Formats>(
                    Formats.Base8,
                    "^0o[0-7]+$",
                    s => IntegerParser.ParseBase8Unsigned(s.Value),
                    IntegerFormatter.FormatBase8Unsigned
                ),
                new MultiFormatScalarMapperFormat<Formats>(
                    Formats.Base16,
                    "^0x[0-9a-fA-F]+$",
                    s => IntegerParser.ParseBase16Unsigned(s.Value),
                    IntegerFormatter.FormatBase16Unsigned
                )
            );

            public static INodeMapper Base8 => Canonical.GetFormat(Formats.Base8);
            public static INodeMapper Base10 => Canonical.GetFormat(Formats.Base10);
            public static INodeMapper Base16 => Canonical.GetFormat(Formats.Base16);
        }

        public static class FloatingPointMapper
        {
            public enum Formats
            {
                Regular,
                Infinity,
                NotANumber,
            }

            public static readonly MultiFormatScalarMapper<Formats> Canonical = new MultiFormatScalarMapper<Formats>(
                YamlTagRepository.FloatingPoint,
                new MultiFormatScalarMapperFormat<Formats>(
                    Formats.Regular,
                    @"^[-+]?(\.[0-9]+|[0-9]+(\.[0-9]*)?)([eE][-+]?[0-9]+)?$",
                    s => FloatingPointParser.ParseBase10Unseparated(s.Value),
                    FloatingPointFormatter.FormatBase10Unseparated
                ),
                new MultiFormatScalarMapperFormat<Formats>(
                    Formats.Infinity,
                    @"^[-+]?(\.inf|\.Inf|\.INF)$",
                    s => s.Value[0] == '-' ? double.NegativeInfinity : double.PositiveInfinity,
                    FloatingPointFormatter.FormatBase10Unseparated
                ),
                new MultiFormatScalarMapperFormat<Formats>(
                    Formats.NotANumber,
                    @"^(\.nan|\.NaN|\.NAN)$",
                    s => double.NaN,
                    FloatingPointFormatter.FormatBase10Unseparated
                )
            );
        }

        private static IEnumerable<NodeMatcher> CreateMatchers(bool scalarsOnly)
        {
            yield return NodeMatcher
                .ForScalars(
                    NodeMapper.CreateScalarMapper(
                        YamlTagRepository.Null,
                        _ => null,
                        _ => "null"
                    )
                )
                .MatchPattern("^(null|Null|NULL|~?)$")
                .Either(
                    s => s.MatchEmptyTags(),
                    s => s.MatchTag(YamlTagRepository.Null)
                )
                .Create();

            yield return NodeMatcher
                .ForScalars(
                    NodeMapper.CreateScalarMapper(
                        YamlTagRepository.Boolean,
                        s =>
                        {
                            // Assumes that the value matches the regex
                            var firstChar = s.Value[0];
                            return firstChar == 't' || firstChar == 'T';
                        },
                        JsonSchema.FormatBoolean
                    ),
                    typeof(bool)
                )
                .MatchPattern("^(true|True|TRUE|false|False|FALSE)$")
                .Either(
                    s => s.MatchEmptyTags(),
                    s => s.MatchTag(YamlTagRepository.Boolean)
                )
                .Create();

            foreach (var format in IntegerMapper.Canonical.Formats)
            {
                yield return NodeMatcher
                    .ForScalars(format.Mapper,
                        typeof(int),
                        typeof(long),
                        typeof(short),
                        typeof(sbyte),
                        typeof(uint),
                        typeof(ulong),
                        typeof(ushort),
                        typeof(byte)
                    )
                    .MatchPattern(format.Pattern)
                    .Either(
                        s => s.MatchEmptyTags(),
                        s => s.MatchTag(YamlTagRepository.Integer)
                    )
                    .Create();
            }

            foreach (var format in FloatingPointMapper.Canonical.Formats)
            {
                yield return NodeMatcher
                    .ForScalars(format.Mapper,
                        typeof(float),
                        typeof(double)
                    )
                    .MatchPattern(format.Pattern)
                    .Either(
                        s => s.MatchEmptyTags(),
                        s => s.MatchTag(YamlTagRepository.FloatingPoint)
                    )
                    .Create();
            }

            yield return NodeMatcher
                .ForScalars(StringMapper.Default, ScalarStyle.SingleQuoted,
                    typeof(string),
                    typeof(char) // TODO: Test this
                )
                .Either(
                    s => s.MatchAnyNonSpecificTags(),
                    s => s.MatchTag(YamlTagRepository.String)
                )
                .Create();

            if (!scalarsOnly)
            {
                yield return NodeMatcher
                    .ForSequences(SequenceMapper<object>.Default)
                    .Either(
                        s => s.MatchAnyNonSpecificTags(),
                        s => s.MatchTag(YamlTagRepository.Sequence)
                    )
                    .Create();

                yield return NodeMatcher
                    .ForMappings(MappingMapper<object, object>.Default)
                    .Either(
                        s => s.MatchAnyNonSpecificTags(),
                        s => s.MatchTag(YamlTagRepository.Mapping)
                    )
                    .Create();
            }
        }
    }
}
