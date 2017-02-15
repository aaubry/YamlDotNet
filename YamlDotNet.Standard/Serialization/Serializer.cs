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
using YamlDotNet.Serialization.EventEmitters;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.ObjectGraphTraversalStrategies;
using YamlDotNet.Serialization.ObjectGraphVisitors;
using YamlDotNet.Serialization.TypeInspectors;
using YamlDotNet.Serialization.TypeResolvers;

namespace YamlDotNet.Serialization
{
    public sealed class Serializer
    {
        private readonly IValueSerializer valueSerializer;

        #region Backwards compatibility
        private class BackwardsCompatibleConfiguration : IValueSerializer
        {
            public IList<IYamlTypeConverter> Converters { get; private set; }
            private readonly SerializationOptions options;
            private readonly INamingConvention namingConvention;
            private readonly ITypeResolver typeResolver;
            private readonly YamlAttributeOverrides overrides;

            public BackwardsCompatibleConfiguration(SerializationOptions options, INamingConvention namingConvention, YamlAttributeOverrides overrides)
            {
                this.options = options;
                this.namingConvention = namingConvention ?? new NullNamingConvention();
                this.overrides = overrides;

                Converters = new List<IYamlTypeConverter>();
                Converters.Add(new GuidConverter(IsOptionSet(SerializationOptions.JsonCompatible)));

                typeResolver = IsOptionSet(SerializationOptions.DefaultToStaticType)
                    ? (ITypeResolver)new StaticTypeResolver()
                    : (ITypeResolver)new DynamicTypeResolver();
            }

            public bool IsOptionSet(SerializationOptions option)
            {
                return (options & option) != 0;
            }

            private IObjectGraphVisitor<IEmitter> CreateEmittingVisitor(IEmitter emitter, IObjectGraphTraversalStrategy traversalStrategy, IEventEmitter eventEmitter, IObjectDescriptor graph)
            {
                IObjectGraphVisitor<IEmitter> emittingVisitor = new EmittingObjectGraphVisitor(eventEmitter);

                ObjectSerializer nestedObjectSerializer = (v, t) => SerializeValue(emitter, v, t);

                emittingVisitor = new CustomSerializationObjectGraphVisitor(emittingVisitor, Converters, nestedObjectSerializer);

                if (!IsOptionSet(SerializationOptions.DisableAliases))
                {
                    var anchorAssigner = new AnchorAssigner(Converters);
                    traversalStrategy.Traverse<Nothing>(graph, anchorAssigner, null);

                    emittingVisitor = new AnchorAssigningObjectGraphVisitor(emittingVisitor, eventEmitter, anchorAssigner);
                }

                if (!IsOptionSet(SerializationOptions.EmitDefaults))
                {
                    emittingVisitor = new DefaultExclusiveObjectGraphVisitor(emittingVisitor);
                }

                return emittingVisitor;
            }

            private IEventEmitter CreateEventEmitter()
            {
                var writer = new WriterEventEmitter();

                if (IsOptionSet(SerializationOptions.JsonCompatible))
                {
                    return new JsonEventEmitter(writer);
                }
                else
                {
                    return new TypeAssigningEventEmitter(writer, IsOptionSet(SerializationOptions.Roundtrip));
                }
            }

            private IObjectGraphTraversalStrategy CreateTraversalStrategy()
            {
                ITypeInspector typeDescriptor = new ReadablePropertiesTypeInspector(typeResolver);
                if (IsOptionSet(SerializationOptions.Roundtrip))
                {
                    typeDescriptor = new ReadableAndWritablePropertiesTypeInspector(typeDescriptor);
                }

                typeDescriptor = new YamlAttributeOverridesInspector(typeDescriptor, overrides);
                typeDescriptor = new YamlAttributesTypeInspector(typeDescriptor);
                typeDescriptor = new NamingConventionTypeInspector(typeDescriptor, namingConvention);

                if (IsOptionSet(SerializationOptions.DefaultToStaticType))
                {
                    typeDescriptor = new CachedTypeInspector(typeDescriptor);
                }

                if (IsOptionSet(SerializationOptions.Roundtrip))
                {
                    return new RoundtripObjectGraphTraversalStrategy(Converters, typeDescriptor, typeResolver, 50);
                }
                else
                {
                    return new FullObjectGraphTraversalStrategy(typeDescriptor, typeResolver, 50, namingConvention);
                }
            }

            public void SerializeValue(IEmitter emitter, object value, Type type)
            {
                var graph = type != null
                    ? new ObjectDescriptor(value, type, type)
                    : new ObjectDescriptor(value, value != null ? value.GetType() : typeof(object), typeof(object));

                var traversalStrategy = CreateTraversalStrategy();
                var emittingVisitor = CreateEmittingVisitor(
                    emitter,
                    traversalStrategy,
                    CreateEventEmitter(),
                    graph
                );

                traversalStrategy.Traverse(graph, emittingVisitor, emitter);
            }
        }

        private readonly BackwardsCompatibleConfiguration backwardsCompatibleConfiguration;

        private void ThrowUnlessInBackwardsCompatibleMode()
        {
            if (backwardsCompatibleConfiguration == null)
            {
                throw new InvalidOperationException("This method / property exists for backwards compatibility reasons, but the Serializer was created using the new configuration mechanism. To configure the Serializer, use the SerializerBuilder.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options">Options that control how the serialization is to be performed.</param>
        /// <param name="namingConvention">Naming strategy to use for serialized property names</param>
        /// <param name="overrides">Yaml attribute overrides</param>
        [Obsolete("Please use SerializerBuilder to customize the Serializer. This constructor will be removed in future releases.")]
        public Serializer(SerializationOptions options = SerializationOptions.None, INamingConvention namingConvention = null, YamlAttributeOverrides overrides = null)
        {
            backwardsCompatibleConfiguration = new BackwardsCompatibleConfiguration(options, namingConvention, overrides);
        }

        /// <summary>
        /// Registers a type converter to be used to serialize and deserialize specific types.
        /// </summary>
        [Obsolete("Please use SerializerBuilder to customize the Serializer. This method will be removed in future releases.")]
        public void RegisterTypeConverter(IYamlTypeConverter converter)
        {
            ThrowUnlessInBackwardsCompatibleMode();
            backwardsCompatibleConfiguration.Converters.Insert(0, converter);
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of <see cref="Serializer" /> using the default configuration.
        /// </summary>
        /// <remarks>
        /// To customize the bahavior of the serializer, use <see cref="SerializerBuilder" />.
        /// </remarks>
        public Serializer()
        // TODO: When the backwards compatibility is dropped, uncomment the following line and remove the body of this constructor.
        //: this(new SerializerBuilder().BuildSerializerParams())
        {
            backwardsCompatibleConfiguration = new BackwardsCompatibleConfiguration(SerializationOptions.None, null, null);
        }

        /// <remarks>
        /// This constructor is private to discourage its use.
        /// To invoke it, call the <see cref="FromValueSerializer"/> method.
        /// </remarks>
        private Serializer(IValueSerializer valueSerializer)
        {
            if (valueSerializer == null)
            {
                throw new ArgumentNullException("valueSerializer");
            }

            this.valueSerializer = valueSerializer;
        }

        /// <summary>
        /// Creates a new <see cref="Serializer" /> that uses the specified <see cref="IValueSerializer" />.
        /// This method is available for advanced scenarios. The preferred way to customize the bahavior of the
        /// deserializer is to use <see cref="SerializerBuilder" />.
        /// </summary>
        public static Serializer FromValueSerializer(IValueSerializer valueSerializer)
        {
            return new Serializer(valueSerializer);
        }

        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter" /> where to serialize the object.</param>
        /// <param name="graph">The object to serialize.</param>
        public void Serialize(TextWriter writer, object graph)
        {
            Serialize(new Emitter(writer), graph);
        }

        /// <summary>
        /// Serializes the specified object into a string.
        /// </summary>
        /// <param name="graph">The object to serialize.</param>
        public string Serialize(object graph)
        {
            using (var buffer = new StringWriter())
            {
                Serialize(buffer, graph);
                return buffer.ToString();
            }
        }

        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter" /> where to serialize the object.</param>
        /// <param name="graph">The object to serialize.</param>
        /// <param name="type">The static type of the object to serialize.</param>
        public void Serialize(TextWriter writer, object graph, Type type)
        {
            Serialize(new Emitter(writer), graph, type);
        }

        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="emitter">The <see cref="IEmitter" /> where to serialize the object.</param>
        /// <param name="graph">The object to serialize.</param>
        public void Serialize(IEmitter emitter, object graph)
        {
            if (emitter == null)
            {
                throw new ArgumentNullException("emitter");
            }

            EmitDocument(emitter, graph, null);
        }

        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="emitter">The <see cref="IEmitter" /> where to serialize the object.</param>
        /// <param name="graph">The object to serialize.</param>
        /// <param name="type">The static type of the object to serialize.</param>
        public void Serialize(IEmitter emitter, object graph, Type type)
        {
            if (emitter == null)
            {
                throw new ArgumentNullException("emitter");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            EmitDocument(emitter, graph, type);
        }

        private void EmitDocument(IEmitter emitter, object graph, Type type)
        {
            emitter.Emit(new StreamStart());
            emitter.Emit(new DocumentStart());

            IValueSerializer actualValueSerializer = backwardsCompatibleConfiguration ?? valueSerializer;
            actualValueSerializer.SerializeValue(emitter, graph, type);

            emitter.Emit(new DocumentEnd(true));
            emitter.Emit(new StreamEnd());
        }
    }
}