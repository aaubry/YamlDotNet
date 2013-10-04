using System;
using System.Collections.Generic;
using System.Reflection;

namespace YamlDotNet.Serialization
{
	internal class TagRegistry
	{
		private readonly Dictionary<string, Type> tagToType;
		private readonly Dictionary<Type, string> typeToTag;
		private readonly List<Assembly> lookupAssemblies;

		public TagRegistry(YamlSerializerSettings settings)
		{
			tagToType = new Dictionary<string, Type>(settings.TagToType);
			typeToTag = new Dictionary<Type, string>(settings.TypeToTag);
			lookupAssemblies = new List<Assembly>(settings.LookupAssemblies);
		}

		public Type TypeFromTag(string tagName)
		{
			if (tagName == null)
			{
				return null;
			}

			Type type;
			if (tagToType.TryGetValue(tagName, out type))
			{
				return type;
			}

			type = Type.GetType(tagName);
			if (type == null)
			{
				foreach (var assembly in lookupAssemblies)
				{
					type = assembly.GetType(tagName);
					if (type != null)
					{
						break;
					}
				}
			}
			return type;
		}

		public string TagFromType(Type type)
		{
			string tagName;
			if (!typeToTag.TryGetValue(type, out tagName))
			{
				return string.Format("!{0}", type.FullName);
			}
			return tagName;
		}
	}
}