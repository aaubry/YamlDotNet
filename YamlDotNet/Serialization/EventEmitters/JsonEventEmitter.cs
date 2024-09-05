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
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.NamingConventions;

namespace YamlDotNet.Serialization.EventEmitters
{
    public sealed class JsonEventEmitter : ChainedEventEmitter
    {
        private readonly YamlFormatter formatter;
        private readonly INamingConvention enumNamingConvention;
        private readonly ITypeInspector typeInspector;
        private static readonly Regex NumericRegex = new Regex(@"^-?\d+\.?\d+$", RegexOptions.Compiled);

        public JsonEventEmitter(IEventEmitter nextEmitter, YamlFormatter formatter, INamingConvention enumNamingConvention, ITypeInspector typeInspector)
            : base(nextEmitter)
        {
            this.formatter = formatter;
            this.enumNamingConvention = enumNamingConvention;
            this.typeInspector = typeInspector;
        }

        public override void Emit(AliasEventInfo eventInfo, IEmitter emitter)
        {
            eventInfo.NeedsExpansion = true;
        }

        public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
        {
            eventInfo.IsPlainImplicit = true;
            eventInfo.Style = ScalarStyle.Plain;

            var value = eventInfo.Source.Value;
            if (value == null)
            {
                eventInfo.RenderedValue = "null";
            }
            else
            {
                var typeCode = eventInfo.Source.Type.GetTypeCode();
                switch (typeCode)
                {
                    case TypeCode.Boolean:
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
                        var valueIsEnum = eventInfo.Source.Type.IsEnum();
                        if (valueIsEnum)
                        {
                            eventInfo.RenderedValue = formatter.FormatEnum(value, typeInspector, enumNamingConvention);
                            eventInfo.Style = formatter.PotentiallyQuoteEnums(value) ? ScalarStyle.DoubleQuoted : ScalarStyle.Plain;
                            break;
                        }

                        eventInfo.RenderedValue = formatter.FormatNumber(value);
                        break;

                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                        eventInfo.RenderedValue = formatter.FormatNumber(value);

                        if (!NumericRegex.IsMatch(eventInfo.RenderedValue))
                        {
                            eventInfo.Style = ScalarStyle.DoubleQuoted;
                        }

                        break;

                    case TypeCode.String:
                    case TypeCode.Char:
                        eventInfo.RenderedValue = value.ToString()!;
                        eventInfo.Style = ScalarStyle.DoubleQuoted;
                        break;

                    case TypeCode.DateTime:
                        eventInfo.RenderedValue = formatter.FormatDateTime(value);
                        break;

                    case TypeCode.Empty:
                        eventInfo.RenderedValue = "null";
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

            base.Emit(eventInfo, emitter);
        }

        public override void Emit(MappingStartEventInfo eventInfo, IEmitter emitter)
        {
            eventInfo.Style = MappingStyle.Flow;

            base.Emit(eventInfo, emitter);
        }

        public override void Emit(SequenceStartEventInfo eventInfo, IEmitter emitter)
        {
            eventInfo.Style = SequenceStyle.Flow;

            base.Emit(eventInfo, emitter);
        }
    }
}
