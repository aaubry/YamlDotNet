using System;

namespace YamlDotNet.Serialization.Processors
{
	internal class CollectionProcessor : ObjectProcessor
	{
		public CollectionProcessor(YamlSerializerSettings settings) : base(settings)
		{
		}


		protected override void ReadItem(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
		{
			throw new NotImplementedException();
			base.ReadItem(context, thisObject, typeDescriptor);
		}

		// TODO implement Collection Processor
	}
}