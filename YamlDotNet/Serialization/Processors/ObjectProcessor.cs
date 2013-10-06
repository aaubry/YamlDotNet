using System;
using YamlDotNet.Events;

namespace YamlDotNet.Serialization.Processors
{
	/// <summary>
	/// Default processor for serializing an object.
	/// </summary>
	public class ObjectProcessor : IYamlProcessor
	{
		private readonly YamlSerializerSettings settings;

		/// <summary>
		/// Initializes a new instance of the <see cref="ObjectProcessor"/> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <exception cref="System.ArgumentNullException">settings</exception>
		public ObjectProcessor(YamlSerializerSettings settings)
		{
			if (settings == null) throw new ArgumentNullException("settings");
			this.settings = settings;
		}

		/// <summary>
		/// Gets the settings.
		/// </summary>
		/// <value>The settings.</value>
		public YamlSerializerSettings Settings
		{
			get { return settings; }
		}

		/// <summary>
		/// Checks if a type is a sequence.
		/// </summary>
		/// <param name="typeDescriptor">The type descriptor.</param>
		/// <returns><c>true</c> if a type is a sequence, <c>false</c> otherwise.</returns>
		protected virtual bool CheckIsSequence(ITypeDescriptor typeDescriptor)
		{
			// By default return false
			return false;
		}

		public virtual object ReadYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
		{
			// Get the object accessor for the corresponding class
			var isSequence = CheckIsSequence(typeDescriptor);

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