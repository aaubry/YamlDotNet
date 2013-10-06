using System;
using System.Collections.Generic;
using YamlDotNet.Events;

namespace YamlDotNet.Serialization.Processors
{
	public class AnchorProcessor : ChainedProcessor
	{
		private Dictionary<string, object> aliasToObject;
		private Dictionary<object, string> objectToAlias;

		public AnchorProcessor(IYamlProcessor next) : base(next)
		{
		}

		public override object ReadYaml(SerializerContext context, object value, Type expectedType)
		{
			var reader = context.Reader;

			// Process Anchor alias (*oxxx)
			var alias = reader.Allow<AnchorAlias>();
			if (alias != null)
			{
				if (!AliasToObject.TryGetValue(alias.Value, out value))
				{
					throw new YamlException("Alias [{0}] not found".DoFormat(alias.Value));
				}

				return value;
			}

			// Test if current node as an anchor
			string anchor = null;
			var nodeEvent = reader.Peek<NodeEvent>();
			if (nodeEvent != null && !string.IsNullOrEmpty(nodeEvent.Anchor))
			{
				anchor = nodeEvent.Anchor;
			}

			// Deserialize the current node
			value = base.ReadYaml(context, value, expectedType);

			// Store Anchor (&oxxx) and override any defined anchor 
			if (anchor != null)
			{
				AliasToObject[anchor] = value;
			}

			return value;
		}

		public override void WriteYaml(SerializerContext context, object value, Type type)
		{
			if (value != null && Type.GetTypeCode(value.GetType()) == TypeCode.Object)
			{
				string alias;
				if (ObjectToString.TryGetValue(value, out alias))
				{
					context.Writer.Emit(new AliasEventInfo(value, type) {Alias = alias});
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

			base.WriteYaml(context, value, type);
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