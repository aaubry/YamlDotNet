using System;
using System.Collections.Generic;
using YamlDotNet.Events;

namespace YamlDotNet.Serialization.Serializers
{
	internal class TypingSerializer : ChainedSerializer
	{
		public TypingSerializer(IYamlSerializable next) : base(next)
		{
		}

		public override ValueResult ReadYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
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

			var type = typeDescriptor != null ? typeDescriptor.Type : null;

			// Tries to get a Type from the TagTypes
			Type typeFromTag = null;
			if (!string.IsNullOrEmpty(node.Tag))
			{
				typeFromTag = context.TypeFromTag(node.Tag);
				if (typeFromTag == null)
				{
					throw new YamlException(parsingEvent.Start, parsingEvent.End, "Unable to resolve tag [{0}] to type from tag resolution or registered assemblies".DoFormat(node.Tag));
				}
			}

			// Use typeFromTag when type are different
			if (typeFromTag != null && type != typeFromTag && typeFromTag.IsClass && typeFromTag != typeof(string))
				type = typeFromTag;

			// If type is null, use type from tag
			if (type == null)
				type = typeFromTag;

			// Handle explicit null scalar
			if (node is Scalar && context.Schema.TryParse((Scalar) node, typeof (object), out value))
			{
				// The value was pick up, go to next
				context.Reader.Parser.MoveNext();
				return new ValueResult(value);
			}

			// If type and value are nulls
			if (type == null && value == null)
			{
				// If the node is a sequence start, fallback to a IList<object>
				if (node is SequenceStart)
				{
					type = typeof (IList<object>);
				}
				else if (node is MappingStart)
				{
					// If the node is a mapping start, fallback to a IDictionary<object, object>
					type = typeof(IDictionary<object, object>);
				}
			}

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