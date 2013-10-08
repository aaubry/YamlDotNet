using System;
using System.Collections;
using System.Linq;
using YamlDotNet.Events;
using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Serialization.Serializers
{
	internal class DictionarySerializer : ObjectSerializer
	{
		private readonly PureDictionarySerializer pureDictionarySerializer;

		public DictionarySerializer()
		{
			pureDictionarySerializer = new PureDictionarySerializer();
		}

		public override IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
		{
			return typeDescriptor is DictionaryDescriptor ? this : null;
		}

		public override void ReadItem(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
		{
			var dictionaryDescriptor = (DictionaryDescriptor) typeDescriptor;

			if (dictionaryDescriptor.IsPureDictionary)
			{
			    pureDictionarySerializer.ReadItem(context, thisObject, typeDescriptor);
			}
			else
			{
				var keyEvent = context.Reader.Peek<Scalar>();
				if (keyEvent != null)
				{
					if (keyEvent.Value == context.Settings.SpecialCollectionMember)
					{
						context.Reader.Accept<Scalar>();
						pureDictionarySerializer.ReadYaml(context, thisObject, context.FindTypeDescriptor(thisObject.GetType()));
						return;
					}
				}

				base.ReadItem(context, thisObject, typeDescriptor);	
			}
		}

		public override void WriteItems(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
		{
			var dictionaryDescriptor = (DictionaryDescriptor)typeDescriptor;
			if (dictionaryDescriptor.IsPureDictionary)
			{
				pureDictionarySerializer.WriteItems(context, thisObject, typeDescriptor);
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
				pureDictionarySerializer.WriteYaml(context, thisObject, context.FindTypeDescriptor(thisObject.GetType()));
			}
		}

		internal class PureDictionarySerializer : ObjectSerializer
		{
			public override void ReadItem(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
			{
				var dictionaryDescriptor = (DictionaryDescriptor)typeDescriptor;

                var keyResult = context.ReadYaml(null, dictionaryDescriptor.KeyType);
                var valueResult = context.ReadYaml(null, dictionaryDescriptor.ValueType);

                // Handle aliasing
                if (keyResult.IsAlias || valueResult.IsAlias)
                {
                    if (keyResult.IsAlias)
                    {
                        if (valueResult.IsAlias)
                        {
                            context.AddAliasBinding(keyResult.Alias, deferredKey => dictionaryDescriptor.AddToDictionary(thisObject, deferredKey, context.GetAliasValue(valueResult.Alias)));
                        }
                        else
                        {
                            context.AddAliasBinding(keyResult.Alias, deferredKey => dictionaryDescriptor.AddToDictionary(thisObject, deferredKey, valueResult.Value));
                        }
                    }
                    else
                    {
                        context.AddAliasBinding(valueResult.Alias, deferredAlias => dictionaryDescriptor.AddToDictionary(thisObject, keyResult.Value, deferredAlias));
                    }
                }
                else
                {
                    dictionaryDescriptor.AddToDictionary(thisObject, keyResult.Value, valueResult.Value);
                }
			}

			public override void WriteItems(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
			{
				var dictionaryDescriptor = (DictionaryDescriptor)typeDescriptor;

			    var keyValues = dictionaryDescriptor.GetEnumerator(thisObject).ToList();

				if (context.Settings.SortKeyForMapping)
				{
                    keyValues.Sort((left, right) =>
						{
							if (left.Key is IComparable && right.Key is IComparable)
							{
								return ((IComparable) left.Key).CompareTo(right.Key);
							}
							return 0;
						});
				}

				var keyType = dictionaryDescriptor.KeyType;
				var valueType = dictionaryDescriptor.ValueType;
				foreach (var keyValue in keyValues)
				{
                    context.WriteYaml(keyValue.Key, keyType);
					context.WriteYaml(keyValue.Value, valueType);
				}
			}
		}
	}
}