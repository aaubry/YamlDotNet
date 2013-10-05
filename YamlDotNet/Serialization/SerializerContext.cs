using System;
using System.Collections.Generic;

namespace YamlDotNet.Serialization
{

	/// <summary>
	/// A context used while deserializing.
	/// </summary>
	public class SerializerContext
	{
        private readonly YamlSerializerSettings settings;
	    private readonly ITagTypeRegistry tagTypeRegistry;
		private readonly ITypeDescriptorFactory typeDescriptorFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializerContext"/> class.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
		internal SerializerContext(YamlSerializer serializer)
        {
            Serializer = serializer;
            settings = serializer.Settings;
	        tagTypeRegistry = serializer.TagTypeRegistry;
	        typeDescriptorFactory = settings.TypeDescriptorFactory;
        }

		/// <summary>
		/// Gets a value indicating whether we are in the context of serializing.
		/// </summary>
		/// <value><c>true</c> if we are in the context of serializing; otherwise, <c>false</c>.</value>
		public bool IsSerializing
		{
			get { return Writer != null; }
		}

		/// <summary>
		/// Gets the settings.
		/// </summary>
		/// <value>The settings.</value>
		public YamlSerializerSettings Settings
		{
			get { return settings; }
		}

		/// <summary>
		/// Gets the type descriptor factory.
		/// </summary>
		/// <value>The type descriptor factory.</value>
		public ITypeDescriptorFactory TypeDescriptorFactory
		{
			get { return typeDescriptorFactory; }
		}

		/// <summary>
		/// Gets the serializer.
		/// </summary>
		/// <value>The serializer.</value>
        public YamlSerializer Serializer { get; private set; }

        /// <summary>
        /// Gets the reader used while deserializing.
        /// </summary>
        /// <value>The reader.</value>
        public EventReader Reader { get; internal set; }

	    /// <summary>
	    /// The default function to read a Yaml.
	    /// </summary>
		public Func<object, Type, object> ReadYaml { get; set; }

		public Func<Type, object> CreateType { get; set; }

		/// <summary>
		/// Gets the writer used while deserializing.
		/// </summary>
		/// <value>The writer.</value>
		public IEventEmitter Writer { get; internal set; }

        /// <summary>
        /// The default function to write an object to Yaml
        /// </summary>
		public Action<object, Type> WriteYaml { get; set; }

		/// <summary>
		/// Resolves a type from the specified tag.
		/// </summary>
		/// <param name="tagName">Name of the tag.</param>
		/// <returns>Type.</returns>
		public Type TypeFromTag(string tagName)
		{
			return tagTypeRegistry.TypeFromTag(Settings.Schema, tagName);
		}
		
		/// <summary>
        /// Resolves a tag from a type
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The associated tag</returns>
	    public string TagFromType(Type type)
        {
	        return tagTypeRegistry.TagFromType(Settings.Schema, type);
        }

		internal string GetAnchor()
		{
			return Anchors.Count > 0 ? Anchors.Pop() : null;
		}

		internal Stack<string> Anchors = new Stack<string>();

		internal int AnchorCount;
	}
}