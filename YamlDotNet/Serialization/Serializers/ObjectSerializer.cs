using YamlDotNet.Events;
using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Serialization.Serializers
{
	/// <summary>
	/// Base class for serializing an object that can be a Yaml !!map or !!seq.
	/// </summary>
	public class ObjectSerializer : IYamlSerializable, IYamlSerializableFactory
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ObjectSerializer"/> class.
		/// </summary>
		public ObjectSerializer()
		{
		}

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

		protected virtual YamlStyle GetStyle(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
		{
			return typeDescriptor.Style;
		}

		public virtual ValueOutput ReadYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
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
			return new ValueOutput(isSequence
						? ReadItems<SequenceStart, SequenceEnd>(context, value, typeDescriptor)
						: ReadItems<MappingStart, MappingEnd>(context, value, typeDescriptor));
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

		public virtual void ReadItem(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
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

			var valueResult = context.ReadYaml(value, propertyType);

			// Handle late binding
			if (memberAccessor.HasSet && memberAccessor.SerializeMemberMode != SerializeMemberMode.Content)
			{
				// If result value is a late binding, register it.
				if (valueResult.IsAlias)
				{
					context.AddAliasBinding(valueResult.Alias, lateValue => memberAccessor.Set(thisObject, lateValue));
				}
				else
				{
					memberAccessor.Set(thisObject, valueResult.Value);
				}
			}
		}

		public virtual void WriteYaml(SerializerContext context, ValueInput input, ITypeDescriptor typeDescriptor)
		{
			var value = input.Value;
			var typeOfValue = value.GetType();

			var isSequence = CheckIsSequence(typeDescriptor);

			// Resolve the style, use default style if not defined.
			var style = ResolveStyle(context, value, typeDescriptor);

			if (isSequence)
			{
				context.Writer.Emit(new SequenceStartEventInfo(value, typeOfValue) { Tag = input.Tag, Anchor = context.GetAnchor(), Style = style});
				WriteItems(context, value, typeDescriptor);
				context.Writer.Emit(new SequenceEndEventInfo(value, typeOfValue));
			}
			else
			{
				context.Writer.Emit(new MappingStartEventInfo(value, typeOfValue) { Tag = input.Tag, Anchor = context.GetAnchor(), Style = style });
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

				// In case of serializing a property/field which is not writeable
				// we need to change the expected type to the actual type of the 
				// content value
				if (member.SerializeMemberMode == SerializeMemberMode.Content)
				{
					if (memberValue != null)
					{
						memberType = memberValue.GetType();
					}
				}

				// Push the style of the current member
				context.PushStyle(member.Style);
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

		private YamlStyle ResolveStyle(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
		{
			// Resolve the style, use default style if not defined.
			// First pop style of current member being serialized.
			var style = context.PopStyle();

			// If a dynamic style format is found, try to resolve through it
			if (context.Settings.DynamicStyleFormat != null)
			{
				var dynamicStyle = context.Settings.DynamicStyleFormat.GetStyle(context, value, typeDescriptor);
				if (dynamicStyle != YamlStyle.Any)
				{
					style = dynamicStyle;
				}
			}

			// If no style yet defined
			if (style == YamlStyle.Any)
			{
				// Try to get the style from this serializer
				style = GetStyle(context, value, typeDescriptor);

				// If not defined, get the default style
				if (style == YamlStyle.Any)
				{
					style = context.Settings.DefaultStyle;

					// If default style is set to Any, set it to Block by default.
					if (style == YamlStyle.Any)
					{
						style = YamlStyle.Block;
					}
				}
			}

			return style;
		}
	}
}