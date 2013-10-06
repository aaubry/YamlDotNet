using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Serialization.Processors
{
	internal class RoutingProcessor : IYamlProcessor
	{
		private readonly PrimitiveProcessor primitiveProcessor;
		private readonly DictionaryProcessor dictionaryProcessor;
		private readonly CollectionProcessor collectionProcessor;
		private readonly ObjectProcessor defaultObjectProcessor;

		public RoutingProcessor(YamlSerializerSettings settings)
		{
			primitiveProcessor = new PrimitiveProcessor();
			dictionaryProcessor = new DictionaryProcessor(settings);
			collectionProcessor = new CollectionProcessor(settings);
			defaultObjectProcessor = new ObjectProcessor(settings);
		}

		public object ReadYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
		{

			// TODO: Handle here user defined/registered IYamlProcessor

			if (typeDescriptor is PrimitiveDescriptor)
			{
				return primitiveProcessor.ReadYaml(context, value, typeDescriptor);
			}
			else if (typeDescriptor is DictionaryDescriptor)
			{
				return dictionaryProcessor.ReadYaml(context, value, typeDescriptor);
			}
			else if (typeDescriptor is CollectionDescriptor)
			{
				return collectionProcessor.ReadYaml(context, value, typeDescriptor);
			}

			return defaultObjectProcessor.ReadYaml(context, value, typeDescriptor);
		}

		public void WriteYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
		{
			throw new System.NotImplementedException();
		}
	}
}