using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.RepresentationModel.Serialization.EventEmitters
{
	public sealed class WriterEventEmitter : IEventEmitter
	{
		private readonly IEmitter emitter;

		public WriterEventEmitter(IEmitter emitter)
		{
			this.emitter = emitter;
		}

		void IEventEmitter.Emit(AliasEventInfo eventInfo)
		{
			emitter.Emit(new AnchorAlias(eventInfo.Alias));
		}

		void IEventEmitter.Emit(ScalarEventInfo eventInfo)
		{
			emitter.Emit(new Scalar(eventInfo.Anchor, eventInfo.Tag, eventInfo.RenderedValue, eventInfo.Style, eventInfo.IsPlainImplicit, eventInfo.IsQuotedImplicit));
		}

		void IEventEmitter.Emit(MappingStartEventInfo eventInfo)
		{
			emitter.Emit(new MappingStart(eventInfo.Anchor, eventInfo.Tag, eventInfo.IsImplicit, eventInfo.Style));
		}

		void IEventEmitter.Emit(MappingEndEventInfo eventInfo)
		{
			emitter.Emit(new MappingEnd());
		}

		void IEventEmitter.Emit(SequenceStartEventInfo eventInfo)
		{
			emitter.Emit(new SequenceStart(eventInfo.Anchor, eventInfo.Tag, eventInfo.IsImplicit, eventInfo.Style));
		}

		void IEventEmitter.Emit(SequenceEndEventInfo eventInfo)
		{
			emitter.Emit(new SequenceEnd());
		}
	}
}