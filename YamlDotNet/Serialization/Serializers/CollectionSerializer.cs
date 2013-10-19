using System;
using System.Collections;
using YamlDotNet.Events;
using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Serialization.Serializers
{
	internal class CollectionSerializer : ObjectSerializer
	{
		public CollectionSerializer()
		{
		}

		public override IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
		{
			return typeDescriptor is CollectionDescriptor ? this : null;
		}

		protected override bool CheckIsSequence(ITypeDescriptor typeDescriptor)
		{
			var collectionDescriptor = (CollectionDescriptor)typeDescriptor;

			// If the dictionary is pure, we can directly output a sequence instead of a mapping
			return collectionDescriptor.IsPureCollection || collectionDescriptor.HasOnlyCapacity;
		}

		public override void ReadItem(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
		{
			var collectionDescriptor = (CollectionDescriptor)typeDescriptor;

			if (CheckIsSequence(collectionDescriptor))
			{
				ReadPureCollectionItems(context, thisObject, typeDescriptor);
			}
			else
			{
				var keyEvent = context.Reader.Peek<Scalar>();
				if (keyEvent != null)
				{
					if (keyEvent.Value == context.Settings.SpecialCollectionMember)
					{
						var reader = context.Reader;
						reader.Parser.MoveNext();

						// Read inner sequence
						reader.Expect<SequenceStart>();
						ReadPureCollectionItems(context, thisObject, typeDescriptor);
						reader.Expect<SequenceEnd>();
						return;
					}
				}

				base.ReadItem(context, thisObject, typeDescriptor);
			}
		}

		protected override YamlStyle GetStyle(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
		{
			var style = base.GetStyle(context, thisObject, typeDescriptor);

			// In case of any style, allow to emit a flow sequence depending on Settings LimitPrimitiveFlowSequence.
			// Apply this only for primitives
			if (style == YamlStyle.Any)
			{
				bool isPrimitiveElementType = false;
				var collectionDescriptor = typeDescriptor as CollectionDescriptor;
				int count = 0;
				if (collectionDescriptor != null)
				{
					isPrimitiveElementType = PrimitiveDescriptor.IsPrimitive(collectionDescriptor.ElementType);
					count = collectionDescriptor.GetCollectionCount(thisObject);
				}
				else
				{
					var arrayDescriptor = typeDescriptor as ArrayDescriptor;
					if (arrayDescriptor != null)
					{
						isPrimitiveElementType = PrimitiveDescriptor.IsPrimitive(arrayDescriptor.ElementType);
						count = thisObject != null ? ((Array) thisObject).Length : -1;
					}
				}

				style = thisObject == null || count >= context.Settings.LimitPrimitiveFlowSequence || !isPrimitiveElementType
					       ? YamlStyle.Block
					       : YamlStyle.Flow;
			}

			return style;
		}

		public override void WriteItems(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
		{
			var collectionDescriptor = (CollectionDescriptor)typeDescriptor;
			if (CheckIsSequence(collectionDescriptor))
			{
				WritePureCollectionItems(context, thisObject, typeDescriptor);
			}
			else
			{
				// Serialize Dictionary members
				foreach (var member in typeDescriptor.Members)
				{
					if (member.Name == "Capacity" && !context.Settings.EmitCapacityForList)
					{
						continue;
					}

					// Emit the key name
					WriteKey(context, member.Name);

					var memberValue = member.Get(thisObject);
					var memberType = member.Type;

					context.PushStyle(member.Style);
					context.WriteYaml(memberValue, memberType);
				}

				WriteKey(context, context.Settings.SpecialCollectionMember);

				context.Writer.Emit(new SequenceStartEventInfo(thisObject, thisObject.GetType()));
				WritePureCollectionItems(context, thisObject, typeDescriptor);
				context.Writer.Emit(new SequenceEndEventInfo(thisObject, thisObject.GetType()));
			}
		}

		private void ReadPureCollectionItems(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
		{
			var collectionDescriptor = (CollectionDescriptor)typeDescriptor;
			if (!collectionDescriptor.HasAdd)
			{
				throw new InvalidOperationException("Cannot deserialize list to type [{0}]. No Add method found".DoFormat(thisObject.GetType()));
			}
			if (collectionDescriptor.IsReadOnly(thisObject))
			{
				throw new InvalidOperationException("Cannot deserialize list to readonly collection type [{0}].".DoFormat(thisObject.GetType()));
			}

			var reader = context.Reader;

			while (!reader.Accept<SequenceEnd>())
			{
				var valueResult = context.ReadYaml(null, collectionDescriptor.ElementType);
	
				// Handle aliasing. TODO: Aliasing doesn't preserve order here. This is not an expected behavior
				if (valueResult.IsAlias)
				{
					context.AddAliasBinding(valueResult.Alias, deferredValue => collectionDescriptor.CollectionAdd(thisObject, deferredValue));
				}
				else
				{
					collectionDescriptor.CollectionAdd(thisObject, valueResult.Value);
				}
			}
		}

		private void WritePureCollectionItems(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
		{
			var collection = (IEnumerable)thisObject;
			var collectionDescriptor = (CollectionDescriptor)typeDescriptor;

			foreach (var item in collection)
			{
				context.WriteYaml(item, collectionDescriptor.ElementType);
			}
		}
	}
}