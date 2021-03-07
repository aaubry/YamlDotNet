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

using System;
using System.Collections.Generic;
using YamlDotNet.Core;

namespace YamlDotNet.Representation.Schemas
{
    /// <summary>
    /// Implements the YAML 1.1 schema: <see href="https://yaml.org/spec/1.2/spec.html#id2802346"/>.
    /// </summary>
    public static class Yaml11Schema
    {
        public static readonly ISchema Complete = new ContextFreeSchema(CreateMatchers(scalarsOnly: false));
        public static readonly ISchema Scalars = new ContextFreeSchema(CreateMatchers(scalarsOnly: true));

        public static class IntegerMapper
        {
            public enum Formats
            {
                Base2,
                Base8,
                Base10,
                Base16,
                Base60,
            }

            public static readonly MultiFormatScalarMapper<Formats> Canonical = new MultiFormatScalarMapper<Formats>(
                YamlTagRepository.Integer,
                new MultiFormatScalarMapperFormat<Formats>(
                    Formats.Base10,
                    "^[-+]?(0|[1-9][0-9_]*)$",
                    s => IntegerParser.ParseBase10(s.Value),
                    IntegerFormatter.FormatBase10
                ),
                new MultiFormatScalarMapperFormat<Formats>(
                    Formats.Base2,
                    "^[-+]?0b[0-1_]+$",
                    s => IntegerParser.ParseBase2(s.Value),
                    IntegerFormatter.FormatBase2Signed
                ),
                new MultiFormatScalarMapperFormat<Formats>(
                    Formats.Base8,
                    "^[-+]?0[0-7_]+$",
                    s => IntegerParser.ParseBase8(s.Value),
                    IntegerFormatter.FormatBase8Signed
                ),
                new MultiFormatScalarMapperFormat<Formats>(
                    Formats.Base16,
                    "^[-+]?0x[0-9a-fA-F_]+$",
                    s => IntegerParser.ParseBase16(s.Value),
                    IntegerFormatter.FormatBase16Signed
                ),
                new MultiFormatScalarMapperFormat<Formats>(
                    Formats.Base60,
                    "^[-+]?[1-9][0-9_]*(:[0-5]?[0-9])+$",
                    s => IntegerParser.ParseBase60(s.Value),
                    IntegerFormatter.FormatBase60Signed
                )
            );

            public static INodeMapper Base2 => Canonical.GetFormat(Formats.Base2);
            public static INodeMapper Base8 => Canonical.GetFormat(Formats.Base8);
            public static INodeMapper Base10 => Canonical.GetFormat(Formats.Base10);
            public static INodeMapper Base16 => Canonical.GetFormat(Formats.Base16);
            public static INodeMapper Base60 => Canonical.GetFormat(Formats.Base60);
        }

        public static class FloatingPointMapper
        {
            public enum Formats
            {
                Base10,
                Base10Separated,
                Base60,
                Infinity,
                NotANumber,
            }

            public static readonly MultiFormatScalarMapper<Formats> Canonical = new MultiFormatScalarMapper<Formats>(
                YamlTagRepository.FloatingPoint,
                new MultiFormatScalarMapperFormat<Formats>(
                    Formats.Base10,
                    @"^[-+]?([0-9][0-9]*)?\.[0-9]*([eE][-+][0-9]+)?$",
                    s => FloatingPointParser.ParseBase10Unseparated(s.Value),
                    FloatingPointFormatter.FormatBase10Unseparated
                ),
                new MultiFormatScalarMapperFormat<Formats>(
                    Formats.Base10Separated,
                    @"^[-+]?([0-9][0-9_]*)?\.[0-9_]*([eE][-+][0-9]+)?$",
                    s => FloatingPointParser.ParseBase10Separated(s.Value),
                    FloatingPointFormatter.FormatBase10Unseparated
                ),
                new MultiFormatScalarMapperFormat<Formats>(
                    Formats.Base60,
                    @"^[-+]?[0-9][0-9_]*(:[0-5]?[0-9])+\.[0-9_]*$",
                    s => FloatingPointParser.ParseBase60(s.Value),
                    FloatingPointFormatter.FormatBase60
                ),
                new MultiFormatScalarMapperFormat<Formats>(
                    Formats.Infinity,
                    @"^[-+]?\.(inf|Inf|INF)$",
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

            public static INodeMapper Base10 => Canonical.GetFormat(Formats.Base10);
            public static INodeMapper Base10Separated => Canonical.GetFormat(Formats.Base10Separated);
            public static INodeMapper Base60 => Canonical.GetFormat(Formats.Base60);
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
                .MatchEmptyTags()
                .Create();


            yield return NodeMatcher
                .ForScalars(
                    NodeMapper.CreateScalarMapper(
                        YamlTagRepository.Boolean,
                        s =>
                        {
                            // Assumes that the value matches the regex
                            return s.Value[0] switch
                            {
                                'y' => true, // y, yes
                                'Y' => true, // Y, Yes, YES
                                't' => true, // true
                                'T' => true, // True, TRUE
                                'n' => false, // n, no
                                'N' => false, // N, No, NO
                                _ => s.Value[1] switch
                                {
                                    'n' => true, // on, On
                                    'N' => true, // ON
                                    _ => false
                                }
                            };
                        },
                        native => true.Equals(native) ? "true" : "false"
                    )
                )
                .MatchPattern("^(y|Y|yes|Yes|YES|n|N|no|No|NO|true|True|TRUE|false|False|FALSE|on|On|ON|off|Off|OFF)$")
                .MatchEmptyTags()
                .Create();

            foreach (var format in IntegerMapper.Canonical.Formats)
            {
                yield return NodeMatcher
                    .ForScalars(format.Mapper)
                    .MatchPattern(format.Pattern)
                    .MatchEmptyTags()
                    .Create();
            }

            foreach (var format in FloatingPointMapper.Canonical.Formats)
            {
                yield return NodeMatcher
                    .ForScalars(format.Mapper)
                    .MatchPattern(format.Pattern)
                    .MatchEmptyTags()
                    .Create();
            }

            yield return NodeMatcher
                .ForScalars(StringMapper.Default, ScalarStyle.SingleQuoted)
                .MatchAnyNonSpecificTags()
                .Create();

            if (scalarsOnly)
            {
                yield return NodeMatcher
                    .ForSequences(SequenceMapper<object>.Default)
                    .MatchAnyNonSpecificTags()
                    .Create();

                yield return NodeMatcher
                    .ForMappings(MappingMapper<object, object>.Default)
                    .MatchAnyNonSpecificTags()
                    .Create();
            }
        }
    }
}