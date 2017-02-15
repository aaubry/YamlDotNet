//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) Antoine Aubry and contributors

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.Converters;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;
using YamlDotNet.Serialization.NodeTypeResolvers;
using YamlDotNet.Serialization.ObjectFactories;
using YamlDotNet.Serialization.TypeInspectors;
using YamlDotNet.Serialization.TypeResolvers;
using YamlDotNet.Serialization.Utilities;
using YamlDotNet.Serialization.ValueDeserializers;

namespace YamlDotNet.Serialization
{
    /// <summary>
    /// Deserializes objects from the YAML format.
    /// To customize the behavior of <see cref="Deserializer" />,
    /// use the <see cref="DeserializerBuilder" /> class.
    /// </summary>
    public sealed class Deserializer
    {
        #region Backwards compatibility
        private class BackwardsCompatibleConfiguration
        {
            private static readonly Dictionary<string, Type> predefinedTagMappings = new Dictionary<string, Type>
            {
                { "tag:yaml.org,2002:map", typeof(Dictionary<object, object>) },
                { "tag:yaml.org,2002:bool", typeof(bool) },
                { "tag:yaml.org,2002:float", typeof(double) },
                { "tag:yaml.org,2002:int", typeof(int) },
                { "tag:yaml.org,2002:str", typeof(string) },
                { "tag:yaml.org,2002:timestamp", typeof(DateTime) }
            };

            private readonly Dictionary<string, Type> tagMappings;
            private readonly List<IYamlTypeConverter> converters;
            private TypeDescriptorProxy typeDescriptor = new TypeDescriptorProxy();
            public IValueDeserializer valueDeserializer;

            public IList<INodeDeserializer> NodeDeserializers { get; private set; }
            public IList<INodeTypeResolver> TypeResolvers { get; private set; }

            private class TypeDescriptorProxy : ITypeInspector
            {
                public ITypeInspector TypeDescriptor;

                public IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
                {
                    return TypeDescriptor.GetProperties(type, container);
                }

                public IPropertyDescriptor GetProperty(Type type, object container, string name, bool ignoreUnmatched)
                {
                    return TypeDescriptor.GetProperty(type, container, name, ignoreUnmatched);
                }
            }

            public BackwardsCompatibleConfiguration(
                IObjectFactory objectFactory,
                INamingConvention namingConvention,
                bool ignoreUnmatched,
                YamlAttributeOverrides overrides)
            {
                objectFactory = objectFactory ?? new DefaultObjectFactory();
                namingConvention = namingConvention ?? new NullNamingConvention();

                typeDescriptor.TypeDescriptor =
                    new CachedTypeInspector(
                        new NamingConventionTypeInspector(
                            new YamlAttributesTypeInspector(
                                new YamlAttributeOverridesInspector(
                                    new ReadableAndWritablePropertiesTypeInspector(
                                        new ReadablePropertiesTypeInspector(
                                            new StaticTypeResolver()
                                        )
                                    ),
                                    overrides
                                )
                            ),
                            namingConvention
                        )
                    );

                converters = new List<IYamlTypeConverter>();
                converters.Add(new GuidConverter(false));

                NodeDeserializers = new List<INodeDeserializer>();
                NodeDeserializers.Add(new YamlConvertibleNodeDeserializer(objectFactory));
                NodeDeserializers.Add(new YamlSerializableNodeDeserializer(objectFactory));
                NodeDeserializers.Add(new TypeConverterNodeDeserializer(converters));
                NodeDeserializers.Add(new NullNodeDeserializer());
                NodeDeserializers.Add(new ScalarNodeDeserializer());
                NodeDeserializers.Add(new ArrayNodeDeserializer());
                NodeDeserializers.Add(new DictionaryNodeDeserializer(objectFactory));
                NodeDeserializers.Add(new CollectionNodeDeserializer(objectFactory));
                NodeDeserializers.Add(new EnumerableNodeDeserializer());
                NodeDeserializers.Add(new ObjectNodeDeserializer(objectFactory, typeDescriptor, ignoreUnmatched));

                tagMappings = new Dictionary<string, Type>(predefinedTagMappings);
                TypeResolvers = new List<INodeTypeResolver>();
                TypeResolvers.Add(new YamlConvertibleTypeResolver());
                TypeResolvers.Add(new YamlSerializableTypeResolver());
                TypeResolvers.Add(new TagNodeTypeResolver(tagMappings));
                TypeResolvers.Add(new TypeNameInTagNodeTypeResolver());
                TypeResolvers.Add(new DefaultContainersNodeTypeResolver());

                valueDeserializer =
                    new AliasValueDeserializer(
                        new NodeValueDeserializer(
                            NodeDeserializers,
                            TypeResolvers
                        )
                    );
            }

            public void RegisterTagMapping(string tag, Type type)
            {
                tagMappings.Add(tag, type);
            }

            public void RegisterTypeConverter(IYamlTypeConverter typeConverter)
            {
                converters.Insert(0, typeConverter);
            }
        }

        private readonly BackwardsCompatibleConfiguration backwardsCompatibleConfiguration;

        private void ThrowUnlessInBackwardsCompatibleMode()
        {
            if (backwardsCompatibleConfiguration == null)
            {
                throw new InvalidOperationException("This method / property exists for backwards compatibility reasons, but the Deserializer was created using the new configuration mechanism. To configure the Deserializer, use the DeserializerBuilder.");
            }
        }

        [Obsolete("Please use DeserializerBuilder to customize the Deserializer. This property will be removed in future releases.")]
        public IList<INodeDeserializer> NodeDeserializers
        {
            get
            {
                ThrowUnlessInBackwardsCompatibleMode();
                return backwardsCompatibleConfiguration.NodeDeserializers;
            }
        }

        [Obsolete("Please use DeserializerBuilder to customize the Deserializer. This property will be removed in future releases.")]
        public IList<INodeTypeResolver> TypeResolvers
        {
            get
            {
                ThrowUnlessInBackwardsCompatibleMode();
                return backwardsCompatibleConfiguration.TypeResolvers;
            }
        }

        [Obsolete("Please use DeserializerBuilder to customize the Deserializer. This constructor will be removed in future releases.")]
        public Deserializer(
            IObjectFactory objectFactory = null,
            INamingConvention namingConvention = null,
            bool ignoreUnmatched = false,
            YamlAttributeOverrides overrides = null)
        {
            backwardsCompatibleConfiguration = new BackwardsCompatibleConfiguration(objectFactory, namingConvention, ignoreUnmatched, overrides);
            valueDeserializer = backwardsCompatibleConfiguration.valueDeserializer;
        }

        [Obsolete("Please use DeserializerBuilder to customize the Deserializer. This method will be removed in future releases.")]
        public void RegisterTagMapping(string tag, Type type)
        {
            ThrowUnlessInBackwardsCompatibleMode();
            backwardsCompatibleConfiguration.RegisterTagMapping(tag, type);
        }

        [Obsolete("Please use DeserializerBuilder to customize the Deserializer. This method will be removed in future releases.")]
        public void RegisterTypeConverter(IYamlTypeConverter typeConverter)
        {
            ThrowUnlessInBackwardsCompatibleMode();
            backwardsCompatibleConfiguration.RegisterTypeConverter(typeConverter);
        }
        #endregion

        private readonly IValueDeserializer valueDeserializer;

        /// <summary>
        /// Initializes a new instance of <see cref="Deserializer" /> using the default configuration.
        /// </summary>
        /// <remarks>
        /// To customize the bahavior of the deserializer, use <see cref="DeserializerBuilder" />.
        /// </remarks>
        public Deserializer()
        // TODO: When the backwards compatibility is dropped, uncomment the following line and remove the body of this constructor.
        // : this(new DeserializerBuilder().BuildValueDeserializer())
        {
            backwardsCompatibleConfiguration = new BackwardsCompatibleConfiguration(null, null, false, null);
            valueDeserializer = backwardsCompatibleConfiguration.valueDeserializer;
        }

        /// <remarks>
        /// This constructor is private to discourage its use.
        /// To invoke it, call the <see cref="FromValueDeserializer"/> method.
        /// </remarks>
        private Deserializer(IValueDeserializer valueDeserializer)
        {
            if (valueDeserializer == null)
            {
                throw new ArgumentNullException("valueDeserializer");
            }

            this.valueDeserializer = valueDeserializer;
        }

        /// <summary>
        /// Creates a new <see cref="Deserializer" /> that uses the specified <see cref="IValueDeserializer" />.
        /// This method is available for advanced scenarios. The preferred way to customize the bahavior of the
        /// deserializer is to use <see cref="DeserializerBuilder" />.
        /// </summary>
        public static Deserializer FromValueDeserializer(IValueDeserializer valueDeserializer)
        {
            return new Deserializer(valueDeserializer);
        }

        public T Deserialize<T>(string input)
        {
            using (var reader = new StringReader(input))
            {
                return (T)Deserialize(reader, typeof(T));
            }
        }

        public T Deserialize<T>(TextReader input)
        {
            return (T)Deserialize(input, typeof(T));
        }

        public object Deserialize(TextReader input)
        {
            return Deserialize(input, typeof(object));
        }

        public object Deserialize(string input, Type type)
        {
            using (var reader = new StringReader(input))
            {
                return Deserialize(reader, type);
            }
        }

        public object Deserialize(TextReader input, Type type)
        {
            return Deserialize(new Parser(input), type);
        }

        public T Deserialize<T>(IParser parser)
        {
            return (T)Deserialize(parser, typeof(T));
        }

        public object Deserialize(IParser parser)
        {
            return Deserialize(parser, typeof(object));
        }

        /// <summary>
        /// Deserializes an object of the specified type.
        /// </summary>
        /// <param name="parser">The <see cref="IParser" /> from where to deserialize the object.</param>
        /// <param name="type">The static type of the object to deserialize.</param>
        /// <returns>Returns the deserialized object.</returns>
        public object Deserialize(IParser parser, Type type)
        {
            if (parser == null)
            {
                throw new ArgumentNullException("reader");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            var hasStreamStart = parser.Allow<StreamStart>() != null;

            var hasDocumentStart = parser.Allow<DocumentStart>() != null;

            object result = null;
            if (!parser.Accept<DocumentEnd>() && !parser.Accept<StreamEnd>())
            {
                using (var state = new SerializerState())
                {
                    result = valueDeserializer.DeserializeValue(parser, type, state, valueDeserializer);
                    state.OnDeserialization();
                }
            }

            if (hasDocumentStart)
            {
                parser.Expect<DocumentEnd>();
            }

            if (hasStreamStart)
            {
                parser.Expect<StreamEnd>();
            }

            return result;
        }
    }
}