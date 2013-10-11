using System;
using System.Collections.Generic;
using YamlDotNet.Events;

namespace YamlDotNet.Serialization.Serializers
{
	internal class AnchorSerializer : ChainedSerializer
	{
		private Dictionary<string, object> aliasToObject;
		private Dictionary<object, string> objectToAlias;

		public AnchorSerializer(IYamlSerializable next) : base(next)
		{
		}

		public bool TryGetAliasValue(string alias, out object value)
		{
			return AliasToObject.TryGetValue(alias, out value);
		}

		public override ValueOutput ReadYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
		{
			var reader = context.Reader;

			// Process Anchor alias (*oxxx)
			var alias = reader.Allow<AnchorAlias>();
			if (alias != null)
			{
				// Return an alias or directly the value
				return !AliasToObject.TryGetValue(alias.Value, out value) ? new ValueOutput(alias) : new ValueOutput(value);
			}

			// Test if current node has an anchor &oxxx
			string anchor = null;
			var nodeEvent = reader.Peek<NodeEvent>();
			if (nodeEvent != null && !string.IsNullOrEmpty(nodeEvent.Anchor))
			{
				anchor = nodeEvent.Anchor;
			}

			// Deserialize the current node
			var valueResult = base.ReadYaml(context, value, typeDescriptor);

			// Store Anchor (&oxxx) and override any defined anchor 
			if (anchor != null)
			{
				AliasToObject[anchor] = valueResult.Value;
			}

			return valueResult;
		}

		public override void WriteYaml(SerializerContext context, ValueInput input, ITypeDescriptor typeDescriptor)
		{
			var value = input.Value;

			// Only write anchors for object (and not value types)
			bool isAnchorable = false;
			if (value != null && !value.GetType().IsValueType)
			{
				var typeCode = Type.GetTypeCode(value.GetType());
				switch (typeCode)
				{
					case TypeCode.Object:
					case TypeCode.String:
						isAnchorable = true;
						break;
				}
			}

			if (isAnchorable)
			{
				string alias;
				if (ObjectToString.TryGetValue(value, out alias))
				{
					context.Writer.Emit(new AliasEventInfo(value, value.GetType()) {Alias = alias});
					return;
				}
				else
				{
					alias = string.Format("o{0}", context.AnchorCount);
					ObjectToString.Add(value, alias);

					// Store the alias in the context
					context.Anchors.Push(alias);
					context.AnchorCount++;
				}
			}

			base.WriteYaml(context, input, typeDescriptor);
		}

		private Dictionary<string, object> AliasToObject
		{
			get { return aliasToObject ?? (aliasToObject = new Dictionary<string, object>()); }
		}

		private Dictionary<object, string> ObjectToString
		{
			get { return objectToAlias ?? (objectToAlias = new Dictionary<object, string>(new IdentityEqualityComparer<object>())); }
		}
	}
}