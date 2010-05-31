using System;
using YamlDotNet.Core;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// 
	/// </summary>
	public class DeserializationOverride
	{
		private readonly Type deserializedType;

		/// <summary>
		/// Gets type that contains the property.
		/// </summary>
		public Type DeserializedType
		{
			get
			{
				return deserializedType;
			}
		}

		private readonly string deserializedPropertyName;

		/// <summary>
		/// Gets the name of the deserialized property.
		/// </summary>
		public string DeserializedPropertyName
		{
			get
			{
				return deserializedPropertyName;
			}
		}

		private readonly Action<object, EventReader> deserializer;

		/// <summary>
		/// Gets the delegate that will perform the deserialization.
		/// </summary>
		public Action<object, EventReader> Deserializer
		{
			get
			{
				return deserializer;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DeserializationOverride"/> class.
		/// </summary>
		/// <param name="deserializedType">The type that contains the property.</param>
		/// <param name="deserializedPropertyName">Name of the deserialized property.</param>
		/// <param name="deserializer">The delegate that will perform the deserialization.</param>
		public DeserializationOverride(Type deserializedType, string deserializedPropertyName, Action<object, EventReader> deserializer)
		{
			this.deserializedType = deserializedType;
			this.deserializedPropertyName = deserializedPropertyName;
			this.deserializer = deserializer;
		}
	}
}