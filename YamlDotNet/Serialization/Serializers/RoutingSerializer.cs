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

		public ValueResult ReadYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
		{
			var serializer = GetSerializer(context, typeDescriptor);
			return serializer.ReadYaml(context, value, typeDescriptor);
		}

		public void WriteYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
		{
			// If value is null, then just output a plain null scalar
			if (value == null)
			{
				context.Writer.Emit(new ScalarEventInfo(null, typeof(object)) { RenderedValue = "null", IsPlainImplicit = true, Style = ScalarStyle.Plain});
				return;
			}

			// If TypeDescriptor is null, typeof(object) or an interface, use the serializer of the actual value
			var localTypeDescriptor = typeDescriptor == null || typeDescriptor.Type == typeof (object) || typeDescriptor.Type.IsInterface
				                          ? context.FindTypeDescriptor(value.GetType())
				                          : typeDescriptor;

			var serializer = GetSerializer(context, localTypeDescriptor);
			serializer.WriteYaml(context, value, typeDescriptor);
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