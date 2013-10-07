using System;
using YamlDotNet.Events;
using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Serialization.Serializers
{
    /// <summary>
    /// Base class for serializing an object that can be a Yaml !!map or !!seq.
    /// </summary>
    public abstract class ObjectSerializerBase : IYamlSerializable
    {
        private readonly SerializerSettings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectSerializer"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <exception cref="System.ArgumentNullException">settings</exception>
        protected ObjectSerializerBase(SerializerSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            this.settings = settings;
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <value>The settings.</value>
        public SerializerSettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Checks if a type is a sequence.
        /// </summary>
        /// <param name="typeDescriptor">The type descriptor.</param>
        /// <returns><c>true</c> if a type is a sequence, <c>false</c> otherwise.</returns>
        protected abstract bool CheckIsSequence(ITypeDescriptor typeDescriptor);


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

            if (isSequence)
            {
                value = ReadItems<SequenceStart,SequenceEnd>(context, value, typeDescriptor);
            }
            else
            {
                value = ReadItems<MappingStart, MappingEnd>(context, value, typeDescriptor);
            }
            return value;
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

        protected abstract void ReadItem(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor);

        public virtual void WriteYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
        {
            var typeOfValue = value.GetType();
            var tag = typeOfValue == typeDescriptor.Type ? null : context.TagFromType(typeOfValue);

            var isSequence = CheckIsSequence(typeDescriptor);
            if (isSequence)
            {
                context.Writer.Emit(new SequenceStartEventInfo(value, typeOfValue) { Tag = tag });
                WriteItems(context, value, typeDescriptor);
                context.Writer.Emit(new SequenceEndEventInfo(value, typeOfValue));
            }
            else
            {
                context.Writer.Emit(new MappingStartEventInfo(value, typeOfValue) { Tag = tag });
                WriteItems(context, value, typeDescriptor);
                context.Writer.Emit(new MappingEndEventInfo(value, typeOfValue));
            }
        }

        public abstract void WriteItems(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor);
    }
}