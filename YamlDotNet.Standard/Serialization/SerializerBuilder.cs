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
using YamlDotNet.Core;
using YamlDotNet.Serialization.Converters;
using YamlDotNet.Serialization.EventEmitters;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.ObjectGraphTraversalStrategies;
using YamlDotNet.Serialization.ObjectGraphVisitors;
using YamlDotNet.Serialization.TypeInspectors;
using YamlDotNet.Serialization.TypeResolvers;

namespace YamlDotNet.Serialization
{
    /// <summary>
    /// Creates and configures instances of <see cref="Serializer" />.
    /// This class is used to customize the behavior of <see cref="Serializer" />. Use the relevant methods
    /// to apply customizations, then call <see cref="Build" /> to create an instance of the serializer
    /// with the desired customizations.
    /// </summary>
    public sealed class SerializerBuilder : BuilderSkeleton<SerializerBuilder>
    {
        private Func<ITypeInspector, ITypeResolver, IEnumerable<IYamlTypeConverter>, IObjectGraphTraversalStrategy> objectGraphTraversalStrategyFactory;
        private readonly LazyComponentRegistrationList<IEnumerable<IYamlTypeConverter>, IObjectGraphVisitor<Nothing>> preProcessingPhaseObjectGraphVisitorFactories;
        private readonly LazyComponentRegistrationList<EmissionPhaseObjectGraphVisitorArgs, IObjectGraphVisitor<IEmitter>> emissionPhaseObjectGraphVisitorFactories;
        private readonly LazyComponentRegistrationList<IEventEmitter, IEventEmitter> eventEmitterFactories;
        private readonly IDictionary<Type, string> tagMappings = new Dictionary<Type, string>();

        public SerializerBuilder()
        {
            typeInspectorFactories.Add(typeof(CachedTypeInspector), inner => new CachedTypeInspector(inner));
            typeInspectorFactories.Add(typeof(NamingConventionTypeInspector), inner => namingConvention != null ? new NamingConventionTypeInspector(inner, namingConvention) : inner);
            typeInspectorFactories.Add(typeof(YamlAttributesTypeInspector), inner => new YamlAttributesTypeInspector(inner));
            typeInspectorFactories.Add(typeof(YamlAttributeOverridesInspector), inner => overrides != null ? new YamlAttributeOverridesInspector(inner, overrides.Clone()) : inner);

            preProcessingPhaseObjectGraphVisitorFactories = new LazyComponentRegistrationList<IEnumerable<IYamlTypeConverter>, IObjectGraphVisitor<Nothing>>();
            preProcessingPhaseObjectGraphVisitorFactories.Add(typeof(AnchorAssigner), typeConverters => new AnchorAssigner(typeConverters));

            emissionPhaseObjectGraphVisitorFactories = new LazyComponentRegistrationList<EmissionPhaseObjectGraphVisitorArgs, IObjectGraphVisitor<IEmitter>>();
            emissionPhaseObjectGraphVisitorFactories.Add(typeof(CustomSerializationObjectGraphVisitor),
                args => new CustomSerializationObjectGraphVisitor(args.InnerVisitor, args.TypeConverters, args.NestedObjectSerializer));

            emissionPhaseObjectGraphVisitorFactories.Add(typeof(AnchorAssigningObjectGraphVisitor),
                args => new AnchorAssigningObjectGraphVisitor(args.InnerVisitor, args.EventEmitter, args.GetPreProcessingPhaseObjectGraphVisitor<AnchorAssigner>()));

            emissionPhaseObjectGraphVisitorFactories.Add(typeof(DefaultExclusiveObjectGraphVisitor),
                args => new DefaultExclusiveObjectGraphVisitor(args.InnerVisitor));

            eventEmitterFactories = new LazyComponentRegistrationList<IEventEmitter, IEventEmitter>();
            eventEmitterFactories.Add(typeof(TypeAssigningEventEmitter), inner => new TypeAssigningEventEmitter(inner, false));

            objectGraphTraversalStrategyFactory = (typeInspector, typeResolver, typeConverters) => new FullObjectGraphTraversalStrategy(typeInspector, typeResolver, 50, namingConvention ?? new NullNamingConvention());

            WithTypeResolver(new DynamicTypeResolver());
            WithEventEmitter(inner => new CustomTagEventEmitter(inner, tagMappings));
        }

        protected override SerializerBuilder Self { get { return this; } }

        /// <summary>
        /// Registers an additional <see cref="IEventEmitter" /> to be used by the serializer.
        /// </summary>
        /// <param name="eventEmitterFactory">A function that instantiates the event emitter.</param>
        public SerializerBuilder WithEventEmitter<TEventEmitter>(Func<IEventEmitter, TEventEmitter> eventEmitterFactory)
            where TEventEmitter : IEventEmitter
        {
            return WithEventEmitter(eventEmitterFactory, w => w.OnTop());
        }

        /// <summary>
        /// Registers an additional <see cref="IEventEmitter" /> to be used by the serializer.
        /// </summary>
        /// <param name="eventEmitterFactory">A function that instantiates the event emitter.</param>
        /// <param name="where">Configures the location where to insert the <see cref="IEventEmitter" /></param>
        public SerializerBuilder WithEventEmitter<TEventEmitter>(
            Func<IEventEmitter, TEventEmitter> eventEmitterFactory,
            Action<IRegistrationLocationSelectionSyntax<IEventEmitter>> where
        )
            where TEventEmitter : IEventEmitter
        {
            if (eventEmitterFactory == null)
            {
                throw new ArgumentNullException("eventEmitterFactory");
            }

            if (where == null)
            {
                throw new ArgumentNullException("where");
            }

            where(eventEmitterFactories.CreateRegistrationLocationSelector(typeof(TEventEmitter), inner => eventEmitterFactory(inner)));
            return Self;
        }

        /// <summary>
        /// Registers an additional <see cref="IEventEmitter" /> to be used by the serializer.
        /// </summary>
        /// <param name="eventEmitterFactory">A function that instantiates the event emitter based on a previously registered <see cref="IEventEmitter" />.</param>
        /// <param name="where">Configures the location where to insert the <see cref="IEventEmitter" /></param>
        public SerializerBuilder WithEventEmitter<TEventEmitter>(
            WrapperFactory<IEventEmitter, IEventEmitter, TEventEmitter> eventEmitterFactory,
            Action<ITrackingRegistrationLocationSelectionSyntax<IEventEmitter>> where
        )
            where TEventEmitter : IEventEmitter
        {
            if (eventEmitterFactory == null)
            {
                throw new ArgumentNullException("eventEmitterFactory");
            }

            if (where == null)
            {
                throw new ArgumentNullException("where");
            }

            where(eventEmitterFactories.CreateTrackingRegistrationLocationSelector(typeof(TEventEmitter), (wrapped, inner) => eventEmitterFactory(wrapped, inner)));
            return Self;
        }

        /// <summary>
        /// Unregisters an existing <see cref="IEventEmitter" /> of type <typeparam name="TEventEmitter" />.
        /// </summary>
        public SerializerBuilder WithoutEventEmitter<TEventEmitter>()
            where TEventEmitter : IEventEmitter
        {
            return WithoutEventEmitter(typeof(TEventEmitter));
        }

        /// <summary>
        /// Unregisters an existing <see cref="IEventEmitter" /> of type <param name="eventEmitterType" />.
        /// </summary>
        public SerializerBuilder WithoutEventEmitter(Type eventEmitterType)
        {
            if (eventEmitterType == null)
            {
                throw new ArgumentNullException("eventEmitterType");
            }

            eventEmitterFactories.Remove(eventEmitterType);
            return this;
        }

        /// <summary>
        /// Registers a tag mapping.
        /// </summary>
        public SerializerBuilder WithTagMapping(string tag, Type type)
        {
            if (tag == null)
            {
                throw new ArgumentNullException("tag");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            string alreadyRegisteredTag;
            if (tagMappings.TryGetValue(type, out alreadyRegisteredTag))
            {
                throw new ArgumentException(string.Format("Type already has a registered tag '{0}' for type '{1}'", alreadyRegisteredTag, type.FullName), "type");
            }

            tagMappings.Add(type, tag);
            return this;
        }

        /// <summary>
        /// Unregisters an existing tag mapping.
        /// </summary>
        public SerializerBuilder WithoutTagMapping(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (!tagMappings.Remove(type))
            {
                throw new KeyNotFoundException(string.Format("Tag for type '{0}' is not registered", type.FullName));
            }
            return this;
        }

        /// <summary>
        /// Ensures that it will be possible to deserialize the serialized objects.
        /// This option will force the emission of tags and emit only properties with setters.
        /// </summary>
        public SerializerBuilder EnsureRoundtrip()
        {
            objectGraphTraversalStrategyFactory = (typeInspector, typeResolver, typeConverters) => new RoundtripObjectGraphTraversalStrategy(
                typeConverters,
                typeInspector,
                typeResolver,
                50
            );
            WithEventEmitter(inner => new TypeAssigningEventEmitter(inner, true), loc => loc.InsteadOf<TypeAssigningEventEmitter>());
            return WithTypeInspector(inner => new ReadableAndWritablePropertiesTypeInspector(inner), loc => loc.OnBottom());
        }

        /// <summary>
        /// Specifies that, if the same object appears more than once in the
        /// serialization graph, it will be serialized each time instead of just once.
        /// </summary>
        /// <remarks>
        /// If the serialization graph contains circular references and this flag is set,
        /// a StackOverflowException will be thrown.
        /// If this flag is not set, there is a performance penalty because the entire
        /// object graph must be walked twice.
        /// </remarks>
        public SerializerBuilder DisableAliases()
        {
            preProcessingPhaseObjectGraphVisitorFactories.Remove(typeof(AnchorAssigner));
            emissionPhaseObjectGraphVisitorFactories.Remove(typeof(AnchorAssigningObjectGraphVisitor));
            return this;
        }

        /// <summary>
        /// Forces every value to be serialized, even if it is the default value for that type.
        /// </summary>
        public SerializerBuilder EmitDefaults()
        {
            emissionPhaseObjectGraphVisitorFactories.Remove(typeof(DefaultExclusiveObjectGraphVisitor));
            return this;
        }

        /// <summary>
        /// Ensures that the result of the serialization is valid JSON.
        /// </summary>
        public SerializerBuilder JsonCompatible()
        {
            return this
                .WithTypeConverter(new GuidConverter(true), w => w.InsteadOf<GuidConverter>())
                .WithEventEmitter(inner => new JsonEventEmitter(inner), loc => loc.InsteadOf<TypeAssigningEventEmitter>());
        }

        /// <summary>
        /// Registers an additional <see cref="IObjectGraphVisitor{Nothing}" /> to be used by the serializer
        /// before emitting an object graph.
        /// </summary>
        /// <remarks>
        /// Registering a visitor in the pre-processing phase enables to traverse the object graph once
        /// before actually emitting it. This allows a visitor to collect information about the graph that
        /// can be used later by another visitor registered in the emission phase.
        /// </remarks>
        /// <param name="objectGraphVisitor">The type inspector.</param>
        public SerializerBuilder WithPreProcessingPhaseObjectGraphVisitor<TObjectGraphVisitor>(TObjectGraphVisitor objectGraphVisitor)
            where TObjectGraphVisitor : IObjectGraphVisitor<Nothing>
        {
            return WithPreProcessingPhaseObjectGraphVisitor(objectGraphVisitor, w => w.OnTop());
        }

        /// <summary>
        /// Registers an additional <see cref="IObjectGraphVisitor{Nothing}" /> to be used by the serializer
        /// before emitting an object graph.
        /// </summary>
        /// <remarks>
        /// Registering a visitor in the pre-processing phase enables to traverse the object graph once
        /// before actually emitting it. This allows a visitor to collect information about the graph that
        /// can be used later by another visitor registered in the emission phase.
        /// </remarks>
        /// <param name="objectGraphVisitor">The type inspector.</param>
        /// <param name="where">Configures the location where to insert the <see cref="IObjectGraphVisitor{Nothing}" /></param>
        public SerializerBuilder WithPreProcessingPhaseObjectGraphVisitor<TObjectGraphVisitor>(
            TObjectGraphVisitor objectGraphVisitor,
            Action<IRegistrationLocationSelectionSyntax<IObjectGraphVisitor<Nothing>>> where
        )
            where TObjectGraphVisitor : IObjectGraphVisitor<Nothing>
        {
            if (objectGraphVisitor == null)
            {
                throw new ArgumentNullException("objectGraphVisitor");
            }

            if (where == null)
            {
                throw new ArgumentNullException("where");
            }

            where(preProcessingPhaseObjectGraphVisitorFactories.CreateRegistrationLocationSelector(typeof(TObjectGraphVisitor), _ => objectGraphVisitor));
            return this;
        }

        /// <summary>
        /// Registers an additional <see cref="IObjectGraphVisitor{Nothing}" /> to be used by the serializer
        /// before emitting an object graph.
        /// </summary>
        /// <remarks>
        /// Registering a visitor in the pre-processing phase enables to traverse the object graph once
        /// before actually emitting it. This allows a visitor to collect information about the graph that
        /// can be used later by another visitor registered in the emission phase.
        /// </remarks>
        /// <param name="objectGraphVisitorFactory">A factory that creates the <see cref="IObjectGraphVisitor{Nothing}" /> based on a previously registered <see cref="IObjectGraphVisitor{Nothing}" />.</param>
        /// <param name="where">Configures the location where to insert the <see cref="IObjectGraphVisitor{Nothing}" /></param>
        public SerializerBuilder WithPreProcessingPhaseObjectGraphVisitor<TObjectGraphVisitor>(
            WrapperFactory<IObjectGraphVisitor<Nothing>, TObjectGraphVisitor> objectGraphVisitorFactory,
            Action<ITrackingRegistrationLocationSelectionSyntax<IObjectGraphVisitor<Nothing>>> where
        )
            where TObjectGraphVisitor : IObjectGraphVisitor<Nothing>
        {
            if (objectGraphVisitorFactory == null)
            {
                throw new ArgumentNullException("objectGraphVisitorFactory");
            }

            if (where == null)
            {
                throw new ArgumentNullException("where");
            }

            where(preProcessingPhaseObjectGraphVisitorFactories.CreateTrackingRegistrationLocationSelector(typeof(TObjectGraphVisitor), (wrapped, _) => objectGraphVisitorFactory(wrapped)));
            return this;
        }

        /// <summary>
        /// Unregisters an existing <see cref="IObjectGraphVisitor{Nothing}" /> of type <typeparam name="TObjectGraphVisitor" />.
        /// </summary>
        public SerializerBuilder WithoutPreProcessingPhaseObjectGraphVisitor<TObjectGraphVisitor>()
            where TObjectGraphVisitor : IObjectGraphVisitor<Nothing>
        {
            return WithoutPreProcessingPhaseObjectGraphVisitor(typeof(TObjectGraphVisitor));
        }

        /// <summary>
        /// Unregisters an existing <see cref="IObjectGraphVisitor{Nothing}" /> of type <param name="objectGraphVisitorType" />.
        /// </summary>
        public SerializerBuilder WithoutPreProcessingPhaseObjectGraphVisitor(Type objectGraphVisitorType)
        {
            if (objectGraphVisitorType == null)
            {
                throw new ArgumentNullException("objectGraphVisitorType");
            }

            preProcessingPhaseObjectGraphVisitorFactories.Remove(objectGraphVisitorType);
            return this;
        }

        /// <summary>
        /// Registers an additional <see cref="IObjectGraphVisitor{IEmitter}" /> to be used by the serializer
        /// while emitting an object graph.
        /// </summary>
        /// <param name="objectGraphVisitorFactory">A function that instantiates the type inspector.</param>
        public SerializerBuilder WithEmissionPhaseObjectGraphVisitor<TObjectGraphVisitor>(Func<EmissionPhaseObjectGraphVisitorArgs, TObjectGraphVisitor> objectGraphVisitorFactory)
            where TObjectGraphVisitor : IObjectGraphVisitor<IEmitter>
        {
            return WithEmissionPhaseObjectGraphVisitor(objectGraphVisitorFactory, w => w.OnTop());
        }

        /// <summary>
        /// Registers an additional <see cref="IObjectGraphVisitor{IEmitter}" /> to be used by the serializer
        /// while emitting an object graph.
        /// </summary>
        /// <param name="objectGraphVisitorFactory">A function that instantiates the type inspector.</param>
        /// <param name="where">Configures the location where to insert the <see cref="IObjectGraphVisitor{IEmitter}" /></param>
        public SerializerBuilder WithEmissionPhaseObjectGraphVisitor<TObjectGraphVisitor>(
            Func<EmissionPhaseObjectGraphVisitorArgs, TObjectGraphVisitor> objectGraphVisitorFactory,
            Action<IRegistrationLocationSelectionSyntax<IObjectGraphVisitor<IEmitter>>> where
        )
            where TObjectGraphVisitor : IObjectGraphVisitor<IEmitter>
        {
            if (objectGraphVisitorFactory == null)
            {
                throw new ArgumentNullException("objectGraphVisitorFactory");
            }

            if (where == null)
            {
                throw new ArgumentNullException("where");
            }

            where(emissionPhaseObjectGraphVisitorFactories.CreateRegistrationLocationSelector(typeof(TObjectGraphVisitor), args => objectGraphVisitorFactory(args)));
            return this;
        }

        /// <summary>
        /// Registers an additional <see cref="IObjectGraphVisitor{IEmitter}" /> to be used by the serializer
        /// while emitting an object graph.
        /// </summary>
        /// <param name="objectGraphVisitorFactory">A function that instantiates the type inspector based on a previously registered <see cref="IObjectGraphVisitor{IEmitter}" />.</param>
        /// <param name="where">Configures the location where to insert the <see cref="IObjectGraphVisitor{IEmitter}" /></param>
        public SerializerBuilder WithEmissionPhaseObjectGraphVisitor<TObjectGraphVisitor>(
            WrapperFactory<EmissionPhaseObjectGraphVisitorArgs, IObjectGraphVisitor<IEmitter>, TObjectGraphVisitor> objectGraphVisitorFactory,
            Action<ITrackingRegistrationLocationSelectionSyntax<IObjectGraphVisitor<IEmitter>>> where
        )
            where TObjectGraphVisitor : IObjectGraphVisitor<IEmitter>
        {
            if (objectGraphVisitorFactory == null)
            {
                throw new ArgumentNullException("objectGraphVisitorFactory");
            }

            if (where == null)
            {
                throw new ArgumentNullException("where");
            }

            where(emissionPhaseObjectGraphVisitorFactories.CreateTrackingRegistrationLocationSelector(typeof(TObjectGraphVisitor), (wrapped, args) => objectGraphVisitorFactory(wrapped, args)));
            return this;
        }

        /// <summary>
        /// Unregisters an existing <see cref="IObjectGraphVisitor{IEmitter}" /> of type <typeparam name="TObjectGraphVisitor" />.
        /// </summary>
        public SerializerBuilder WithoutEmissionPhaseObjectGraphVisitor<TObjectGraphVisitor>()
            where TObjectGraphVisitor : IObjectGraphVisitor<IEmitter>
        {
            return WithoutEmissionPhaseObjectGraphVisitor(typeof(TObjectGraphVisitor));
        }

        /// <summary>
        /// Unregisters an existing <see cref="IObjectGraphVisitor{IEmitter}" /> of type <param name="objectGraphVisitorType" />.
        /// </summary>
        public SerializerBuilder WithoutEmissionPhaseObjectGraphVisitor(Type objectGraphVisitorType)
        {
            if (objectGraphVisitorType == null)
            {
                throw new ArgumentNullException("objectGraphVisitorType");
            }

            emissionPhaseObjectGraphVisitorFactories.Remove(objectGraphVisitorType);
            return this;
        }

        /// <summary>
        /// Creates a new <see cref="Serializer" /> according to the current configuration.
        /// </summary>
        public Serializer Build()
        {
            return Serializer.FromValueSerializer(BuildValueSerializer());
        }

        /// <summary>
        /// Creates a new <see cref="IValueDeserializer" /> that implements the current configuration.
        /// This method is available for advanced scenarios. The preferred way to customize the bahavior of the
        /// deserializer is to use the <see cref="Build" /> method.
        /// </summary>
        public IValueSerializer BuildValueSerializer()
        {
            var typeConverters = BuildTypeConverters();
            var typeInspector = BuildTypeInspector();
            var traversalStrategy = objectGraphTraversalStrategyFactory(typeInspector, typeResolver, typeConverters);
            var eventEmitter = eventEmitterFactories.BuildComponentChain(new WriterEventEmitter());

            return new ValueSerializer(
                traversalStrategy,
                eventEmitter,
                typeConverters,
                preProcessingPhaseObjectGraphVisitorFactories.Clone(),
                emissionPhaseObjectGraphVisitorFactories.Clone()
            );
        }

        private class ValueSerializer : IValueSerializer
        {
            private readonly IObjectGraphTraversalStrategy traversalStrategy;
            private readonly IEventEmitter eventEmitter;
            private readonly IEnumerable<IYamlTypeConverter> typeConverters;
            private readonly LazyComponentRegistrationList<IEnumerable<IYamlTypeConverter>, IObjectGraphVisitor<Nothing>> preProcessingPhaseObjectGraphVisitorFactories;
            private readonly LazyComponentRegistrationList<EmissionPhaseObjectGraphVisitorArgs, IObjectGraphVisitor<IEmitter>> emissionPhaseObjectGraphVisitorFactories;

            public ValueSerializer(
                IObjectGraphTraversalStrategy traversalStrategy,
                IEventEmitter eventEmitter,
                IEnumerable<IYamlTypeConverter> typeConverters,
                LazyComponentRegistrationList<IEnumerable<IYamlTypeConverter>, IObjectGraphVisitor<Nothing>> preProcessingPhaseObjectGraphVisitorFactories,
                LazyComponentRegistrationList<EmissionPhaseObjectGraphVisitorArgs, IObjectGraphVisitor<IEmitter>> emissionPhaseObjectGraphVisitorFactories
            )
            {
                this.traversalStrategy = traversalStrategy;
                this.eventEmitter = eventEmitter;
                this.typeConverters = typeConverters;
                this.preProcessingPhaseObjectGraphVisitorFactories = preProcessingPhaseObjectGraphVisitorFactories;
                this.emissionPhaseObjectGraphVisitorFactories = emissionPhaseObjectGraphVisitorFactories;
            }

            public void SerializeValue(IEmitter emitter, object value, Type type)
            {
                var actualType = type != null ? type : value != null ? value.GetType() : typeof(object);
                var staticType = type ?? typeof(object);

                var graph = new ObjectDescriptor(value, actualType, staticType);

                var preProcessingPhaseObjectGraphVisitors = preProcessingPhaseObjectGraphVisitorFactories.BuildComponentList(typeConverters);
                foreach (var visitor in preProcessingPhaseObjectGraphVisitors)
                {
                    traversalStrategy.Traverse(graph, visitor, null);
                }

                ObjectSerializer nestedObjectSerializer = (v, t) => SerializeValue(emitter, v, t);

                var emittingVisitor = emissionPhaseObjectGraphVisitorFactories.BuildComponentChain(
                    new EmittingObjectGraphVisitor(eventEmitter),
                    inner => new EmissionPhaseObjectGraphVisitorArgs(inner, eventEmitter, preProcessingPhaseObjectGraphVisitors, typeConverters, nestedObjectSerializer)
                );

                traversalStrategy.Traverse(graph, emittingVisitor, emitter);
            }
        }
    }
}