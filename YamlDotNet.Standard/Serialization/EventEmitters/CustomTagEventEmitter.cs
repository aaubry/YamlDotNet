using System;
using System.Collections.Generic;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.EventEmitters
{
    internal class CustomTagEventEmitter: ChainedEventEmitter
    {
        private IDictionary<Type, string> tagMappings;

        public CustomTagEventEmitter(IEventEmitter inner, IDictionary<Type, string> tagMappings)
            :base(inner)
        {
            this.tagMappings = tagMappings;
        }

        public override void Emit(MappingStartEventInfo eventInfo, IEmitter emitter)
        {
            if(tagMappings.ContainsKey(eventInfo.Source.Type))
            {
                eventInfo.Tag = tagMappings[eventInfo.Source.Type];
            }
            base.Emit(eventInfo, emitter);
        }
    }
}