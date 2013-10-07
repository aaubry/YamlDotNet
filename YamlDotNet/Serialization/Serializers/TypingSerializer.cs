using YamlDotNet.Events;
using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Serialization.Serializers
{
    internal class TypingSerializer : ChainedSerializer
    {
        public TypingSerializer(IYamlSerializable next) : base(next)
        {
        }

        public override object ReadYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
        {
            var parsingEvent = context.Reader.Peek<ParsingEvent>();
            // Can this happen here?
            if (parsingEvent == null)
            {
                // TODO check how to put a location in this case?
                throw new YamlException("Unable to parse input");
            }

            var node = parsingEvent as NodeEvent;
            if (node == null)
            {
                throw new YamlException(parsingEvent.Start, parsingEvent.End, "Unexpected parsing event found [{0}]. Expecting Scalar, Mapping or Sequence".DoFormat(parsingEvent));
            }

            var expectedType = typeDescriptor != null ? typeDescriptor.Type : null;

            // If expected type is object, set it to null, else use expected
            var type = expectedType == typeof(object) ? null : expectedType;

            // Tries to get a Type from the TagTypes
            var typeFromTag = context.TypeFromTag(node.Tag);

            // Use typeFromTag when type are different
            if (typeFromTag != null && type != typeFromTag && typeFromTag.IsClass && typeFromTag != typeof(string))
                type = typeFromTag;

            // If type is null, use type from tag
            if (type == null)
                type = typeFromTag;

            if (type == null && value == null)
            {
                throw new YamlException(node.Start, node.End, "Unable to find a type for this element [{0}]".DoFormat(node));
            }

            if (type == null)
            {
                type = value.GetType();
            }

            typeDescriptor = context.FindTypeDescriptor(type);

            return base.ReadYaml(context, value, typeDescriptor);
        }
    }
}