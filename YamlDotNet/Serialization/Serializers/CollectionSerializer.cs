using System.Collections;
using YamlDotNet.Events;
using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Serialization.Serializers
{
	internal class CollectionSerializer : ObjectSerializer
	{
		private readonly PureCollectionSerializer pureCollectionSerializer;

		public CollectionSerializer()
		{
			pureCollectionSerializer = new PureCollectionSerializer();
		}

		public override IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
		{
			return typeDescriptor is CollectionDescriptor ? this : null;
		}

		protected override bool CheckIsSequence(ITypeDescriptor typeDescriptor)
		{
			var dictionaryDescriptor = (CollectionDescriptor)typeDescriptor;

			// If the dictionary is pure, we can directly output a sequence instead of a mapping
			return dictionaryDescriptor.IsPureCollection || dictionaryDescriptor.HasOnlyCapacity;
		}

		public override void ReadItem(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
		{
			var collectionDescriptor = (CollectionDescriptor)typeDescriptor;

			if (CheckIsSequence(collectionDescriptor))
			{
				pureCollectionSerializer.ReadItem(context, thisObject, typeDescriptor);
			}
			else
			{
				var keyEvent = context.Reader.Peek<Scalar>();
				if (keyEvent != null)
				{
					if (keyEvent.Value == context.Settings.SpecialCollectionMember)
					{
						context.Reader.Accept<Scalar>();
						pureCollectionSerializer.ReadYaml(context, thisObject, context.FindTypeDescriptor(thisObject.GetType()));
						return;
					}
				}

				base.ReadItem(context, thisObject, typeDescriptor);
			}
		}

		protected override SequenceStyle GetSequenceStyle(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
		{
			var collection = (ICollection) thisObject;
			return collection.Count < context.Settings.LimitFlowSequence ? SequenceStyle.Flow : SequenceStyle.Block;
		}

		public override void WriteItems(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
		{
			var collectionDescriptor = (CollectionDescriptor)typeDescriptor;
			if (CheckIsSequence(collectionDescriptor))
			{
				pureCollectionSerializer.WriteItems(context, thisObject, typeDescriptor);
			}
			else
			{
				// Serialize Dictionary members
				foreach (var member in typeDescriptor.Members)
				{
					// Emit the key name
					WriteKey(context, member.Name);

					var memberValue = member.Get(thisObject);
					var memberType = member.Type;
					context.WriteYaml(memberValue, memberType);
				}

				WriteKey(context, context.Settings.SpecialCollectionMember);
				pureCollectionSerializer.WriteYaml(context, thisObject, context.FindTypeDescriptor(thisObject.GetType()));
			}
		}

		internal class PureCollectionSerializer : ObjectSerializer
		{
			public override void ReadItem(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
			{
				var list = (IList)thisObject;
				var collectionDescriptor = (CollectionDescriptor)typeDescriptor;

				var valueResult = context.ReadYaml(null, collectionDescriptor.ElementType);

				// Handle aliasing
				if (valueResult.IsAlias)
				{
					context.AddAliasBinding(valueResult.Alias, deferredValue => list.Add(deferredValue));
				}
				else
				{
					list.Add(valueResult.Value);
				}
			}

			protected override SequenceStyle GetSequenceStyle(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
			{
				var collection = (ICollection)thisObject;
				return collection.Count < context.Settings.LimitFlowSequence ? SequenceStyle.Flow : SequenceStyle.Block;
			}

			public override void WriteItems(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
			{
				var collection = (ICollection)thisObject;
				var collectionDescriptor = (CollectionDescriptor)typeDescriptor;

				foreach (var item in collection)
				{
					context.WriteYaml(item, collectionDescriptor.ElementType);
				}
			}
		}
	}
}