using System;
using System.Collections.Generic;
using YamlDotNet.Core;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Allows to register delegates that take care of the deserialization process of specific properties.
	/// </summary>
	public class DeserializationOverrides
	{
		private readonly Dictionary<Type, Dictionary<string, Action<object, EventReader>>> overrides = new Dictionary<Type, Dictionary<string, Action<object, EventReader>>>();

		/// <summary>
		/// Adds an override for the specified property.
		/// </summary>
		/// <param name="deserializedType">The type that contains the property.</param>
		/// <param name="deserializedPropertyName">Name of the deserialized property.</param>
		/// <param name="deserializer">The delegate that will perform the deserialization.</param>
		public void Add(Type deserializedType, string deserializedPropertyName, Action<object, EventReader> deserializer)
		{
			if (deserializedType == null)
			{
				throw new ArgumentNullException("deserializedType");
			}
			if (string.IsNullOrEmpty(deserializedPropertyName))
			{
				throw new ArgumentNullException("deserializedPropertyName");
			}
			if (deserializer == null)
			{
				throw new ArgumentNullException("deserializer");
			}

			Dictionary<string, Action<object, EventReader>> typeOverrides;
			if (!overrides.TryGetValue(deserializedType, out typeOverrides))
			{
				typeOverrides = new Dictionary<string, Action<object, EventReader>>();
				overrides.Add(deserializedType, typeOverrides);
			}
			typeOverrides.Add(deserializedPropertyName, deserializer);
		}

		internal Action<object, EventReader> GetOverride(Type deserializedType, string deserializedPropertyName)
		{
			Dictionary<string, Action<object, EventReader>> typeOverrides;
			if (overrides.TryGetValue(deserializedType, out typeOverrides))
			{
				Action<object, EventReader> deserializer;
				if(typeOverrides.TryGetValue(deserializedPropertyName, out deserializer))
				{
					return deserializer;
				}
			}
			return null;
		}
	}
}