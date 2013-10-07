using System;
using System.Collections.Generic;
using YamlDotNet.Events;
using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Serialization
{

	/// <summary>
	/// A context used while deserializing.
	/// </summary>
	public class SerializerContext
	{
        private readonly SerializerSettings settings;
	    private readonly ITagTypeRegistry tagTypeRegistry;
		private readonly ITypeDescriptorFactory typeDescriptorFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializerContext"/> class.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
		internal SerializerContext(Serializer serializer)
        {
            Serializer = serializer;
            settings = serializer.Settings;
	        tagTypeRegistry = settings.TagTypes;
	        ObjectFactory = settings.ObjectFactory;
	        typeDescriptorFactory = new TypeDescriptorFactory(Settings.Attributes);
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
		public SerializerSettings Settings
		{
			get { return settings; }
		}

		/// <summary>
		/// Gets the serializer.
		/// </summary>
		/// <value>The serializer.</value>
        public Serializer Serializer { get; private set; }

        /// <summary>
        /// Gets the reader used while deserializing.
        /// </summary>
        /// <value>The reader.</value>
        public EventReader Reader { get; internal set; }

        /// <summary>
        /// The default function to read an object from the current Yaml stream.
        /// </summary>
        /// <param name="value">The value of the receiving object, may be null.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <returns>System.Object.</returns>
	    public object ReadYaml(object value, Type expectedType)
        {
	        var node = Reader.Parser.Current;
	        try
	        {
				return ObjectSerializer.ReadYaml(this, value, FindTypeDescriptor(expectedType));
	        }
	        catch (Exception ex)
	        {
		        if (ex is YamlException)
			        throw;
				throw new YamlException(node.Start, node.End, "Error while deserializing node [{0}]".DoFormat(node), ex);
	        }
	    }

		/// <summary>
		/// Gets or sets the type of the create.
		/// </summary>
		/// <value>The type of the create.</value>
		public IObjectFactory ObjectFactory { get; set; }

		/// <summary>
		/// Gets the writer used while deserializing.
		/// </summary>
		/// <value>The writer.</value>
		public IEventEmitter Writer { get; internal set; }

        /// <summary>
        /// The default function to write an object to Yaml
        /// </summary>
        public void WriteYaml(object value, Type type)
        {
            ObjectSerializer.WriteYaml(this, value, FindTypeDescriptor(type));
        }

		/// <summary>
		/// Finds the type descriptor for the specified type.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>An instance of <see cref="ITypeDescriptor"/>.</returns>
		public ITypeDescriptor FindTypeDescriptor(Type type)
		{
			return typeDescriptorFactory.Find(type);
		}

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

		/// <summary>
		/// Gets the default tag and value for the specified <see cref="Scalar" />. The default tag can be different from a actual tag of this <see cref="NodeEvent" />.
		/// </summary>
		/// <param name="scalar">The scalar event.</param>
		/// <param name="defaultTag">The default tag decoded from the scalar.</param>
		/// <param name="value">The value extracted from a scalar.</param>
		/// <returns>System.String.</returns>
		public bool TryParseScalar(Scalar scalar, out string defaultTag, out object value)
		{
			return Settings.Schema.TryParse(scalar, true, out defaultTag, out value);
		}

		internal IYamlSerializable ObjectSerializer { get; set; }

		internal string GetAnchor()
		{
			return Anchors.Count > 0 ? Anchors.Pop() : null;
		}

		internal Stack<string> Anchors = new Stack<string>();

		internal int AnchorCount;
	}
}