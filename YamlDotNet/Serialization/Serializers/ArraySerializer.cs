using System.Collections;
using YamlDotNet.Events;
using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Serialization.Serializers
{
    internal class ArraySerializer : IYamlSerializable
    {
	    private SerializerSettings settings;

	    public ArraySerializer(SerializerSettings settings)
	    {
		    this.settings = settings;
	    }

	    public virtual object ReadYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
		{
			var reader = context.Reader;
			var arrayDescriptor = (ArrayDescriptor)typeDescriptor;

			bool isArray = value != null && value.GetType().IsArray;
			var arrayList = isArray ? (IList)value : arrayDescriptor.CreateListType();

			reader.Expect<SequenceStart>();
			if (isArray)
			{
				int index = 0;
				while (!reader.Accept<SequenceEnd>())
				{
					var node = reader.Peek<ParsingEvent>();
					if (index >= arrayList.Count)
					{
						throw new YamlException(node.Start, node.End, "Unable to deserialize array. Current number of elements [{0}] exceeding array size [{1}]".DoFormat(index, arrayList.Count));
					}

					arrayList[index++] = context.ReadYaml(null, arrayDescriptor.ElementType);
				}
			}
			else
			{
				while (!reader.Accept<SequenceEnd>())
				{
					arrayList.Add(context.ReadYaml(null, arrayDescriptor.ElementType));
				}
			}
			reader.Expect<SequenceEnd>();

			return isArray ? arrayList : arrayDescriptor.ToArray(arrayList);
		}

	    public void WriteYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
	    {
			var arrayDescriptor = (ArrayDescriptor)typeDescriptor;

		    var valueType = value.GetType();
		    var arrayList = (IList) value;

			var tag = valueType != typeDescriptor.Type ? context.TagFromType(valueType) : null;

			// Emit a Flow sequence or block sequence depending on settings 
		    context.Writer.Emit(new SequenceStartEventInfo(value, valueType)
			    {
				    Tag = tag,
				    Anchor = context.GetAnchor(),
				    Style = arrayList.Count < settings.LimitFlowSequence ? SequenceStyle.Flow : SequenceStyle.Block
			    });

			foreach (var element in arrayList)
			{
				context.WriteYaml(element, arrayDescriptor.ElementType);
			}
			context.Writer.Emit(new SequenceEndEventInfo(value, valueType));
	    }
    }
}