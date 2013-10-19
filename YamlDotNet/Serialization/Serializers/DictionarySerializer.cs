using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Events;
using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Serialization.Serializers
{
	internal class DictionarySerializer : ObjectSerializer
	{
		public DictionarySerializer()
		{
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
				ReadPureDictionaryItems(context, thisObject, typeDescriptor);
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

						reader.Expect<MappingStart>();
						ReadPureDictionaryItems(context, thisObject, typeDescriptor);
						reader.Expect<MappingEnd>();
						return;
					}
				}

				base.ReadItem(context, thisObject, typeDescriptor);	
			}
		}

		public override void WriteItems(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor, YamlStyle style)
		{
			var dictionaryDescriptor = (DictionaryDescriptor)typeDescriptor;
			if (dictionaryDescriptor.IsPureDictionary)
			{
				WritePureDictionaryItems(context, thisObject, typeDescriptor);
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

					context.PushStyle(member.Style);
					context.WriteYaml(memberValue, memberType);
				}

				WriteKey(context, context.Settings.SpecialCollectionMember);

				context.Writer.Emit(new MappingStartEventInfo(thisObject, thisObject.GetType()) { Style = style });
				WritePureDictionaryItems(context, thisObject, typeDescriptor);
				context.Writer.Emit(new MappingEndEventInfo(thisObject, thisObject.GetType()));
			}
		}

		private void ReadPureDictionaryItems(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
		{
			var dictionaryDescriptor = (DictionaryDescriptor)typeDescriptor;

			var reader = context.Reader;
			while (!reader.Accept<MappingEnd>())
			{
				var keyResult = context.ReadYaml(null, dictionaryDescriptor.KeyType);
				var valueResult = context.ReadYaml(null, dictionaryDescriptor.ValueType);

				// Handle aliasing
				if (keyResult.IsAlias || valueResult.IsAlias)
				{
					if (keyResult.IsAlias)
					{
						if (valueResult.IsAlias)
						{
							context.AddAliasBinding(keyResult.Alias,
							                        deferredKey =>
							                        dictionaryDescriptor.AddToDictionary(thisObject, deferredKey,
							                                                             context.GetAliasValue(valueResult.Alias)));
						}
						else
						{
							context.AddAliasBinding(keyResult.Alias,
							                        deferredKey =>
							                        dictionaryDescriptor.AddToDictionary(thisObject, deferredKey, valueResult.Value));
						}
					}
					else
					{
						context.AddAliasBinding(valueResult.Alias,
						                        deferredAlias =>
						                        dictionaryDescriptor.AddToDictionary(thisObject, keyResult.Value, deferredAlias));
					}
				}
				else
				{
					dictionaryDescriptor.AddToDictionary(thisObject, keyResult.Value, valueResult.Value);
				}
			}
		}

		private void WritePureDictionaryItems(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
		{
			var dictionaryDescriptor = (DictionaryDescriptor)typeDescriptor;

			var keyValues = dictionaryDescriptor.GetEnumerator(thisObject).ToList();

			if (context.Settings.SortKeyForMapping)
			{
				keyValues.Sort(SortDictionaryByKeys);
			}

			var keyType = dictionaryDescriptor.KeyType;
			var valueType = dictionaryDescriptor.ValueType;
			foreach (var keyValue in keyValues)
			{
				context.WriteYaml(keyValue.Key, keyType);
				context.WriteYaml(keyValue.Value, valueType);
			}
		}

		private static int SortDictionaryByKeys(KeyValuePair<object, object> left, KeyValuePair<object, object> right)
		{
			if (left.Key is string && right.Key is string)
			{
				return string.CompareOrdinal((string)left.Key, (string)right.Key);
			}

			if (left.Key is IComparable && right.Key is IComparable)
			{
				return ((IComparable)left.Key).CompareTo(right.Key);
			}
			return 0;
		}
	}
}