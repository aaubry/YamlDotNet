using System;
using System.Collections.Generic;
using YamlDotNet.Events;
using YamlDotNet.Schemas;
using YamlDotNet.Serialization.Descriptors;
using YamlDotNet.Serialization.Serializers;

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
		private readonly List<AnchorLateBinding> anchorLateBindings;

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
			Schema = Settings.Schema;
			typeDescriptorFactory = new TypeDescriptorFactory(Settings.Attributes, Settings.EmitDefaultValues);
			anchorLateBindings = new List<AnchorLateBinding>();
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
		/// Gets the schema.
		/// </summary>
		/// <value>The schema.</value>
		public IYamlSchema Schema { get; private set; }

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

		internal AnchorSerializer ObjectSerializer { get; set; }



		/// <summary>
		/// The default function to read an object from the current Yaml stream.
		/// </summary>
		/// <param name="value">The value of the receiving object, may be null.</param>
		/// <param name="expectedType">The expected type.</param>
		/// <returns>System.Object.</returns>
		public ValueResult ReadYaml(object value, Type expectedType)
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
		public void WriteYaml(object value, Type expectedType)
		{
			ObjectSerializer.WriteYaml(this, value, FindTypeDescriptor(expectedType));
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
			return tagTypeRegistry.TypeFromTag(tagName);
		}
		
		/// <summary>
		/// Resolves a tag from a type
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>The associated tag</returns>
		public string TagFromType(Type type)
		{
			return tagTypeRegistry.TagFromType(type);
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


		private struct AnchorLateBinding
		{
			public AnchorLateBinding(AnchorAlias anchorAlias, Action<object> setter)
			{
				AnchorAlias = anchorAlias;
				Setter = setter;
			}

			public readonly AnchorAlias AnchorAlias;

			public readonly Action<object> Setter;
		}

		/// <summary>
		/// Gets the alias value.
		/// </summary>
		/// <param name="alias">The alias.</param>
		/// <returns>System.Object.</returns>
		/// <exception cref="System.ArgumentNullException">alias</exception>
		/// <exception cref="AnchorNotFoundException">Alias [{0}] not found.DoFormat(alias.Value)</exception>
		public object GetAliasValue(AnchorAlias alias)
		{
			if (alias == null) throw new ArgumentNullException("alias");

			object value;
			if (!ObjectSerializer.TryGetAliasValue(alias.Value, out value))
			{
				throw new AnchorNotFoundException(alias.Value, alias.Start, alias.End, "Alias [{0}] not found".DoFormat(alias.Value));				
			}
			return value;
		}

		/// <summary>
		/// Adds the late binding.
		/// </summary>
		/// <param name="alias">The alias.</param>
		/// <param name="setter">The setter.</param>
		/// <exception cref="System.ArgumentException">No alias found in the ValueResult;valueResult</exception>
		public void AddAliasBinding(AnchorAlias alias, Action<object> setter)
		{
			if (alias == null) throw new ArgumentNullException("alias");
			if (setter == null) throw new ArgumentNullException("setter");

			anchorLateBindings.Add(new AnchorLateBinding(alias, setter));
		}

		internal string GetAnchor()
		{
			return Anchors.Count > 0 ? Anchors.Pop() : null;
		}

		internal Stack<string> Anchors = new Stack<string>();

		internal int AnchorCount;

		internal void ResolveLateAliasBindings()
		{
			foreach (var lateBinding in anchorLateBindings)
			{
				var alias = lateBinding.AnchorAlias;
				var value = GetAliasValue(alias);
				lateBinding.Setter(value);
			}
		}
	}
}