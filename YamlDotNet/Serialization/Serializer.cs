using System;
using System.IO;
using YamlDotNet.Events;
using YamlDotNet.Serialization.Serializers;

namespace YamlDotNet.Serialization
{
    public class Serializer
    {
        private readonly SerializerSettings settings;

		/// <summary>
		/// Initializes a new instance of the <see cref="Serializer"/> class.
		/// </summary>
        public Serializer() : this(null)
        {
        }

		/// <summary>
		/// Gets the settings.
		/// </summary>
		/// <value>The settings.</value>
        public SerializerSettings Settings { get { return settings; } }

        public Serializer(SerializerSettings settings)
        {
            this.settings = settings ?? new SerializerSettings();
        }

		public object Deserialize(Stream stream)
		{
			return Deserialize(stream, null);
		}

		public object Deserialize(TextReader reader)
		{
			return Deserialize((TextReader)reader, null);
		}

		public object Deserialize(Stream stream, Type expectedType)
		{
			if (stream == null) throw new ArgumentNullException("stream");

			return Deserialize(new StreamReader(stream), null);
		}

		public object Deserialize(string fromText)
		{
			return Deserialize(fromText, null);
		}

		public object Deserialize(string fromText, Type expectedType)
		{
			if (fromText == null) throw new ArgumentNullException("fromText");
			return Deserialize(new StringReader(fromText), expectedType);
		}

	    public object Deserialize(TextReader reader, Type expectedType)
	    {
		    if (reader == null) throw new ArgumentNullException("reader");
		    return Deserialize(new EventReader(new Parser(reader)), null);
	    }

	    public object Deserialize(EventReader reader, Type expectedType)
		{
			if (reader == null) throw new ArgumentNullException("reader");
			
			var hasStreamStart = reader.Allow<StreamStart>() != null;
			var hasDocumentStart = reader.Allow<DocumentStart>() != null;

			object result = null;
			if (!reader.Accept<DocumentEnd>() && !reader.Accept<StreamEnd>())
			{
				var context = new SerializerContext(this)
					{
						Reader = reader,
						ObjectSerializer = CreateProcessor(settings),
					};
				result = context.ReadYaml(null, expectedType);
			}

			if (hasDocumentStart)
			{
				reader.Expect<DocumentEnd>();
			}

			if (hasStreamStart)
			{
				reader.Expect<StreamEnd>();
			}

			return result;
		}

        public void Serialize(Stream stream, object value)
        {
            var context = new SerializerContext(this);
            var writer = CreateEmitter(stream, context);
            context.Writer = writer;
			throw new NotImplementedException();
        }

		private IYamlSerializable CreateProcessor(SerializerSettings settings)
		{
            return new AnchorSerializer(new TypingSerializer(new RoutingSerializer(settings)));
		}

        private IEventEmitter CreateEmitter(Stream stream, SerializerContext context)
        {
            return CreateEmitter(new Emitter(new StreamWriter(stream), context.Settings.PreferredIndent), context);
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