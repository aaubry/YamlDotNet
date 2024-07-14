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
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.Serialization.Schemas;
using YamlDotNet.Serialization.NamingConventions;

namespace YamlDotNet.Serialization.EventEmitters
{
    public sealed class TypeAssigningEventEmitter : ChainedEventEmitter
    {
        private readonly IDictionary<Type, TagName> tagMappings;
        private readonly bool quoteNecessaryStrings;
        private readonly Regex? isSpecialStringValue_Regex;
        private static readonly string SpecialStrings_Pattern =
            @"^("
                + @"null|Null|NULL|\~"
                + @"|true|True|TRUE|false|False|FALSE"
                + @"|[-+]?[0-9]+" // int base 10
                + @"|0o[0-7]+" // int base 8
                + @"|0x[0-9a-fA-F]+" // int base 16
                + @"|[-+]?(\.[0-9]+|[0-9]+(\.[0-9]*)?)([eE][-+]?[0-9]+)?" // float number
                + @"|[-+]?(\.inf|\.Inf|\.INF)"
                + @"|\.nan|\.NaN|\.NAN"
                + @"|\s.*"
            + @")$";

        /// <summary>
        /// This pattern matches strings that are special both in YAML 1.1 and 1.2
        /// </summary>
        private static readonly string CombinedYaml1_1SpecialStrings_Pattern =
            @"^("
                + @"null|Null|NULL|\~"
                + @"|true|True|TRUE|false|False|FALSE"
                + @"|y|Y|yes|Yes|YES|n|N|no|No|NO"
                + @"|on|On|ON|off|Off|OFF"
                + @"|[-+]?0b[0-1_]+" // int base 2
                + @"|[-+]?0o?[0-7_]+" // int base 8 both with and without "o"
                + @"|[-+]?(0|[1-9][0-9_]*)" // int base 10
                + @"|[-+]?0x[0-9a-fA-F_]+" // int base 16
                + @"|[-+]?[1-9][0-9_]*(:[0-5]?[0-9])+" // int base 60
                + @"|[-+]?([0-9][0-9_]*)?\.[0-9_]*([eE][-+][0-9]+)?" // float base 10
                + @"|[-+]?[0-9][0-9_]*(:[0-5]?[0-9])+\.[0-9_]*" // float base 60
                + @"|[-+]?\.(inf|Inf|INF)"
                + @"|\.(nan|NaN|NAN)"
            + @")$";

        private readonly ScalarStyle defaultScalarStyle = ScalarStyle.Any;
        private readonly YamlFormatter formatter;
        private readonly INamingConvention enumNamingConvention;
        private readonly ITypeInspector typeInspector;

        public TypeAssigningEventEmitter(IEventEmitter nextEmitter,
            IDictionary<Type, TagName> tagMappings,
            bool quoteNecessaryStrings,
            bool quoteYaml1_1Strings,
            ScalarStyle defaultScalarStyle,
            YamlFormatter formatter,
            INamingConvention enumNamingConvention,
            ITypeInspector typeInspector)
            : base(nextEmitter)
        {
            this.defaultScalarStyle = defaultScalarStyle;
            this.formatter = formatter;
            this.tagMappings = tagMappings;
            this.quoteNecessaryStrings = quoteNecessaryStrings;

            var specialStringValuePattern = quoteYaml1_1Strings
                ? CombinedYaml1_1SpecialStrings_Pattern
                : SpecialStrings_Pattern;
#if NET40
            isSpecialStringValue_Regex = new Regex(specialStringValuePattern);
#else
            isSpecialStringValue_Regex = new Regex(specialStringValuePattern, RegexOptions.Compiled);
#endif
            this.enumNamingConvention = enumNamingConvention;
            this.typeInspector = typeInspector;
        }

        public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
        {
            var suggestedStyle = ScalarStyle.Plain;

            var value = eventInfo.Source.Value;
            if (value == null)
            {
                eventInfo.Tag = JsonSchema.Tags.Null;
                eventInfo.RenderedValue = "";
            }
            else
            {
                var typeCode = eventInfo.Source.Type.GetTypeCode();
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                        eventInfo.Tag = JsonSchema.Tags.Bool;
                        eventInfo.RenderedValue = formatter.FormatBoolean(value);
                        break;

                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.SByte:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        //Enum's are special cases, they fall in here, but get sent out as a string.
                        if (eventInfo.Source.Type.IsEnum)
                        {
                            eventInfo.Tag = FailsafeSchema.Tags.Str;
                            eventInfo.RenderedValue = formatter.FormatEnum(value, typeInspector, enumNamingConvention);

                            if (quoteNecessaryStrings &&
                                IsSpecialStringValue(eventInfo.RenderedValue) &&
                                formatter.PotentiallyQuoteEnums(value))
                            {
                                suggestedStyle = ScalarStyle.DoubleQuoted;
                            }
                            else
                            {
                                suggestedStyle = defaultScalarStyle;
                            }
                        }
                        else
                        {
                            eventInfo.Tag = JsonSchema.Tags.Int;
                            eventInfo.RenderedValue = formatter.FormatNumber(value);
                        }
                        break;

                    case TypeCode.Single:
                        eventInfo.Tag = JsonSchema.Tags.Float;
                        eventInfo.RenderedValue = formatter.FormatNumber((float)value);
                        break;

                    case TypeCode.Double:
                        eventInfo.Tag = JsonSchema.Tags.Float;
                        eventInfo.RenderedValue = formatter.FormatNumber((double)value);
                        break;

                    case TypeCode.Decimal:
                        eventInfo.Tag = JsonSchema.Tags.Float;
                        eventInfo.RenderedValue = formatter.FormatNumber(value);
                        break;

                    case TypeCode.String:
                    case TypeCode.Char:
                        eventInfo.Tag = FailsafeSchema.Tags.Str;
                        eventInfo.RenderedValue = value.ToString()!;

                        if (quoteNecessaryStrings && IsSpecialStringValue(eventInfo.RenderedValue))
                        {
                            suggestedStyle = ScalarStyle.DoubleQuoted;
                        }
                        else
                        {
                            suggestedStyle = defaultScalarStyle;
                        }

                        break;

                    case TypeCode.DateTime:
                        eventInfo.Tag = DefaultSchema.Tags.Timestamp;
                        eventInfo.RenderedValue = formatter.FormatDateTime(value);
                        break;

                    case TypeCode.Empty:
                        eventInfo.Tag = JsonSchema.Tags.Null;
                        eventInfo.RenderedValue = "";
                        break;

                    default:
                        if (eventInfo.Source.Type == typeof(TimeSpan))
                        {
                            eventInfo.RenderedValue = formatter.FormatTimeSpan(value);
                            break;
                        }

                        throw new NotSupportedException($"TypeCode.{typeCode} is not supported.");
                }
            }

            eventInfo.IsPlainImplicit = true;
            if (eventInfo.Style == ScalarStyle.Any)
            {
                eventInfo.Style = suggestedStyle;
            }

            base.Emit(eventInfo, emitter);
        }

        public override void Emit(MappingStartEventInfo eventInfo, IEmitter emitter)
        {
            AssignTypeIfNeeded(eventInfo);
            base.Emit(eventInfo, emitter);
        }

        public override void Emit(SequenceStartEventInfo eventInfo, IEmitter emitter)
        {
            AssignTypeIfNeeded(eventInfo);
            base.Emit(eventInfo, emitter);
        }

        private void AssignTypeIfNeeded(ObjectEventInfo eventInfo)
        {
            if (tagMappings.TryGetValue(eventInfo.Source.Type, out var tag))
            {
                eventInfo.Tag = tag;
            }
        }

        private bool IsSpecialStringValue(string value)
        {
            if (value.Trim() == string.Empty)
            {
                return true;
            }

            return isSpecialStringValue_Regex?.IsMatch(value) ?? false;
        }
    }
}
