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
        private Func<ITypeInspector, ITypeResolver, IObjectGraphTraversalStrategy> objectGraphTraversalStrategyFactory;
        private readonly LazyComponentRegistrationList<Nothing, IObjectGraphVisitor> preProcessingPhaseObjectGraphVisitorFactories;
        private readonly LazyComponentRegistrationList<EmissionPhaseObjectGraphVisitorArgs, IObjectGraphVisitor> emissionPhaseObjectGraphVisitorFactories;
        private readonly LazyComponentRegistrationList<IEventEmitter, IEventEmitter> eventEmitterFactories;


        public SerializerBuilder()
        {
            typeInspectorFactories.Add(typeof(CachedTypeInspector), inner => new CachedTypeInspector(inner));
            typeInspectorFactories.Add(typeof(YamlAttributesTypeInspector), inner => new YamlAttributesTypeInspector(inner));
            typeInspectorFactories.Add(typeof(YamlAttributeOverridesInspector), inner => overrides != null ? new YamlAttributeOverridesInspector(inner, overrides.Clone()) : inner);
            typeInspectorFactories.Add(typeof(NamingConventionTypeInspector), inner => namingConvention != null ? new NamingConventionTypeInspector(inner, namingConvention) : inner);

            preProcessingPhaseObjectGraphVisitorFactories = new LazyComponentRegistrationList<Nothing, IObjectGraphVisitor>();
            preProcessingPhaseObjectGraphVisitorFactories.Add(typeof(AnchorAssigner), _ => new AnchorAssigner());

            emissionPhaseObjectGraphVisitorFactories = new LazyComponentRegistrationList<EmissionPhaseObjectGraphVisitorArgs, IObjectGraphVisitor>();
            emissionPhaseObjectGraphVisitorFactories.Add(typeof(CustomSerializationObjectGraphVisitor),
                args => new CustomSerializationObjectGraphVisitor(args.Emitter, args.InnerVisitor, BuildTypeConverters()));

            emissionPhaseObjectGraphVisitorFactories.Add(typeof(AnchorAssigningObjectGraphVisitor),
                args => new AnchorAssigningObjectGraphVisitor(args.InnerVisitor, args.EventEmitter, args.GetPreProcessingPhaseObjectGraphVisitor<AnchorAssigner>()));

            emissionPhaseObjectGraphVisitorFactories.Add(typeof(DefaultExclusiveObjectGraphVisitor),
                args => new DefaultExclusiveObjectGraphVisitor(args.InnerVisitor));

            eventEmitterFactories = new LazyComponentRegistrationList<IEventEmitter, IEventEmitter>();
            eventEmitterFactories.Add(typeof(TypeAssigningEventEmitter), inner => new TypeAssigningEventEmitter(inner, false));

            objectGraphTraversalStrategyFactory = (typeInspector, typeResolver) => new FullObjectGraphTraversalStrategy(typeInspector, typeResolver, 50, namingConvention ?? new NullNamingConvention());

            WithTypeResolver(new DynamicTypeResolver());
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
        /// Ensures that it will be possible to deserialize the serialized objects.
        /// This option will force the emission of tags and emit only properties with setters.
        /// </summary>
        public SerializerBuilder EnsureRoundtrip()
        {
            objectGraphTraversalStrategyFactory = (typeInspector, typeResolver) => new RoundtripObjectGraphTraversalStrategy(
                BuildTypeConverters(),
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
        /// Registers an additional <see cref="IObjectGraphVisitor" /> to be used by the serializer
        /// before emitting an object graph.
        /// </summary>
        /// <remarks>
        /// Registering a visitor in the pre-processing phase enables to traverse the object graph once
        /// before actually emitting it. This allows a visitor to collect information about the graph that
        /// can be used later by another visitor registered in the emission phase.
        /// </remarks>
        /// <param name="objectGraphVisitor">The type inspector.</param>
        public SerializerBuilder WithPreProcessingPhaseObjectGraphVisitor<TObjectGraphVisitor>(TObjectGraphVisitor objectGraphVisitor)
            where TObjectGraphVisitor : IObjectGraphVisitor
        {
            return WithPreProcessingPhaseObjectGraphVisitor(objectGraphVisitor, w => w.OnTop());
        }

        /// <summary>
        /// Registers an additional <see cref="IObjectGraphVisitor" /> to be used by the serializer
        /// before emitting an object graph.
        /// </summary>
        /// <remarks>
        /// Registering a visitor in the pre-processing phase enables to traverse the object graph once
        /// before actually emitting it. This allows a visitor to collect information about the graph that
        /// can be used later by another visitor registered in the emission phase.
        /// </remarks>
        /// <param name="objectGraphVisitor">The type inspector.</param>
        /// <param name="where">Configures the location where to insert the <see cref="IObjectGraphVisitor" /></param>
        public SerializerBuilder WithPreProcessingPhaseObjectGraphVisitor<TObjectGraphVisitor>(
            TObjectGraphVisitor objectGraphVisitor,
            Action<IRegistrationLocationSelectionSyntax<IObjectGraphVisitor>> where
        )
            where TObjectGraphVisitor : IObjectGraphVisitor
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
        /// Registers an additional <see cref="IObjectGraphVisitor" /> to be used by the serializer
        /// while emitting an object graph.
        /// </summary>
        /// <param name="objectGraphVisitorFactory">A function that instantiates the type inspector.</param>
        public SerializerBuilder WithEmissionPhaseObjectGraphVisitor<TObjectGraphVisitor>(Func<EmissionPhaseObjectGraphVisitorArgs, TObjectGraphVisitor> objectGraphVisitorFactory)
            where TObjectGraphVisitor : IObjectGraphVisitor
        {
            return WithEmissionPhaseObjectGraphVisitor(objectGraphVisitorFactory, w => w.OnTop());
        }

        /// <summary>
        /// Registers an additional <see cref="IObjectGraphVisitor" /> to be used by the serializer
        /// while emitting an object graph.
        /// </summary>
        /// <param name="objectGraphVisitorFactory">A function that instantiates the type inspector.</param>
        /// <param name="where">Configures the location where to insert the <see cref="IObjectGraphVisitor" /></param>
        public SerializerBuilder WithEmissionPhaseObjectGraphVisitor<TObjectGraphVisitor>(
            Func<EmissionPhaseObjectGraphVisitorArgs, TObjectGraphVisitor> objectGraphVisitorFactory,
            Action<IRegistrationLocationSelectionSyntax<IObjectGraphVisitor>> where
        )
            where TObjectGraphVisitor : IObjectGraphVisitor
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
        /// Creates a new <see cref="Serializer" /> according to the current configuration.
        /// </summary>
        public Serializer Build()
        {
            return Serializer.FromSerializerParams(BuildSerializerParams());
        }

        /// <summary>
        /// Creates a new <see cref="SerializerParams" /> that implements the current configuration.
        /// This method is available for advanced scenarios. The preferred way to customize the bahavior of the
        /// deserializer is to use the <see cref="Build" /> method.
        /// </summary>
        public SerializerParams BuildSerializerParams()
        {
            var traversalStrategy = CreateTraversalStrategy();
            return new SerializerParams(traversalStrategy, (emitter, graph) => CreateEmittingVisitor(traversalStrategy, emitter, graph));
        }

        private IObjectGraphVisitor CreateEmittingVisitor(IObjectGraphTraversalStrategy traversalStrategy, IEmitter emitter, IObjectDescriptor graph)
        {
            var eventEmitter = CreateEventEmitter(emitter);

            var preProcessingPhaseObjectGraphVisitors = preProcessingPhaseObjectGraphVisitorFactories.BuildComponentList();
            foreach (var visitor in preProcessingPhaseObjectGraphVisitors)
            {
                traversalStrategy.Traverse(graph, visitor);
            }

            return emissionPhaseObjectGraphVisitorFactories.BuildComponentChain(
                new EmittingObjectGraphVisitor(eventEmitter),
                inner => new EmissionPhaseObjectGraphVisitorArgs(inner, emitter, eventEmitter, preProcessingPhaseObjectGraphVisitors)
            );
        }

        private IEventEmitter CreateEventEmitter(IEmitter emitter)
        {
            return eventEmitterFactories.BuildComponentChain(
                new WriterEventEmitter(emitter)
            );
        }

        private IObjectGraphTraversalStrategy CreateTraversalStrategy()
        {
            var typeInspector = BuildTypeInspector();
            return objectGraphTraversalStrategyFactory(typeInspector, typeResolver);
        }
    }
}