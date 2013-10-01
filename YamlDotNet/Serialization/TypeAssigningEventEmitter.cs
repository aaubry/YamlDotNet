using System;
using System.Globalization;
using YamlDotNet;

namespace YamlDotNet.Serialization
{
	public sealed class TypeAssigningEventEmitter : ChainedEventEmitter
	{
		public TypeAssigningEventEmitter(IEventEmitter nextEmitter)
			: base(nextEmitter)
		{
		}

		public override void Emit(ScalarEventInfo eventInfo)
		{
			eventInfo.IsPlainImplicit = true;
			eventInfo.Style = ScalarStyle.Plain;

			var typeCode = eventInfo.SourceValue != null
				? Type.GetTypeCode(eventInfo.SourceType)
				: TypeCode.Empty;

			switch (typeCode)
			{
				case TypeCode.Boolean:
					eventInfo.Tag = "tag:yaml.org,2002:bool";
					eventInfo.RenderedValue = YamlFormatter.FormatBoolean(eventInfo.SourceValue);
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
					eventInfo.RenderedValue = YamlFormatter.FormatNumber(eventInfo.SourceValue);
					break;

				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal:
					eventInfo.Tag = "tag:yaml.org,2002:float";
					eventInfo.RenderedValue = YamlFormatter.FormatNumber(eventInfo.SourceValue);
					break;

				case TypeCode.String:
				case TypeCode.Char:
					eventInfo.Tag = "tag:yaml.org,2002:str";
					eventInfo.RenderedValue = eventInfo.SourceValue.ToString();
					eventInfo.Style = ScalarStyle.Any;
					break;

				case TypeCode.DateTime:
					eventInfo.Tag = "tag:yaml.org,2002:timestamp";
					eventInfo.RenderedValue = YamlFormatter.FormatDateTime(eventInfo.SourceValue);
					break;

				case TypeCode.Empty:
					eventInfo.Tag = "tag:yaml.org,2002:null";
					eventInfo.RenderedValue = "";
					break;

				default:
					if (eventInfo.SourceType == typeof(TimeSpan))
					{
						eventInfo.RenderedValue = YamlFormatter.FormatTimeSpan(eventInfo.SourceValue);
						break;
					}

					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "TypeCode.{0} is not supported.", typeCode));
			}

			base.Emit(eventInfo);
		}

        public override void Emit(SequenceStartEventInfo eventInfo)
        {
            FillTag(eventInfo);
            base.Emit(eventInfo);
        }


        public override void Emit(MappingStartEventInfo eventInfo)
        {
            FillTag(eventInfo);
            base.Emit(eventInfo);
        }

        private void FillTag(ObjectEventInfo eventInfo)
        {
            var typeCode = eventInfo.SourceValue != null
                ? Type.GetTypeCode(eventInfo.SourceType)
                : TypeCode.Empty;

            // TODO handle sequence differently
            // TODO handle ICollection<>
            // TODO handle Dictionary<,>
            // TODO Add support for Tag Mapping

            if (typeCode == TypeCode.Object && string.IsNullOrEmpty(eventInfo.Tag))
            {
                // quick fix to output tag information
                eventInfo.Tag = "!" + Uri.EscapeDataString(eventInfo.SourceType.FullName);
            }
        }
	}
}