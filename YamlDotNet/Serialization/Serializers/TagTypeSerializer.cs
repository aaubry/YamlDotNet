using System;
using System.Collections.Generic;
using YamlDotNet.Events;
using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Serialization.Serializers
{
	internal class TagTypeSerializer : ChainedSerializer
	{
		public TagTypeSerializer(IYamlSerializable next) : base(next)
		{
		}

		public override ValueOutput ReadYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
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
				return new ValueOutput(value);
			}

			// If type is null or equal to typeof(object) and value is null
			// and we have a node starting with a Sequence or Mapping
			// Set the type to accept IList<object> for sequences
			// or IDictionary<object, object> for mappings
			// This allow to load any YAML documents into dictionary/list
			// automatically
			if ((type == null || type == typeof(object)) && value == null)
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

			// If this is a nullable descriptor, use its underlying type directly
			if (typeDescriptor is NullableDescriptor)
			{
				typeDescriptor = context.FindTypeDescriptor(((NullableDescriptor) typeDescriptor).UnderlyingType);
			}
			return base.ReadYaml(context, value, typeDescriptor);
		}

		public override void WriteYaml(SerializerContext context, ValueInput input, ITypeDescriptor typeDescriptor)
		{
			var value = input.Value;

			// If value is null, then just output a plain null scalar
			if (value == null)
			{
				context.Writer.Emit(new ScalarEventInfo(null, typeof(object)) { RenderedValue = "null", IsPlainImplicit = true, Style = ScalarStyle.Plain });
				return;
			}

			var typeOfValue = value.GetType();


			// If we have a nullable value, get its type directly and replace the descriptor
			if (typeDescriptor is NullableDescriptor)
			{
				typeDescriptor = context.FindTypeDescriptor(((NullableDescriptor) typeDescriptor).UnderlyingType);
			}
			
			// Expected type 
			var expectedType = typeDescriptor != null ? typeDescriptor.Type : null;
			bool isAutoMapSeq = false;

			// Allow to serialize back to plain YAML !!map and !!seq if the expected type is an object
			// and the value is of the type Dictionary<object, object> or List<object>
			if (expectedType == typeof(object))
			{
				if (typeOfValue == typeof (Dictionary<object, object>) || typeOfValue == typeof(List<object>))
				{
					isAutoMapSeq = true;
				}
			}

			// Auto !!map !!seq for collections/dictionaries
			var defaultImplementationType = DefaultObjectFactory.GetDefaultImplementation(expectedType);
			if (defaultImplementationType != null && defaultImplementationType == typeOfValue)
			{
				isAutoMapSeq = true;
			}

			// If this is an anonymous tag we will serialize only a default untyped YAML mapping
			var tag = typeOfValue.IsAnonymous() || typeOfValue == expectedType || isAutoMapSeq
				          ? null
				          : context.TagFromType(typeOfValue);

			// Set the tag
			input.Tag = tag;

			// We will use the type of the value for the rest of the WriteYaml serialization
			typeDescriptor = context.FindTypeDescriptor(typeOfValue);
			
			// Go next to the chain
			base.WriteYaml(context, input, typeDescriptor);
		}
	}
}