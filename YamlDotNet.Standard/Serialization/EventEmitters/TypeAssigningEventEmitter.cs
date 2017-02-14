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
using System.Globalization;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.EventEmitters
{
    public sealed class TypeAssigningEventEmitter : ChainedEventEmitter
    {
        private readonly bool _assignTypeWhenDifferent;

        public TypeAssigningEventEmitter(IEventEmitter nextEmitter, bool assignTypeWhenDifferent)
            : base(nextEmitter)
        {
            _assignTypeWhenDifferent = assignTypeWhenDifferent;
        }

        public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
        {
            var suggestedStyle = ScalarStyle.Plain;

            var typeCode = eventInfo.Source.Value != null
                ? eventInfo.Source.Type.GetTypeCode()
                : TypeCode.Empty;

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    eventInfo.Tag = "tag:yaml.org,2002:bool";
                    eventInfo.RenderedValue = YamlFormatter.FormatBoolean(eventInfo.Source.Value);
                    break;

                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    eventInfo.Tag = "tag:yaml.org,2002:int";
                    eventInfo.RenderedValue = YamlFormatter.FormatNumber(eventInfo.Source.Value);
                    break;

                case TypeCode.Single:
                    eventInfo.Tag = "tag:yaml.org,2002:float";
                    eventInfo.RenderedValue = YamlFormatter.FormatNumber((float)eventInfo.Source.Value);
                    break;

                case TypeCode.Double:
                    eventInfo.Tag = "tag:yaml.org,2002:float";
                    eventInfo.RenderedValue = YamlFormatter.FormatNumber((double)eventInfo.Source.Value);
                    break;

                case TypeCode.Decimal:
                    eventInfo.Tag = "tag:yaml.org,2002:float";
                    eventInfo.RenderedValue = YamlFormatter.FormatNumber(eventInfo.Source.Value);
                    break;

                case TypeCode.String:
                case TypeCode.Char:
                    eventInfo.Tag = "tag:yaml.org,2002:str";
                    eventInfo.RenderedValue = eventInfo.Source.Value.ToString();
                    suggestedStyle = ScalarStyle.Any;
                    break;

                case TypeCode.DateTime:
                    eventInfo.Tag = "tag:yaml.org,2002:timestamp";
                    eventInfo.RenderedValue = YamlFormatter.FormatDateTime(eventInfo.Source.Value);
                    break;

                case TypeCode.Empty:
                    eventInfo.Tag = "tag:yaml.org,2002:null";
                    eventInfo.RenderedValue = "";
                    break;

                default:
                    if (eventInfo.Source.Type == typeof(TimeSpan))
                    {
                        eventInfo.RenderedValue = YamlFormatter.FormatTimeSpan(eventInfo.Source.Value);
                        break;
                    }

                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "TypeCode.{0} is not supported.", typeCode));
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
            AssignTypeIfDifferent(eventInfo);
            base.Emit(eventInfo, emitter);
        }

        public override void Emit(SequenceStartEventInfo eventInfo, IEmitter emitter)
        {
            AssignTypeIfDifferent(eventInfo);
            base.Emit(eventInfo, emitter);
        }

        private void AssignTypeIfDifferent(ObjectEventInfo eventInfo)
        {
            if (_assignTypeWhenDifferent && eventInfo.Source.Value != null)
            {
                if (eventInfo.Source.Type != eventInfo.Source.StaticType)
                {
                    eventInfo.Tag = "!" + eventInfo.Source.Type.AssemblyQualifiedName;
                }
            }
        }
    }
}