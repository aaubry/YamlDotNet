using System;
using System.Collections;
using System.Linq;
using YamlDotNet.Events;
using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Serialization.Processors
{
	public class DictionaryProcessor : ObjectProcessor
	{
		private readonly PureDictionaryProcessor pureDictionaryProcessor;

		public DictionaryProcessor(YamlSerializerSettings settings) : base(settings)
		{
			pureDictionaryProcessor = new PureDictionaryProcessor(settings);
		}

		protected override void ReadItem(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
		{
			var dictionary = (IDictionary) thisObject;
			var dictionaryDescriptor = (DictionaryDescriptor) typeDescriptor;

			if (!dictionaryDescriptor.HasMembers)
			{
				var key = context.ReadYaml(null, dictionaryDescriptor.KeyType);
				var value = context.ReadYaml(null, dictionaryDescriptor.ValueType);
				dictionary.Add(key, value);
			}
			else
			{
				var keyEvent = context.Reader.Peek<Scalar>();
				if (keyEvent != null)
				{
					if (keyEvent.Value == Settings.SpecialCollectionMember)
					{
						pureDictionaryProcessor.ReadYaml(context, thisObject, context.FindTypeDescriptor(thisObject.GetType()));
						return;
					}
				}

				base.ReadItem(context, thisObject, typeDescriptor);	
			}
		}

		public override void WriteItems(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
		{
			if (!typeDescriptor.HasMembers)
			{
				pureDictionaryProcessor.WriteItems(context, thisObject, typeDescriptor);
			}
			else
			{
				// Serialize Dictionary members
				foreach (var member in typeDescriptor.Members)
				{
					// Emit the key name
					context.Writer.Emit(new ScalarEventInfo(member.Name, typeof(string)));

					var memberValue = member.Get(thisObject);
					var memberType = member.Type;
					context.WriteYaml(memberValue, memberType);
				}

				context.Writer.Emit(new ScalarEventInfo(Settings.SpecialCollectionMember, typeof(string)));
				pureDictionaryProcessor.WriteYaml(context, thisObject, context.FindTypeDescriptor(thisObject.GetType()));
			}
		}

		internal class PureDictionaryProcessor : ObjectProcessor
		{
			public PureDictionaryProcessor(YamlSerializerSettings settings) : base(settings)
			{
			}

			protected override void ReadItem(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
			{
				var dictionary = (IDictionary)thisObject;
				var dictionaryDescriptor = (DictionaryDescriptor)typeDescriptor;

				var key = context.ReadYaml(null, dictionaryDescriptor.KeyType);
				var value = context.ReadYaml(null, dictionaryDescriptor.ValueType);
				dictionary.Add(key, value);
			}

			public override void WriteItems(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
			{
				var dictionary = (IDictionary) thisObject;
				var dictionaryDescriptor = (DictionaryDescriptor)typeDescriptor;

				var keys = dictionary.Keys;
				if (context.Settings.SortKeyForMapping)
				{
					var sortedKeys = keys.Cast<object>().ToList();
					sortedKeys.Sort((left, right) =>
						{
							if (left is IComparable && right is IComparable)
							{
								return ((IComparable) left).CompareTo(right);
							}
							return 0;
						});
					keys = sortedKeys;
				}

				var keyType = dictionaryDescriptor.KeyType;
				var valueType = dictionaryDescriptor.ValueType;
				foreach (var key in keys)
				{
					context.WriteYaml(key, keyType);
					context.WriteYaml(dictionary[key], valueType);
				}
			}
		}
	}
}