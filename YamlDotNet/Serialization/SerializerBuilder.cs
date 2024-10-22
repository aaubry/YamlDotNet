// This file is part of YamlDotNet - A .NET library for YAML.
// Copyright (c) Antoine Aubry and contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using YamlDotNet.Core;
using YamlDotNet.Helpers;
using YamlDotNet.Serialization.Converters;
using YamlDotNet.Serialization.EventEmitters;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.ObjectFactories;
using YamlDotNet.Serialization.ObjectGraphTraversalStrategies;
using YamlDotNet.Serialization.ObjectGraphVisitors;
using YamlDotNet.Serialization.TypeInspectors;
using YamlDotNet.Serialization.TypeResolvers;

namespace YamlDotNet.Serialization
{
#pragma warning disable IDE0055 // Fix formatting
    /// <summary>
    /// Creates and configures instances of <see cref="Serializer" />.
    /// This class is used to customize the behavior of <see cref="Serializer" />. Use the relevant methods
    /// to apply customizations, then call <see cref="Build" /> to create an instance of the serializer
    /// with the desired customizations.
    /// </summary>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("This builder configures the serializer to use reflection which is not compatible with ahead-of-time compilation or assembly trimming." +
        " You need to use the code generator/analyzer to generate static code and use the 'StaticSerializerBuilder' object instead of this one.")]
#endif
    public sealed class SerializerBuilder : BuilderSkeleton<SerializerBuilder>
    {
#pragma warning restore IDE0055
        private ObjectGraphTraversalStrategyFactory objectGraphTraversalStrategyFactory;
        private readonly LazyComponentRegistrationList<IEnumerable<IYamlTypeConverter>, IObjectGraphVisitor<Nothing>> preProcessingPhaseObjectGraphVisitorFactories;
        private readonly LazyComponentRegistrationList<EmissionPhaseObjectGraphVisitorArgs, IObjectGraphVisitor<IEmitter>> emissionPhaseObjectGraphVisitorFactories;
        private readonly LazyComponentRegistrationList<IEventEmitter, IEventEmitter> eventEmitterFactories;
        private readonly Dictionary<Type, TagName> tagMappings = new ();
        private readonly IObjectFactory objectFactory;
        private int maximumRecursion = 50;
        private EmitterSettings emitterSettings = EmitterSettings.Default;
        private DefaultValuesHandling defaultValuesHandlingConfiguration = DefaultValuesHandling.Preserve;
        private ScalarStyle defaultScalarStyle = ScalarStyle.Any;
        private bool quoteNecessaryStrings;
        private bool quoteYaml1_1Strings;

        public SerializerBuilder()
            : base(new DynamicTypeResolver())
        {
            typeInspectorFactories.Add(typeof(CachedTypeInspector), inner => new CachedTypeInspector(inner));
            typeInspectorFactories.Add(typeof(NamingConventionTypeInspector), inner => namingConvention is NullNamingConvention ? inner : new NamingConventionTypeInspector(inner, namingConvention));
            typeInspectorFactories.Add(typeof(YamlAttributesTypeInspector), inner => new YamlAttributesTypeInspector(inner));
            typeInspectorFactories.Add(typeof(YamlAttributeOverridesInspector), inner => overrides != null ? new YamlAttributeOverridesInspector(inner, overrides.Clone()) : inner);

            preProcessingPhaseObjectGraphVisitorFactories = new LazyComponentRegistrationList<IEnumerable<IYamlTypeConverter>, IObjectGraphVisitor<Nothing>>
            {
                { typeof(AnchorAssigner), typeConverters => new AnchorAssigner(typeConverters) }
            };

            emissionPhaseObjectGraphVisitorFactories = new LazyComponentRegistrationList<EmissionPhaseObjectGraphVisitorArgs, IObjectGraphVisitor<IEmitter>>
            {
                {
                    typeof(CustomSerializationObjectGraphVisitor),
                    args => new CustomSerializationObjectGraphVisitor(args.InnerVisitor, args.TypeConverters, args.NestedObjectSerializer)
                },
                {
                    typeof(AnchorAssigningObjectGraphVisitor),
                    args => new AnchorAssigningObjectGraphVisitor(args.InnerVisitor, args.EventEmitter, args.GetPreProcessingPhaseObjectGraphVisitor<AnchorAssigner>())
                },
                {
                    typeof(DefaultValuesObjectGraphVisitor),
                    args => new DefaultValuesObjectGraphVisitor(defaultValuesHandlingConfiguration, args.InnerVisitor, new DefaultObjectFactory())
                },
                {
                    typeof(CommentsObjectGraphVisitor),
                    args => new CommentsObjectGraphVisitor(args.InnerVisitor)
                }
            };

            eventEmitterFactories = new LazyComponentRegistrationList<IEventEmitter, IEventEmitter>
            {
                {
                    typeof(TypeAssigningEventEmitter), inner =>
                        new TypeAssigningEventEmitter(inner,
                            tagMappings,
                            quoteNecessaryStrings,
                            quoteYaml1_1Strings,
                            defaultScalarStyle,
                            yamlFormatter,
                            enumNamingConvention,
                            BuildTypeInspector())
                }
            };

            objectFactory = new DefaultObjectFactory();

            objectGraphTraversalStrategyFactory = (typeInspector, typeResolver, typeConverters, maximumRecursion) =>
                new FullObjectGraphTraversalStrategy(typeInspector, typeResolver, maximumRecursion, namingConvention, objectFactory);
        }

        protected override SerializerBuilder Self { get { return this; } }

        /// <summary>
        /// Put double quotes around strings that need it, for example Null, True, False, a number. This should be called before any other "With" methods if you want this feature enabled.
        /// </summary>
        /// <param name="quoteYaml1_1Strings">Also quote strings that are valid scalars in the YAML 1.1 specification (which includes boolean Yes/No/On/Off, base 60 numbers and more)</param>
        public SerializerBuilder WithQuotingNecessaryStrings(bool quoteYaml1_1Strings = false)
        {
            quoteNecessaryStrings = true;
            this.quoteYaml1_1Strings = quoteYaml1_1Strings;
            return this;
        }

        /// <summary>
        /// Sets the default quoting style for scalar values. The default value is <see cref="ScalarStyle.Any"/>
        /// </summary>
        public SerializerBuilder WithDefaultScalarStyle(ScalarStyle style)
        {
            this.defaultScalarStyle = style;
            return this;
        }

        /// <summary>
        /// Sets the maximum recursion that is allowed while traversing the object graph. The default value is 50.
        /// </summary>
        public SerializerBuilder WithMaximumRecursion(int maximumRecursion)
        {
            if (maximumRecursion <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumRecursion), $"The maximum recursion specified ({maximumRecursion}) is invalid. It should be a positive integer.");
            }

            this.maximumRecursion = maximumRecursion;
            return this;
        }

        /// <summary>
        /// Registers an additional <see cref="IEventEmitter" /> to be used by the serializer.
        /// </summary>
        /// <param name="eventEmitterFactory">A function that instantiates the event emitter.</param>
        public SerializerBuilder WithEventEmitter<TEventEmitter>(Func<IEventEmitter, TEventEmitter> eventEmitterFactory)
            where TEventEmitter : IEventEmitter => WithEventEmitter(eventEmitterFactory, w => w.OnTop());

        /// <summary>
        /// Registers an additional <see cref="IEventEmitter" /> to be used by the serializer.
        /// </summary>
        /// <param name="eventEmitterFactory">A function that instantiates the event emitter.</param>
        public SerializerBuilder WithEventEmitter<TEventEmitter>(Func<IEventEmitter, ITypeInspector, TEventEmitter> eventEmitterFactory)
            where TEventEmitter : IEventEmitter => WithEventEmitter(eventEmitterFactory, w => w.OnTop());

        /// <summary>
        /// Registers an additional <see cref="IEventEmitter" /> to be used by the serializer.
        /// </summary>
        /// <param name="eventEmitterFactory">A function that instantiates the event emitter.</param>
        /// <param name="where">Configures the location where to insert the <see cref="IEventEmitter" /></param>
        public SerializerBuilder WithEventEmitter<TEventEmitter>(
            Func<IEventEmitter, TEventEmitter> eventEmitterFactory,
            Action<IRegistrationLocationSelectionSyntax<IEventEmitter>> where
        ) where TEventEmitter : IEventEmitter => WithEventEmitter((IEventEmitter e, ITypeInspector _) => eventEmitterFactory(e), where);

        /// <summary>
        /// Registers an additional <see cref="IEventEmitter" /> to be used by the serializer.
        /// </summary>
        /// <param name="eventEmitterFactory">A function that instantiates the event emitter.</param>
        /// <param name="where">Configures the location where to insert the <see cref="IEventEmitter" /></param>
        public SerializerBuilder WithEventEmitter<TEventEmitter>(
            Func<IEventEmitter, ITypeInspector, TEventEmitter> eventEmitterFactory,
            Action<IRegistrationLocationSelectionSyntax<IEventEmitter>> where
        )
            where TEventEmitter : IEventEmitter
        {
            if (eventEmitterFactory == null)
            {
                throw new ArgumentNullException(nameof(eventEmitterFactory));
            }

            if (where == null)
            {
                throw new ArgumentNullException(nameof(where));
            }

            where(eventEmitterFactories.CreateRegistrationLocationSelector(typeof(TEventEmitter), inner => eventEmitterFactory(inner, BuildTypeInspector())));
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
                throw new ArgumentNullException(nameof(eventEmitterFactory));
            }

            if (where == null)
            {
                throw new ArgumentNullException(nameof(where));
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
                throw new ArgumentNullException(nameof(eventEmitterType));
            }

            eventEmitterFactories.Remove(eventEmitterType);
            return this;
        }

        /// <summary>
        /// Registers a tag mapping.
        /// </summary>
        public override SerializerBuilder WithTagMapping(TagName tag, Type type)
        {
            if (tag.IsEmpty)
            {
                throw new ArgumentException("Non-specific tags cannot be maped");
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (tagMappings.TryGetValue(type, out var alreadyRegisteredTag))
            {
                throw new ArgumentException($"Type already has a registered tag '{alreadyRegisteredTag}' for type '{type.FullName}'", nameof(type));
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
                throw new ArgumentNullException(nameof(type));
            }

            if (!tagMappings.Remove(type))
            {
                throw new KeyNotFoundException($"Tag for type '{type.FullName}' is not registered");
            }
            return this;
        }

        /// <summary>
        /// Ensures that it will be possible to deserialize the serialized objects.
        /// This option will force the emission of tags and emit only properties with setters.
        /// </summary>
        public SerializerBuilder EnsureRoundtrip()
        {
            objectGraphTraversalStrategyFactory = (typeInspector, typeResolver, typeConverters, maximumRecursion) => new RoundtripObjectGraphTraversalStrategy(
                typeConverters,
                typeInspector,
                typeResolver,
                maximumRecursion,
                namingConvention,
                settings,
                objectFactory
            );

            WithEventEmitter(inner =>
                new TypeAssigningEventEmitter(inner,
                    tagMappings,
                    quoteNecessaryStrings,
                    quoteYaml1_1Strings,
                    defaultScalarStyle,
                    yamlFormatter,
                    enumNamingConvention,
                    BuildTypeInspector()), loc => loc.InsteadOf<TypeAssigningEventEmitter>());

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
        [Obsolete("The default behavior is now to always emit default values, thefore calling this method has no effect. This behavior is now controlled by ConfigureDefaultValuesHandling.", error: true)]
        public SerializerBuilder EmitDefaults() => ConfigureDefaultValuesHandling(DefaultValuesHandling.Preserve);

        /// <summary>
        /// Configures how properties with default and null values should be handled. The default value is DefaultValuesHandling.Preserve
        /// </summary>
        /// <remarks>
        /// If more control is needed, create a class that extends from ChainedObjectGraphVisitor and override its EnterMapping methods.
        /// Then register it as follows:
        /// WithEmissionPhaseObjectGraphVisitor(args => new MyDefaultHandlingStrategy(args.InnerVisitor));
        /// </remarks>
        public SerializerBuilder ConfigureDefaultValuesHandling(DefaultValuesHandling configuration)
        {
            this.defaultValuesHandlingConfiguration = configuration;
            return this;
        }

        /// <summary>
        /// Ensures that the result of the serialization is valid JSON.
        /// </summary>
        public SerializerBuilder JsonCompatible()
        {
            this.emitterSettings = this.emitterSettings
                                       .WithMaxSimpleKeyLength(int.MaxValue)
                                       .WithoutAnchorName();

            return this
                .WithTypeConverter(new GuidConverter(true), w => w.InsteadOf<GuidConverter>())
                .WithTypeConverter(new DateTime8601Converter(ScalarStyle.DoubleQuoted))
#if NET6_0_OR_GREATER
                .WithTypeConverter(new DateOnlyConverter(doubleQuotes: true))
                .WithTypeConverter(new TimeOnlyConverter(doubleQuotes: true))
#endif
                .WithEventEmitter(inner => new JsonEventEmitter(inner, yamlFormatter, enumNamingConvention, BuildTypeInspector()), loc => loc.InsteadOf<TypeAssigningEventEmitter>());
        }

        /// <summary>
        /// Allows you to override the new line character to use when serializing to YAML.
        /// </summary>
        /// <param name="newLine">NewLine character(s) to use when serializing to YAML.</param>
        public SerializerBuilder WithNewLine(string newLine)
        {
            this.emitterSettings = this.emitterSettings.WithNewLine(newLine);
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
        /// <param name="objectGraphVisitorFactory">A function that instantiates the type inspector.</param>
        public SerializerBuilder WithPreProcessingPhaseObjectGraphVisitor<TObjectGraphVisitor>(Func<IEnumerable<IYamlTypeConverter>, TObjectGraphVisitor> objectGraphVisitorFactory)
            where TObjectGraphVisitor : IObjectGraphVisitor<Nothing>
        {
            return WithPreProcessingPhaseObjectGraphVisitor(objectGraphVisitorFactory, w => w.OnTop());
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
                throw new ArgumentNullException(nameof(objectGraphVisitor));
            }

            if (where == null)
            {
                throw new ArgumentNullException(nameof(where));
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
        /// <param name="objectGraphVisitorFactory">A function that instantiates the type inspector.</param>
        /// <param name="where">Configures the location where to insert the <see cref="IObjectGraphVisitor{Nothing}" /></param>
        public SerializerBuilder WithPreProcessingPhaseObjectGraphVisitor<TObjectGraphVisitor>(
            Func<IEnumerable<IYamlTypeConverter>, TObjectGraphVisitor> objectGraphVisitorFactory,
            Action<IRegistrationLocationSelectionSyntax<IObjectGraphVisitor<Nothing>>> where
        )
            where TObjectGraphVisitor : IObjectGraphVisitor<Nothing>
        {
            if (objectGraphVisitorFactory == null)
            {
                throw new ArgumentNullException(nameof(objectGraphVisitorFactory));
            }

            if (where == null)
            {
                throw new ArgumentNullException(nameof(where));
            }

            where(preProcessingPhaseObjectGraphVisitorFactories.CreateRegistrationLocationSelector(typeof(TObjectGraphVisitor), typeConverters => objectGraphVisitorFactory(typeConverters)));
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
        /// <param name="objectGraphVisitorFactory">A function that instantiates the type inspector based on a previously registered <see cref="IObjectGraphVisitor{Nothing}" />.</param>
        /// <param name="where">Configures the location where to insert the <see cref="IObjectGraphVisitor{Nothing}" /></param>
        public SerializerBuilder WithPreProcessingPhaseObjectGraphVisitor<TObjectGraphVisitor>(
            WrapperFactory<IObjectGraphVisitor<Nothing>, TObjectGraphVisitor> objectGraphVisitorFactory,
            Action<ITrackingRegistrationLocationSelectionSyntax<IObjectGraphVisitor<Nothing>>> where
        )
            where TObjectGraphVisitor : IObjectGraphVisitor<Nothing>
        {
            if (objectGraphVisitorFactory == null)
            {
                throw new ArgumentNullException(nameof(objectGraphVisitorFactory));
            }

            if (where == null)
            {
                throw new ArgumentNullException(nameof(where));
            }

            where(preProcessingPhaseObjectGraphVisitorFactories.CreateTrackingRegistrationLocationSelector(typeof(TObjectGraphVisitor), (wrapped, _) => objectGraphVisitorFactory(wrapped)));
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
        /// <param name="objectGraphVisitorFactory">A function that instantiates the type inspector based on a previously registered <see cref="IObjectGraphVisitor{Nothing}" />.</param>
        /// <param name="where">Configures the location where to insert the <see cref="IObjectGraphVisitor{Nothing}" /></param>
        public SerializerBuilder WithPreProcessingPhaseObjectGraphVisitor<TObjectGraphVisitor>(
            WrapperFactory<IEnumerable<IYamlTypeConverter>, IObjectGraphVisitor<Nothing>, TObjectGraphVisitor> objectGraphVisitorFactory,
            Action<ITrackingRegistrationLocationSelectionSyntax<IObjectGraphVisitor<Nothing>>> where
        )
            where TObjectGraphVisitor : IObjectGraphVisitor<Nothing>
        {
            if (objectGraphVisitorFactory == null)
            {
                throw new ArgumentNullException(nameof(objectGraphVisitorFactory));
            }

            if (where == null)
            {
                throw new ArgumentNullException(nameof(where));
            }

            where(preProcessingPhaseObjectGraphVisitorFactories.CreateTrackingRegistrationLocationSelector(typeof(TObjectGraphVisitor), (wrapped, typeConverters) => objectGraphVisitorFactory(wrapped, typeConverters)));
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
                throw new ArgumentNullException(nameof(objectGraphVisitorType));
            }

            preProcessingPhaseObjectGraphVisitorFactories.Remove(objectGraphVisitorType);
            return this;
        }

        /// <summary>
        /// Registers an <see cref="ObjectGraphTraversalStrategyFactory"/> to be used by the serializer
        /// while traversing the object graph.
        /// </summary>
        /// <param name="objectGraphTraversalStrategyFactory">A function that instantiates the traversal strategy.</param>
        public SerializerBuilder WithObjectGraphTraversalStrategyFactory(ObjectGraphTraversalStrategyFactory objectGraphTraversalStrategyFactory)
        {
            this.objectGraphTraversalStrategyFactory = objectGraphTraversalStrategyFactory;

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
                throw new ArgumentNullException(nameof(objectGraphVisitorFactory));
            }

            if (where == null)
            {
                throw new ArgumentNullException(nameof(where));
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
                throw new ArgumentNullException(nameof(objectGraphVisitorFactory));
            }

            if (where == null)
            {
                throw new ArgumentNullException(nameof(where));
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
                throw new ArgumentNullException(nameof(objectGraphVisitorType));
            }

            emissionPhaseObjectGraphVisitorFactories.Remove(objectGraphVisitorType);
            return this;
        }

        /// <summary>
        /// Creates sequences with extra indentation
        /// </summary>
        /// <example>
        ///  list:
        ///    - item
        ///    - item
        /// </example>
        /// <returns></returns>
        public SerializerBuilder WithIndentedSequences()
        {
            emitterSettings = emitterSettings.WithIndentedSequences();

            return this;
        }

        /// <summary>
        /// Creates a new <see cref="Serializer" /> according to the current configuration.
        /// </summary>
        public ISerializer Build()
        {
            return Serializer.FromValueSerializer(BuildValueSerializer(), emitterSettings);
        }

        /// <summary>
        /// Creates a new <see cref="IValueDeserializer" /> that implements the current configuration.
        /// This method is available for advanced scenarios. The preferred way to customize the behavior of the
        /// deserializer is to use the <see cref="Build" /> method.
        /// </summary>
        public IValueSerializer BuildValueSerializer()
        {
            var typeConverters = BuildTypeConverters();
            var typeInspector = BuildTypeInspector();
            var traversalStrategy = objectGraphTraversalStrategyFactory(typeInspector, typeResolver, typeConverters, maximumRecursion);
            var eventEmitter = eventEmitterFactories.BuildComponentChain(new WriterEventEmitter());

            return new ValueSerializer(
                traversalStrategy,
                eventEmitter,
                typeConverters,
                preProcessingPhaseObjectGraphVisitorFactories.Clone(),
                emissionPhaseObjectGraphVisitorFactories.Clone()
            );
        }

        /// <summary>
        /// Builds the type inspector used by various classes to get information about types and their members.
        /// </summary>
        /// <returns></returns>
        public ITypeInspector BuildTypeInspector()
        {
            ITypeInspector innerInspector = new ReadablePropertiesTypeInspector(typeResolver, includeNonPublicProperties);

            if (!ignoreFields)
            {
                innerInspector = new CompositeTypeInspector(
                    new ReadableFieldsTypeInspector(typeResolver),
                    innerInspector
                );
            }

            return typeInspectorFactories.BuildComponentChain(innerInspector);
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

            public void SerializeValue(IEmitter emitter, object? value, Type? type)
            {
                var actualType = type ?? (value != null ? value.GetType() : typeof(object));
                var staticType = type ?? typeof(object);
                var graph = new ObjectDescriptor(value, actualType, staticType);

                var preProcessingPhaseObjectGraphVisitors = preProcessingPhaseObjectGraphVisitorFactories.BuildComponentList(typeConverters);

                void NestedObjectSerializer(object? v, Type? t)
                {
                    SerializeValue(emitter, v, t);
                }

                var emittingVisitor = emissionPhaseObjectGraphVisitorFactories.BuildComponentChain(
                    new EmittingObjectGraphVisitor(eventEmitter),
                    inner => new EmissionPhaseObjectGraphVisitorArgs(inner, eventEmitter, preProcessingPhaseObjectGraphVisitors, typeConverters, NestedObjectSerializer)
                );

                foreach (var visitor in preProcessingPhaseObjectGraphVisitors)
                {
                    traversalStrategy.Traverse(graph, visitor, default, NestedObjectSerializer);
                }

                traversalStrategy.Traverse(graph, emittingVisitor, emitter, NestedObjectSerializer);
            }
        }
    }
}
