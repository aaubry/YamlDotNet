using System;
using System.Collections.Generic;
using YamlDotNet.Schemas;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// Settings used to configure serialization and control how objects are encoded into YAML.
	/// </summary>
	public sealed class SerializerSettings
	{
		private readonly AttributeRegistry attributeRegistry;
		internal readonly List<IYamlSerializableFactory> factories = new List<IYamlSerializableFactory>();
		internal readonly Dictionary<Type, IYamlSerializable> serializers = new Dictionary<Type, IYamlSerializable>();
		private readonly TagTypeRegistry tagTypeRegistry;
		private IObjectFactory objectFactory;
		private int preferredIndent;
		private IYamlSchema schema;
		private string specialCollectionMember;

		/// <summary>
		/// Initializes a new instance of the <see cref="SerializerSettings" /> class.
		/// </summary>
		public SerializerSettings()
		{
			PreferredIndent = 2;
			SortKeyForMapping = true;
			EmitJsonComptible = false;
			EmitCapacityForList = false;
			SpecialCollectionMember = "~Items";
			LimitFlowSequence = 20;
			schema = new CoreSchema();
			tagTypeRegistry = new TagTypeRegistry();
			attributeRegistry = new AttributeRegistry();
			ObjectFactory = new DefaultObjectFactory();
		}

		/// <summary>
		/// Gets or sets the preferred indentation. Default is 2.
		/// </summary>
		/// <value>The preferred indentation.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">value;Expecting value &gt; 0</exception>
		public int PreferredIndent
		{
			get { return preferredIndent; }
			set
			{
				if (value < 1) throw new ArgumentOutOfRangeException("value", "Expecting value > 0");
				preferredIndent = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to enable sorting keys from dictionary to YAML mapping. Default is true. See remarks.
		/// </summary>
		/// <value><c>true</c> to enable sorting keys from dictionary to YAML mapping; otherwise, <c>false</c>.</value>
		/// <remarks>When storing a YAML document, It can be important to keep the same order for key mapping in order to keep
		/// a YAML document versionable/diffable.</remarks>
		public bool SortKeyForMapping { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to emit json comptible YAML.
		/// </summary>
		/// <value><c>true</c> if to emit json comptible YAML; otherwise, <c>false</c>.</value>
		public bool EmitJsonComptible { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the property <see cref="List{T}.Capacity" /> should be emitted. Default is false.
		/// </summary>
		/// <value><c>true</c> if the property <see cref="List{T}.Capacity" /> should be emitted; otherwise, <c>false</c>.</value>
		public bool EmitCapacityForList { get; set; }

		/// <summary>
		/// Gets or sets the maximum number of elements an array/list of primitive can be emitted as a
		/// flow sequence (instead of a block sequence by default). Default is 20.
		/// </summary>
		/// <value>The emit compact array limit.</value>
		public int LimitFlowSequence { get; set; }

		/// <summary>
		/// Gets or sets the prefix used to serialize items for a non pure <see cref="System.Collections.IDictionary" /> or
		/// <see cref="System.Collections.ICollection" />
		/// . Default to "~Items", see remarks.
		/// </summary>
		/// <value>The prefix for items.</value>
		/// <exception cref="System.ArgumentNullException">value</exception>
		/// <exception cref="System.ArgumentException">Expecting length >= 2 and at least a special character '.', '~', '-' (not starting on first char for '-')</exception>
		/// <remarks>A pure <see cref="System.Collections.IDictionary" /> or <see cref="System.Collections.ICollection" /> is a class that inherits from these types but are not adding any
		/// public properties or fields. When these types are pure, they are respectively serialized as a YAML mapping (for dictionary) or a YAML sequence (for collections).
		/// If the collection type to serialize is not pure, the type is serialized as a YAML mapping sequence that contains the public properties/fields as well as a
		/// special fielx (e.g. "~Items") that contains the actual items of the collection (either a mapping for dictionary or a sequence for collections).
		/// The <see cref="SpecialCollectionMember" /> is this special key that is used when serializing items of a non-pure collection.</remarks>
		public string SpecialCollectionMember
		{
			get { return specialCollectionMember; }
			set
			{
				if (value == null) throw new ArgumentNullException("value");

				// TODO this is a poor check. Need to verify this against the specs
				if (value.Length < 2 || !(value.Contains(".") || value.Contains("~") || value.IndexOf('-') > 0))
				{
					throw new ArgumentException(
						"Expecting length >= 2 and at least a special character '.', '~', '-' (not starting on first char for '-')");
				}

				specialCollectionMember = value;
			}
		}

		/// <summary>
		/// Gets the attribute registry.
		/// </summary>
		/// <value>The attribute registry.</value>
		public AttributeRegistry Attributes
		{
			get { return attributeRegistry; }
		}

		/// <summary>
		/// Gets or sets the tag type registry. Default is <see cref="YamlDotNet.Serialization.TagTypeRegistry" />
		/// </summary>
		/// <value>The tag type registry.</value>
		/// <exception cref="System.ArgumentNullException">value</exception>
		public TagTypeRegistry TagTypes
		{
			get { return tagTypeRegistry; }
		}


		/// <summary>
		/// Gets or sets the default factory to instantiate a type. Default is <see cref="DefaultObjectFactory" />.
		/// </summary>
		/// <value>The default factory to instantiate a type.</value>
		/// <exception cref="System.ArgumentNullException">value</exception>
		public IObjectFactory ObjectFactory
		{
			get { return objectFactory; }
			set
			{
				if (value == null) throw new ArgumentNullException("value");
				objectFactory = value;
			}
		}

		/// <summary>
		/// Gets or sets the schema. Default is <see cref="CoreSchema" />. When setting the schema in this settings, the schema is initialized by calling its
		/// <see cref="IYamlSchema.Initialize" />
		/// method.
		/// </summary>
		/// <value>The schema.</value>
		/// <exception cref="System.ArgumentNullException">value</exception>
		public IYamlSchema Schema
		{
			get { return schema; }
			set
			{
				if (value == null) throw new ArgumentNullException("value");
				schema = value;
			}
		}

		/// <summary>
		/// Adds a custom serializer for the specified type.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="serializer">The serializer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// type
		/// or
		/// serializer
		/// </exception>
		public void AddSerializer(Type type, IYamlSerializable serializer)
		{
			if (type == null) throw new ArgumentNullException("type");
			if (serializer == null) throw new ArgumentNullException("serializer");
			serializers[type] = serializer;
		}

		/// <summary>
		/// Adds a serializer factory.
		/// </summary>
		/// <param name="factory">The factory.</param>
		/// <exception cref="System.ArgumentNullException">factory</exception>
		public void AddSerializerFactory(IYamlSerializableFactory factory)
		{
			if (factory == null) throw new ArgumentNullException("factory");
			factories.Add(factory);
		}
	}
}