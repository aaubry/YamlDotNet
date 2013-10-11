using System;
using YamlDotNet.Events;

namespace YamlDotNet.Serialization.Serializers
{
	public abstract class ScalarSerializerBase : IYamlSerializable
	{
		public ValueOutput ReadYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
		{
			var scalar = context.Reader.Expect<Scalar>();
			return new ValueOutput(ConvertFrom(context, value, scalar, typeDescriptor));
		}

		public abstract object ConvertFrom(SerializerContext context, object value, Scalar fromScalar, ITypeDescriptor typeDescriptor);

		public void WriteYaml(SerializerContext context, ValueInput input, ITypeDescriptor typeDescriptor)
		{
			var value = input.Value;
			var typeOfValue = value.GetType();

			var isSchemaImplicitTag = context.Schema.IsTagImplicit(input.Tag);
			var scalar = new ScalarEventInfo(value, typeOfValue)
				{
					IsPlainImplicit = isSchemaImplicitTag,
					Style = ScalarStyle.Plain,
					Anchor = context.GetAnchor(),
					Tag = input.Tag,
				};


			// Parse default types 
			switch (Type.GetTypeCode(typeOfValue))
			{
				case TypeCode.Object:
				case TypeCode.String:
				case TypeCode.Char:
					scalar.Style = ScalarStyle.Any;
					break;
			}

			scalar.RenderedValue =  ConvertTo(context, value, typeDescriptor);

			// Emit the scalar
			context.Writer.Emit(scalar);
		}

		public abstract string ConvertTo(SerializerContext context, object value, ITypeDescriptor typeDescriptor);
	}
}