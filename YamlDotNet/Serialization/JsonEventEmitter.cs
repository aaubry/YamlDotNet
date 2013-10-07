using System;
using System.Globalization;
using YamlDotNet;
using YamlDotNet.Events;

namespace YamlDotNet.Serialization
{
	internal sealed class JsonEventEmitter : ChainedEventEmitter
	{
		public JsonEventEmitter(IEventEmitter nextEmitter)
			: base(nextEmitter)
		{
		}

		public override void Emit(AliasEventInfo eventInfo)
		{
			throw new NotSupportedException("Aliases are not supported in JSON");
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
				case TypeCode.String:
				case TypeCode.Char:
					eventInfo.Style = ScalarStyle.DoubleQuoted;
					break;
				case TypeCode.Empty:
					eventInfo.RenderedValue = "null";
					break;
			}

			base.Emit(eventInfo);
		}

		public override void Emit(MappingStartEventInfo eventInfo)
		{
			eventInfo.Style = MappingStyle.Flow;

			base.Emit(eventInfo);
		}

		public override void Emit(SequenceStartEventInfo eventInfo)
		{
			eventInfo.Style = SequenceStyle.Flow;

			base.Emit(eventInfo);
		}
	}
}