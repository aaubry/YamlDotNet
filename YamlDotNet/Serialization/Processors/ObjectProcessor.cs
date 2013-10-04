using System;
using YamlDotNet.Events;

namespace YamlDotNet.Serialization
{
	public class ObjectProcessor : IYamlProcessor
	{
		private readonly YamlSerializerSettings settings;

		public ObjectProcessor(YamlSerializerSettings settings)
		{
			if (settings == null) throw new ArgumentNullException("settings");
			this.settings = settings;
		}

		public YamlSerializerSettings Settings
		{
			get { return settings; }
		}

		protected virtual bool IsSequence(ITypeDescriptor typeDescriptor)
		{
			return false;
		}

		public virtual object ReadYaml(SerializerContext context, object value, Type expectedType)
		{
			var reader = context.Reader;
			var nodeEvent = reader.Peek<NodeEvent>();

			var type = context.TypeFromTag(nodeEvent.Tag) ?? (value != null ? value.GetType() : expectedType);
			if (type == null)
			{
				throw new YamlException("Unable to find type for mapping [{0}]".DoFormat(nodeEvent));
			}

			if (value == null)
			{
				value = context.CreateType(type);
				if (value == null)
				{
					throw new YamlException("Unexpected null value");
				}
			}

			// Get the object accessor for the corresponding class
			var typeDescriptor = context.TypeDescriptorFactory.Find(value.GetType());
			var isSequence = IsSequence(typeDescriptor);

			if (isSequence)
			{
				ReadItems<SequenceStart,SequenceEnd>(context, value, typeDescriptor);
			}
			else
			{
				ReadItems<MappingStart, MappingEnd>(context, value, typeDescriptor);
			}
			return value;
		}

		protected virtual void ReadItems<TStart, TEnd>(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor) 
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

		public virtual void WriteYaml(SerializerContext context, object value, Type type)
		{
			var typeOfValue = value.GetType();
			var tag = typeOfValue == type ? null : context.TagFromType(typeOfValue);

			// Get the object accessor for the corresponding class
			var typeDescriptor = context.TypeDescriptorFactory.Find(typeOfValue);

			var isSequence = IsSequence(typeDescriptor);

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

		public virtual void WriteItems(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
		{
			foreach (var member in typeDescriptor.Members)
			{
				// Skip any member that we won't serialize
				if (!member.ShouldSerialize(thisObject)) continue;

				// Emit the key name
				context.Writer.Emit(new ScalarEventInfo(member.Name, typeof (string)));

				var memberValue = member.Get(thisObject);
				var memberType = member.Type;
				context.WriteYaml(memberValue, memberType);
			}
		}
	}
}