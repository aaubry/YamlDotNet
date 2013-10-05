using System.IO;

namespace YamlDotNet.Serialization
{
    public class YamlSerializer
    {
        private readonly YamlSerializerSettings settings;
	    internal readonly ITagTypeRegistry TagTypeRegistry;

        public YamlSerializer() : this(null)
        {
        }

        public YamlSerializerSettings Settings { get { return settings; } }

        public YamlSerializer(YamlSerializerSettings settings)
        {
            this.settings = settings ?? new YamlSerializerSettings();
	        TagTypeRegistry = settings.TagTypeRegistry;
        }

        public void Serialize(Stream stream, object value)
        {
            var context = new SerializerContext(this);
            var writer = CreateEmitter(stream, context);
            context.Writer = writer;

        }

        private IEventEmitter CreateEmitter(Stream stream, SerializerContext context)
        {
            return CreateEmitter(new Emitter(new StreamWriter(stream)), context);
        }

        private IEventEmitter CreateEmitter(IEmitter emitter, SerializerContext context)
        {
            var writer = new WriterEventEmitter(emitter, context);

            if (settings.EmitJsonComptible)
            {
                return new JsonEventEmitter(writer);
            }
	        return writer;
        }
    }
}