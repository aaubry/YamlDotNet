using System;
using YamlDotNet.Events;
using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Serialization.Serializers
{
	internal class NullableSerializer : IYamlSerializable, IYamlSerializableFactory
	{
		public IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
		{
			return typeDescriptor is NullableDescriptor ? this : null;
		}

		public ValueResult ReadYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
		{
			var nullableDescriptor = (NullableDescriptor) typeDescriptor;

			var subTypeDescriptor = context.FindTypeDescriptor(nullableDescriptor.UnderlyingType);

			return context.ReadYaml(null, subTypeDescriptor.Type);
		}

		public void WriteYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
		{
			var nullableDescriptor = (NullableDescriptor)typeDescriptor;
			context.WriteYaml(value, nullableDescriptor.UnderlyingType);
		}

	}
}