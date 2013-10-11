using System;
using System.Collections.Generic;
using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Serialization.Serializers
{
	/// <summary>
	/// This serializer is responsible to route to a specific serializer.
	/// </summary>
	internal class RoutingSerializer : IYamlSerializable
	{
		private readonly Dictionary<Type, IYamlSerializable> serializers = new Dictionary<Type, IYamlSerializable>();
		private readonly List<IYamlSerializableFactory> factories = new List<IYamlSerializableFactory>();

		public RoutingSerializer()
		{
		}

		public void AddSerializer(Type type, IYamlSerializable serializer)
		{
			serializers[type] = serializer;
		}

		public void AddSerializerFactory(IYamlSerializableFactory factory)
		{
			factories.Add(factory);
		}

		public ValueOutput ReadYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
		{
			// If value is not null, use its TypeDescriptor otherwise use expected type descriptor
			var typeDescriptorOfValue = value != null ? context.FindTypeDescriptor(value.GetType()) : typeDescriptor;
			var serializer = GetSerializer(context, typeDescriptorOfValue);
			return serializer.ReadYaml(context, value, typeDescriptor);
		}

		public void WriteYaml(SerializerContext context, ValueInput input, ITypeDescriptor typeDescriptor)
		{
			var serializer = GetSerializer(context, typeDescriptor);
			serializer.WriteYaml(context, input, typeDescriptor);
		}

		private IYamlSerializable GetSerializer(SerializerContext context, ITypeDescriptor typeDescriptor)
		{
			IYamlSerializable serializer;
			if (!serializers.TryGetValue(typeDescriptor.Type, out serializer))
			{
				foreach (var factory in factories)
				{
					serializer = factory.TryCreate(context, typeDescriptor);
					if (serializer != null)
					{
						break;
					}
				}
			}

			if (serializer == null)
			{
				throw new InvalidOperationException("Unable to find a serializer for the type [{0}]".DoFormat(typeDescriptor.Type));
			}

			return serializer;
		}
	}
}