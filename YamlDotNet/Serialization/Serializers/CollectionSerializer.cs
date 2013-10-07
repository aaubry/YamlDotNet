using System;

namespace YamlDotNet.Serialization.Serializers
{
	internal class CollectionSerializer : ObjectSerializer
	{
		public CollectionSerializer(SerializerSettings settings) : base(settings)
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