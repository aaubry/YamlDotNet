using System;
using System.IO;
using YamlDotNet.Events;

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

		public object Deserialize(Stream stream)
		{
			var context = new SerializerContext(this);
			context.ReadYaml = (readValue, type) => ReadYamlInternal(context, readValue, type);
			throw new NotImplementedException();
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


		private object ReadYamlInternal(SerializerContext context, object value, Type expectedType)
		{
			var node = context.Reader.Peek<NodeEvent>();
			if (node == null)
			{
				throw new InvalidOperationException("Unexpected event from input");
			}

			// If expected type is object, set it to null, else use expected
			var type = expectedType == typeof(object) ? null : expectedType;

			// Tries to get a Type from the TagTypeRegistry
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

			// handle type here.
			throw new NotImplementedException();
		}
    }
}