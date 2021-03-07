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
    /// Implements the JSON schema: <see href="https://yaml.org/spec/1.2/spec.html#id2803231"/>.
    /// </summary>
    public static class JsonSchema
    {
        /// <summary>
        /// A version of the <see cref="JsonSchema"/> that conforms strictly to the specification
        /// by not resolving any unrecognized scalars.
        /// </summary>
        public static readonly ISchema Strict = new ContextFreeSchema(CreateMatchers(true));

        /// <summary>
        /// A version of the <see cref="JsonSchema"/> that treats unrecognized scalars as strings.
        /// </summary>
        public static readonly ISchema Lenient = new ContextFreeSchema(CreateMatchers(false));

        private static IEnumerable<NodeMatcher> CreateMatchers(bool strict)
        {
            const string NullLiteral = "null";

            yield return NodeMatcher
                .ForScalars(
                    NodeMapper.CreateScalarMapper(
                        YamlTagRepository.Null,
                        _ => null,
                        _ => NullLiteral
                    )
                )
                .MatchPattern("^null$")
                .MatchEmptyTags()
                .Create();

            yield return NodeMatcher
                .ForScalars(
                    NodeMapper.CreateScalarMapper(
                        YamlTagRepository.Boolean,
                        s => s.Value[0] == 't', // Assumes that the value matches the regex
                        FormatBoolean
                    )
                )
                .MatchPattern("^(true|false)$")
                .MatchEmptyTags()
                .Create();

            yield return NodeMatcher
                .ForScalars(
                    NodeMapper.CreateScalarMapper(
                        YamlTagRepository.Integer,
                        s => IntegerParser.ParseBase10(s.Value),
                        IntegerFormatter.FormatBase10
                    )
                )
                .MatchPattern("^-?(0|[1-9][0-9]*)$")
                .MatchEmptyTags()
                .Create();

            yield return NodeMatcher
                .ForScalars(
                    NodeMapper.CreateScalarMapper(
                        YamlTagRepository.FloatingPoint,
                        s => FloatingPointParser.ParseBase10Unseparated(s.Value),
                        FloatingPointFormatter.FormatBase10Unseparated
                    )
                )
                .MatchPattern(@"^-?(0|[1-9][0-9]*)(\.[0-9]*)?([eE][-+]?[0-9]+)?$")
                .MatchEmptyTags()
                .Create();

            yield return NodeMatcher
                .ForSequences(SequenceMapper<object>.Default, SequenceStyle.Flow)
                .MatchAnyNonSpecificTags()
                .Create();

            yield return NodeMatcher
                .ForMappings(MappingMapper<object, object>.Default, MappingStyle.Flow)
                .MatchAnyNonSpecificTags()
                .Create();

            if (strict)
            {
                yield return NodeMatcher
                    .ForScalars(StringMapper.Default, ScalarStyle.DoubleQuoted)
                    .MatchNonSpecificTags()
                    .Create();
            }
            else
            {
                yield return NodeMatcher
                    .ForScalars(StringMapper.Default, ScalarStyle.DoubleQuoted)
                    .MatchAnyNonSpecificTags()
                    .Create();
            }
        }

        internal static string FormatBoolean(object? native)
        {
            return true.Equals(native) ? "true" : "false";
        }
    }
}
