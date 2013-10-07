using System;
using YamlDotNet.Events;
using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Serialization.Serializers
{
    /// <summary>
    /// Base class for serializing an object that can be a Yaml !!map or !!seq.
    /// </summary>
    public class ObjectSerializer : IYamlSerializable, IYamlSerializableFactory
    {
		public virtual IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
		{
			// always accept
			return this;
		}
		
		/// <summary>
        /// Checks if a type is a sequence.
        /// </summary>
        /// <param name="typeDescriptor">The type descriptor.</param>
        /// <returns><c>true</c> if a type is a sequence, <c>false</c> otherwise.</returns>
		protected virtual bool CheckIsSequence(ITypeDescriptor typeDescriptor)
		{
			// By default an object serializer is a mapping
			return false;
		}

		protected virtual SequenceStyle GetSequenceStyle(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
		{
			return SequenceStyle.Block;
		}

        public virtual object ReadYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
        {
	        var type = typeDescriptor.Type;

			// When the node is not scalar, we need to instantiate the type directly
			if (value == null && !(typeDescriptor is PrimitiveDescriptor))
			{
				value = context.ObjectFactory.Create(type);
				//if (value == null)
				//{
				//	throw new YamlException(node.Start, node.End, "Unexpected null value");
				//}
			}

            // Get the object accessor for the corresponding class
            var isSequence = CheckIsSequence(typeDescriptor);

			// Process members
	        return isSequence
		                ? ReadItems<SequenceStart, SequenceEnd>(context, value, typeDescriptor)
		                : ReadItems<MappingStart, MappingEnd>(context, value, typeDescriptor);
        }

        protected virtual object ReadItems<TStart, TEnd>(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor) 
            where TStart : NodeEvent
            where TEnd : ParsingEvent
        {
			var reader = context.Reader;
			reader.Expect<TStart>();
			while (!reader.Accept<TEnd>())
			{
				ReadItem(context, thisObject, typeDescriptor);
			}
			reader.Expect<TEnd>();
            return thisObject;
        }

		protected virtual void ReadItem(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
		{
			var reader = context.Reader;

			// For a regular object, the key is expected to be a simple scalar
			var propertyName = reader.Expect<Scalar>().Value;
			var memberAccessor = typeDescriptor[propertyName];

			// Read the value according to the type
			var propertyType = memberAccessor.Type;

			object value = null;
			if (memberAccessor.SerializeMemberMode == SerializeMemberMode.Content)
			{
				value = memberAccessor.Get(thisObject);
			}

			var propertyValue = context.ReadYaml(value, propertyType);

			if (memberAccessor.HasSet && memberAccessor.SerializeMemberMode != SerializeMemberMode.Content)
			{
				memberAccessor.Set(thisObject, propertyValue);
			}
		}

        public virtual void WriteYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
        {
            var typeOfValue = value.GetType();
	        var expectedType = typeDescriptor != null ? typeDescriptor.Type : null;

            var tag = typeOfValue == expectedType ? null : context.TagFromType(typeOfValue);

			if (typeDescriptor == null)
			{
				typeDescriptor = context.FindTypeDescriptor(typeOfValue);
			}

            var isSequence = CheckIsSequence(typeDescriptor);
            if (isSequence)
            {
	            var style = GetSequenceStyle(context, value, typeDescriptor);
                context.Writer.Emit(new SequenceStartEventInfo(value, typeOfValue) { Tag = tag, Anchor = context.GetAnchor(), Style = style});
                WriteItems(context, value, typeDescriptor);
                context.Writer.Emit(new SequenceEndEventInfo(value, typeOfValue));
            }
            else
            {
				context.Writer.Emit(new MappingStartEventInfo(value, typeOfValue) { Tag = tag, Anchor = context.GetAnchor() });
                WriteItems(context, value, typeDescriptor);
                context.Writer.Emit(new MappingEndEventInfo(value, typeOfValue));
            }
        }

		public virtual void WriteItems(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
		{
			foreach (var member in typeDescriptor.Members)
			{
				// Skip any member that we won't serialize
				if (!member.ShouldSerialize(thisObject)) continue;

				// Emit the key name
				WriteKey(context, member.Name);

				var memberValue = member.Get(thisObject);
				var memberType = member.Type;
				context.WriteYaml(memberValue, memberType);
			}
		}

		protected void WriteKey(SerializerContext context, string name)
		{
			// Emit the key name
			context.Writer.Emit(new ScalarEventInfo(name, typeof(string))
			{
				RenderedValue = name,
				IsPlainImplicit = true,
				Style = ScalarStyle.Plain
			});
		}
    }
}