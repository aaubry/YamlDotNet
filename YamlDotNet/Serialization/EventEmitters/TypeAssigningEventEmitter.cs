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
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.Serialization.Schemas;

namespace YamlDotNet.Serialization.EventEmitters
{
    public sealed class TypeAssigningEventEmitter : ChainedEventEmitter
    {
        private readonly bool requireTagWhenStaticAndActualTypesAreDifferent;
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

        public TypeAssigningEventEmitter(IEventEmitter nextEmitter, bool requireTagWhenStaticAndActualTypesAreDifferent, IDictionary<Type, TagName> tagMappings, bool quoteNecessaryStrings, bool quoteYaml1_1Strings)
            : this(nextEmitter, requireTagWhenStaticAndActualTypesAreDifferent, tagMappings)
        {
            this.quoteNecessaryStrings = quoteNecessaryStrings;

            var specialStringValuePattern = quoteYaml1_1Strings
                ? CombinedYaml1_1SpecialStrings_Pattern
                : SpecialStrings_Pattern;
#if NET40
            isSpecialStringValue_Regex = new Regex(specialStringValuePattern);
#else
            isSpecialStringValue_Regex = new Regex(specialStringValuePattern, RegexOptions.Compiled);
#endif
        }

        public TypeAssigningEventEmitter(IEventEmitter nextEmitter, bool requireTagWhenStaticAndActualTypesAreDifferent, IDictionary<Type, TagName> tagMappings, bool quoteNecessaryStrings)
            : this(nextEmitter, requireTagWhenStaticAndActualTypesAreDifferent, tagMappings)
        {
            this.quoteNecessaryStrings = quoteNecessaryStrings;

#if NET40
            isSpecialStringValue_Regex = new Regex(SpecialStrings_Pattern);
#else
            isSpecialStringValue_Regex = new Regex(SpecialStrings_Pattern, RegexOptions.Compiled);
#endif
        }

        public TypeAssigningEventEmitter(IEventEmitter nextEmitter, bool requireTagWhenStaticAndActualTypesAreDifferent, IDictionary<Type, TagName> tagMappings)
            : base(nextEmitter)
        {
            this.requireTagWhenStaticAndActualTypesAreDifferent = requireTagWhenStaticAndActualTypesAreDifferent;
            this.tagMappings = tagMappings ?? throw new ArgumentNullException(nameof(tagMappings));
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
                        eventInfo.RenderedValue = YamlFormatter.FormatBoolean(value);
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
                            eventInfo.RenderedValue = value.ToString()!;

                            if (quoteNecessaryStrings && IsSpecialStringValue(eventInfo.RenderedValue))
                            {
                                suggestedStyle = ScalarStyle.DoubleQuoted;
                            }
                            else
                            {
                                suggestedStyle = ScalarStyle.Any;
                            }
                        }
                        else
                        {
                            eventInfo.Tag = JsonSchema.Tags.Int;
                            eventInfo.RenderedValue = YamlFormatter.FormatNumber(value);
                        }
                        break;

                    case TypeCode.Single:
                        eventInfo.Tag = JsonSchema.Tags.Float;
                        eventInfo.RenderedValue = YamlFormatter.FormatNumber((float)value);
                        break;

                    case TypeCode.Double:
                        eventInfo.Tag = JsonSchema.Tags.Float;
                        eventInfo.RenderedValue = YamlFormatter.FormatNumber((double)value);
                        break;

                    case TypeCode.Decimal:
                        eventInfo.Tag = JsonSchema.Tags.Float;
                        eventInfo.RenderedValue = YamlFormatter.FormatNumber(value);
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
                            suggestedStyle = ScalarStyle.Any;
                        }

                        break;

                    case TypeCode.DateTime:
                        eventInfo.Tag = DefaultSchema.Tags.Timestamp;
                        eventInfo.RenderedValue = YamlFormatter.FormatDateTime(value);
                        break;

                    case TypeCode.Empty:
                        eventInfo.Tag = JsonSchema.Tags.Null;
                        eventInfo.RenderedValue = "";
                        break;

                    default:
                        if (eventInfo.Source.Type == typeof(TimeSpan))
                        {
                            eventInfo.RenderedValue = YamlFormatter.FormatTimeSpan(value);
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
            else if (requireTagWhenStaticAndActualTypesAreDifferent && eventInfo.Source.Value != null && eventInfo.Source.Type != eventInfo.Source.StaticType)
            {
                throw new YamlException(
                    $"Cannot serialize type '{eventInfo.Source.Type.FullName}' where a '{eventInfo.Source.StaticType.FullName}' was expected "
                    + $"because no tag mapping has been registered for '{eventInfo.Source.Type.FullName}', "
                    + $"which means that it won't be possible to deserialize the document.\n"
                    + $"Register a tag mapping using the SerializerBuilder.WithTagMapping method.\n\n"
                    + $"E.g: builder.WithTagMapping(\"!{eventInfo.Source.Type.Name}\", typeof({eventInfo.Source.Type.FullName}));"
                );
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
