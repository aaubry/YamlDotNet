using System;
using YamlDotNet.Events;
using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Serialization.Processors
{
	public class PrimitiveProcessor : IYamlProcessor
	{
		public object ReadYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
		{
			var primitiveType = (PrimitiveDescriptor) typeDescriptor;
			var type = primitiveType.Type;

			var scalar = context.Reader.Expect<Scalar>();

			// If this is an enum
			if (type.IsEnum)
			{
				return Enum.Parse(type, scalar.Value, false);
			}

			// Else parse using the default schema
			string defaultTag;
			if (!context.Settings.Schema.TryParse(scalar, true, out defaultTag, out value))
			{
				throw new YamlException(scalar.Start, scalar.End, "Unable to decode scalar [{0}] not supported by current schema".DoFormat(scalar));
			}

			return value;
		}

		public void WriteYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
		{
			
		}
	}
}