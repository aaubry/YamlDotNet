using YamlDotNet;
using YamlDotNet.Events;

namespace YamlDotNet.Serialization
{
	public sealed class WriterEventEmitter : IEventEmitter
	{
		private readonly IEmitter emitter;
		private readonly SerializerContext context;

		public WriterEventEmitter(IEmitter emitter, SerializerContext context)
		{
			this.emitter = emitter;
			this.context = context;
		}

		void IEventEmitter.Emit(AliasEventInfo eventInfo)
		{
			emitter.Emit(new AnchorAlias(eventInfo.Alias));
		}

		void IEventEmitter.Emit(ScalarEventInfo eventInfo)
		{
			emitter.Emit(new Scalar(eventInfo.Anchor ?? context.GetAnchor(), eventInfo.Tag, eventInfo.RenderedValue, eventInfo.Style, eventInfo.IsPlainImplicit, eventInfo.IsQuotedImplicit));
		}

		void IEventEmitter.Emit(MappingStartEventInfo eventInfo)
		{
			emitter.Emit(new MappingStart(eventInfo.Anchor ?? context.GetAnchor(), eventInfo.Tag, eventInfo.IsImplicit, eventInfo.Style));
		}

		void IEventEmitter.Emit(MappingEndEventInfo eventInfo)
		{
			emitter.Emit(new MappingEnd());
		}

		void IEventEmitter.Emit(SequenceStartEventInfo eventInfo)
		{
			emitter.Emit(new SequenceStart(eventInfo.Anchor ?? context.GetAnchor(), eventInfo.Tag, eventInfo.IsImplicit, eventInfo.Style));
		}

		void IEventEmitter.Emit(SequenceEndEventInfo eventInfo)
		{
			emitter.Emit(new SequenceEnd());
		}
	}
}