using System;
using System.Globalization;
using YamlDotNet.Core;

namespace YamlDotNet.RepresentationModel.Serialization
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
	}
}