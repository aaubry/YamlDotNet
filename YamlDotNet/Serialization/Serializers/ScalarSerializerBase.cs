using YamlDotNet.Events;

namespace YamlDotNet.Serialization.Serializers
{
    public abstract class ScalarSerializerBase : IYamlSerializable
    {
        public ValueResult ReadYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
        {
            var scalar = context.Reader.Expect<Scalar>();
            return new ValueResult(ConvertFrom(context, value, scalar, typeDescriptor));
        }

        public abstract object ConvertFrom(SerializerContext context, object value, Scalar fromScalar, ITypeDescriptor typeDescriptor);

        public void WriteYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
        {
            var valueType = value != null ? value.GetType() : typeDescriptor != null ? typeDescriptor.Type : null;

            var scalar = new ScalarEventInfo(value, valueType)
                {
                    IsPlainImplicit = true,
                    Style = ScalarStyle.Plain,
                    Anchor = context.GetAnchor()
                };

            if (typeDescriptor == null)
            {
                typeDescriptor = context.FindTypeDescriptor(valueType);
            }

            ConvertTo(context, value, scalar, typeDescriptor);

            // Emit the scalar
            context.Writer.Emit(scalar);
        }

        public abstract void ConvertTo(SerializerContext context, object value, ScalarEventInfo toScalar, ITypeDescriptor typeDescriptor);
    }
}