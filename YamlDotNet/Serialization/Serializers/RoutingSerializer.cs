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

			// TODO: Handle here user defined/registered IYamlSerializable

			if (typeDescriptor is PrimitiveDescriptor)
			{
				return primitiveSerializer.ReadYaml(context, value, typeDescriptor);
			}
			else if (typeDescriptor is DictionaryDescriptor)
			{
				return dictionarySerializer.ReadYaml(context, value, typeDescriptor);
			}
			else if (typeDescriptor is CollectionDescriptor)
			{
				return collectionSerializer.ReadYaml(context, value, typeDescriptor);
			}
            else if (typeDescriptor is ArrayDescriptor)
            {
                return arraySerializer.ReadYaml(context, value, typeDescriptor);
            }

			return defaultObjectSerializer.ReadYaml(context, value, typeDescriptor);
		}

		public void WriteYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
		{
			throw new System.NotImplementedException();
		}
	}
}