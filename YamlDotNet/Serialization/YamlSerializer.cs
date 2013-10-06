using System;
using System.IO;
using YamlDotNet.Events;
using YamlDotNet.Serialization.Processors;

namespace YamlDotNet.Serialization
{
    public class YamlSerializer
    {
        private readonly YamlSerializerSettings settings;

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlSerializer"/> class.
		/// </summary>
        public YamlSerializer() : this(null)
        {
        }

		/// <summary>
		/// Gets the settings.
		/// </summary>
		/// <value>The settings.</value>
        public YamlSerializerSettings Settings { get { return settings; } }

        public YamlSerializer(YamlSerializerSettings settings)
        {
            this.settings = settings ?? new YamlSerializerSettings();
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
						ObjectProcessor = new ChainedProcessor(new ObjectProcessor(Settings)),
						PrimitiveProcessor = new PrimitiveProcessor()
					};
				context.ReadYaml = (readValue, readType) => ReadYamlInternal(context, readValue, readType);
				result = ReadYamlInternal(context, null, expectedType);
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

		private object ReadYamlInternal(SerializerContext context, object value, Type expectedType)
		{
			var node = context.Reader.Peek<NodeEvent>();
			if (node == null)
			{
				throw new InvalidOperationException("Unexpected event from input");
			}

			// If expected type is object, set it to null, else use expected
			var type = expectedType == typeof(object) ? null : expectedType;

			// Tries to get a Type from the TagTypes
			var typeFromTag = context.TypeFromTag(node.Tag);

			// Use typeFromTag when type are different
			if (typeFromTag != null && type != typeFromTag && typeFromTag.IsClass && typeFromTag != typeof(string))
				type = typeFromTag;

			// If type is null, use type from tag
			if (type == null)
				type = typeFromTag;

			// Try to decode directly a scalar if types are identical
			if (type == typeFromTag && typeFromTag != null && node is Scalar)
			{
				object scalarValue;
				string defaultScalarTag;
				if (context.TryParseScalar((Scalar) node, out defaultScalarTag, out scalarValue))
				{
					return scalarValue;
				}
			}

			if (type == null && value == null)
			{
				throw new YamlException(node.Start, node.End, "Unable to find a type for this element [{0}]".DoFormat(node));
			}

			if (type == null)
			{
				type = value.GetType();
			}

			var typeDescriptor = context.FindTypeDescriptor(type);

			if (node is Scalar)
			{
				return context.PrimitiveProcessor.ReadYaml(context, value, typeDescriptor);
			}
			
			// Else this is an object
			if (value == null)
			{
				value = context.CreateType(type);
				if (value == null)
				{
					throw new YamlException("Unexpected null value");
				}
			}

			// Call the top level processor
			return context.ObjectProcessor.ReadYaml(context, value, typeDescriptor);
		}
    }
}