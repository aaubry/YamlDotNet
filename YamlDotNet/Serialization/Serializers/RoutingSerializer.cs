using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Serialization.Serializers
{
    /// <summary>
    /// TODO: this class is temporary, we should make it more configurable
    /// </summary>
	internal class RoutingSerializer : IYamlSerializable
	{
		private readonly PrimitiveSerializer primitiveSerializer;
		private readonly DictionarySerializer dictionarySerializer;
		private readonly CollectionSerializer collectionSerializer;
        private readonly ArraySerializer arraySerializer;
        private readonly ObjectSerializer defaultObjectSerializer;

        public RoutingSerializer(SerializerSettings settings)
		{
			primitiveSerializer = new PrimitiveSerializer();
			dictionarySerializer = new DictionarySerializer(settings);
			collectionSerializer = new CollectionSerializer(settings);
			defaultObjectSerializer = new ObjectSerializer(settings);
            arraySerializer = new ArraySerializer(settings);
		}

		public object ReadYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
		{
			return GetSerializer(typeDescriptor).ReadYaml(context, value, typeDescriptor);
		}

		public void WriteYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
		{
			// If value is null, then just output a plain null scalar
			if (value == null)
			{
				context.Writer.Emit(new ScalarEventInfo(null, typeof(object)) { RenderedValue = string.Empty, IsPlainImplicit = true, Style = ScalarStyle.Plain});
				return;
			}

			var localTypeDescriptor = typeDescriptor ?? context.FindTypeDescriptor(value.GetType());
			GetSerializer(localTypeDescriptor).WriteYaml(context, value, typeDescriptor);
		}

		private IYamlSerializable GetSerializer(ITypeDescriptor typeDescriptor)
		{
			if (typeDescriptor is PrimitiveDescriptor)
			{
				return primitiveSerializer;
			}
			else if (typeDescriptor is DictionaryDescriptor)
			{
				return dictionarySerializer;
			}
			else if (typeDescriptor is CollectionDescriptor)
			{
				return collectionSerializer;
			}
			else if (typeDescriptor is ArrayDescriptor)
			{
				return arraySerializer;
			}

			return defaultObjectSerializer;
		}
	}
}