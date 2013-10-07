using System;
using System.Collections.Generic;

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

		public object ReadYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
		{
			return GetSerializer(context, typeDescriptor).ReadYaml(context, value, typeDescriptor);
		}

		public void WriteYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
		{
			// If value is null, then just output a plain null scalar
			if (value == null)
			{
				context.Writer.Emit(new ScalarEventInfo(null, typeof(object)) { RenderedValue = "null", IsPlainImplicit = true, Style = ScalarStyle.Plain});
				return;
			}

			var localTypeDescriptor = typeDescriptor ?? context.FindTypeDescriptor(value.GetType());
			GetSerializer(context, localTypeDescriptor).WriteYaml(context, value, typeDescriptor);
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