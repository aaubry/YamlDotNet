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
using System.Runtime.CompilerServices;

#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using YamlDotNet.Core;
using YamlDotNet.Serialization.BufferedDeserialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;
using YamlDotNet.Serialization.NodeTypeResolvers;
using YamlDotNet.Serialization.ObjectFactories;
using YamlDotNet.Serialization.Schemas;
using YamlDotNet.Serialization.TypeInspectors;
using YamlDotNet.Serialization.TypeResolvers;
using YamlDotNet.Serialization.Utilities;
using YamlDotNet.Serialization.ValueDeserializers;

namespace YamlDotNet.Serialization
{
    /// <summary>
    /// Creates and configures instances of <see cref="Deserializer" />.
    /// This class is used to customize the behavior of <see cref="Deserializer" />. Use the relevant methods
    /// to apply customizations, then call <see cref="Build" /> to create an instance of the deserializer
    /// with the desired customizations.
    /// </summary>
    public sealed class StaticDeserializerBuilder : StaticBuilderSkeleton<StaticDeserializerBuilder>
    {
        private readonly StaticContext context;
        private readonly StaticObjectFactory factory;
        private readonly LazyComponentRegistrationList<Nothing, INodeDeserializer> nodeDeserializerFactories;
        private readonly LazyComponentRegistrationList<Nothing, INodeTypeResolver> nodeTypeResolverFactories;
        private readonly Dictionary<TagName, Type> tagMappings;
        private readonly ITypeConverter typeConverter;
        private readonly Dictionary<Type, Type> typeMappings;
        private bool ignoreUnmatched;
        private bool duplicateKeyChecking;
        private bool attemptUnknownTypeDeserialization;
        private bool enforceNullability;
        private bool caseInsensitivePropertyMatching;

        /// <summary>
        /// Initializes a new <see cref="DeserializerBuilder" /> using the default component registrations.
        /// </summary>
        public StaticDeserializerBuilder(StaticContext context)
            : base(context.GetTypeResolver())
        {
            this.context = context;
            factory = context.GetFactory();
            typeMappings = new ();

            tagMappings = new Dictionary<TagName, Type>
            {
                { FailsafeSchema.Tags.Map, typeof(Dictionary<object, object>) },
                { FailsafeSchema.Tags.Str, typeof(string) },
                { JsonSchema.Tags.Bool, typeof(bool) },
                { JsonSchema.Tags.Float, typeof(double) },
                { JsonSchema.Tags.Int, typeof(int) },
                { DefaultSchema.Tags.Timestamp, typeof(DateTime) }
            };

            typeInspectorFactories.Add(typeof(CachedTypeInspector), inner => new CachedTypeInspector(inner));
            typeInspectorFactories.Add(typeof(NamingConventionTypeInspector), inner => namingConvention is NullNamingConvention ? inner : new NamingConventionTypeInspector(inner, namingConvention));
            typeInspectorFactories.Add(typeof(YamlAttributesTypeInspector), inner => new YamlAttributesTypeInspector(inner));

            typeConverter = new NullTypeConverter();

            nodeDeserializerFactories = new LazyComponentRegistrationList<Nothing, INodeDeserializer>
            {
                { typeof(YamlConvertibleNodeDeserializer), _ => new YamlConvertibleNodeDeserializer(factory) },
                { typeof(YamlSerializableNodeDeserializer), _ => new YamlSerializableNodeDeserializer(factory) },
                { typeof(TypeConverterNodeDeserializer), _ => new TypeConverterNodeDeserializer(BuildTypeConverters()) },
                { typeof(NullNodeDeserializer), _ => new NullNodeDeserializer() },
                { typeof(ScalarNodeDeserializer), _ => new ScalarNodeDeserializer(attemptUnknownTypeDeserialization, typeConverter, BuildTypeInspector(), yamlFormatter, enumNamingConvention) },
                { typeof(StaticArrayNodeDeserializer), _ => new StaticArrayNodeDeserializer(factory) },
                { typeof(StaticDictionaryNodeDeserializer), _ => new StaticDictionaryNodeDeserializer(factory, duplicateKeyChecking) },
                { typeof(StaticCollectionNodeDeserializer), _ => new StaticCollectionNodeDeserializer(factory) },
                { typeof(ObjectNodeDeserializer), _ => new ObjectNodeDeserializer(factory,
                    BuildTypeInspector(),
                    ignoreUnmatched,
                    duplicateKeyChecking,
                    typeConverter,
                    enumNamingConvention,
                    enforceNullability,
                    caseInsensitivePropertyMatching,
                    false, // the static builder doesn't support required attributes
                    BuildTypeConverters())
                },
            };

            nodeTypeResolverFactories = new LazyComponentRegistrationList<Nothing, INodeTypeResolver>
            {
                { typeof(MappingNodeTypeResolver), _ => new MappingNodeTypeResolver(typeMappings) },
                { typeof(YamlConvertibleTypeResolver), _ => new YamlConvertibleTypeResolver() },
                { typeof(YamlSerializableTypeResolver), _ => new YamlSerializableTypeResolver() },
                { typeof(TagNodeTypeResolver), _ => new TagNodeTypeResolver(tagMappings) },
                { typeof(PreventUnknownTagsNodeTypeResolver), _ => new PreventUnknownTagsNodeTypeResolver() },
                { typeof(DefaultContainersNodeTypeResolver), _ => new DefaultContainersNodeTypeResolver() }
            };
        }

        protected override StaticDeserializerBuilder Self { get { return this; } }

        /// <summary>
        /// Builds the type inspector used by various classes to get information about types and their members.
        /// </summary>
        /// <returns></returns>
        public ITypeInspector BuildTypeInspector()
        {
            ITypeInspector innerInspector = context.GetTypeInspector();

            return typeInspectorFactories.BuildComponentChain(innerInspector);
        }

        /// <summary>
        /// When deserializing it will attempt to convert unquoted strings to their correct datatype. If conversion is not sucessful, it will leave it as a string.
        /// This option is only applicable when not specifying a type or specifying the object type during deserialization.
        /// </summary>
        public StaticDeserializerBuilder WithAttemptingUnquotedStringTypeDeserialization()
        {
            attemptUnknownTypeDeserialization = true;
            return this;
        }

        /// <summary>
        /// Registers an additional <see cref="INodeDeserializer" /> to be used by the deserializer.
        /// </summary>
        public StaticDeserializerBuilder WithNodeDeserializer(INodeDeserializer nodeDeserializer)
        {
            return WithNodeDeserializer(nodeDeserializer, w => w.OnTop());
        }

        /// <summary>
        /// Registers an additional <see cref="INodeDeserializer" /> to be used by the deserializer.
        /// </summary>
        /// <param name="nodeDeserializer"></param>
        /// <param name="where">Configures the location where to insert the <see cref="INodeDeserializer" /></param>
        public StaticDeserializerBuilder WithNodeDeserializer(
            INodeDeserializer nodeDeserializer,
            Action<IRegistrationLocationSelectionSyntax<INodeDeserializer>> where
        )
        {
            if (nodeDeserializer == null)
            {
                throw new ArgumentNullException(nameof(nodeDeserializer));
            }

            if (where == null)
            {
                throw new ArgumentNullException(nameof(where));
            }

            where(nodeDeserializerFactories.CreateRegistrationLocationSelector(nodeDeserializer.GetType(), _ => nodeDeserializer));
            return this;
        }

        /// <summary>
        /// Registers an additional <see cref="INodeDeserializer" /> to be used by the deserializer.
        /// </summary>
        /// <param name="nodeDeserializerFactory">A factory that creates the <see cref="INodeDeserializer" /> based on a previously registered <see cref="INodeDeserializer" />.</param>
        /// <param name="where">Configures the location where to insert the <see cref="INodeDeserializer" /></param>
        public StaticDeserializerBuilder WithNodeDeserializer<TNodeDeserializer>(
            WrapperFactory<INodeDeserializer, TNodeDeserializer> nodeDeserializerFactory,
            Action<ITrackingRegistrationLocationSelectionSyntax<INodeDeserializer>> where
        )
            where TNodeDeserializer : INodeDeserializer
        {
            if (nodeDeserializerFactory == null)
            {
                throw new ArgumentNullException(nameof(nodeDeserializerFactory));
            }

            if (where == null)
            {
                throw new ArgumentNullException(nameof(where));
            }

            where(nodeDeserializerFactories.CreateTrackingRegistrationLocationSelector(typeof(TNodeDeserializer), (wrapped, _) => nodeDeserializerFactory(wrapped)));
            return this;
        }

        /// <summary>
        /// Ignore case when matching property names.
        /// </summary>
        /// <returns></returns>
        public StaticDeserializerBuilder WithCaseInsensitivePropertyMatching()
        {
            caseInsensitivePropertyMatching = true;
            return this;
        }

        /// <summary>
        /// Enforce whether null values can be set on non-nullable properties and fields.
        /// </summary>
        /// <returns>This static deserializer builder.</returns>
        public StaticDeserializerBuilder WithEnforceNullability()
        {
            enforceNullability = true;
            return this;
        }

        /// <summary>
        /// Unregisters an existing <see cref="INodeDeserializer" /> of type <typeparam name="TNodeDeserializer" />.
        /// </summary>
        public StaticDeserializerBuilder WithoutNodeDeserializer<TNodeDeserializer>()
            where TNodeDeserializer : INodeDeserializer
        {
            return WithoutNodeDeserializer(typeof(TNodeDeserializer));
        }

        /// <summary>
        /// Unregisters an existing <see cref="INodeDeserializer" /> of type <param name="nodeDeserializerType" />.
        /// </summary>
        public StaticDeserializerBuilder WithoutNodeDeserializer(Type nodeDeserializerType)
        {
            if (nodeDeserializerType == null)
            {
                throw new ArgumentNullException(nameof(nodeDeserializerType));
            }

            nodeDeserializerFactories.Remove(nodeDeserializerType);
            return this;
        }

        /// <summary>
        /// Registers a <see cref="TypeDiscriminatingNodeDeserializer" /> to be used by the deserializer. This internally registers
        /// all existing <see cref="INodeDeserializer" /> as inner deserializers available to the <see cref="TypeDiscriminatingNodeDeserializer" />.
        /// Usually you will want to call this after any other changes to the <see cref="INodeDeserializer" />s used by the deserializer.
        /// </summary>
        /// <param name="configureTypeDiscriminatingNodeDeserializerOptions">An action that can configure the <see cref="TypeDiscriminatingNodeDeserializer" />.</param>
        /// <param name="maxDepth">Configures the max depth of yaml nodes that will be buffered. A value of -1 (the default) means yaml nodes of any depth will be buffered.</param>
        /// <param name="maxLength">Configures the max number of yaml nodes that will be buffered. A value of -1 (the default) means there is no limit on the number of yaml nodes buffered.</param>
        public StaticDeserializerBuilder WithTypeDiscriminatingNodeDeserializer(
            Action<ITypeDiscriminatingNodeDeserializerOptions> configureTypeDiscriminatingNodeDeserializerOptions, int maxDepth = -1, int maxLength = -1)
        {
            var options = new TypeDiscriminatingNodeDeserializerOptions();
            configureTypeDiscriminatingNodeDeserializerOptions(options);
            // We use all current NodeDeserializers as the inner deserializers for the TypeDiscriminatingNodeDeserializer,
            // so that it can successfully deserialize anything our root deserializer can.
            var typeDiscriminatingNodeDeserializer = new TypeDiscriminatingNodeDeserializer(nodeDeserializerFactories.BuildComponentList(), options.discriminators, maxDepth, maxLength);

            // We register this before the DictionaryNodeDeserializer, as otherwise it will take precedence
            // and cases where BaseType = object will not reach the TypeDiscriminatingNodeDeserializer
            return WithNodeDeserializer(typeDiscriminatingNodeDeserializer, s => s.Before<DictionaryNodeDeserializer>());
        }

        /// <summary>
        /// Registers an additional <see cref="INodeTypeResolver" /> to be used by the deserializer.
        /// </summary>
        public StaticDeserializerBuilder WithNodeTypeResolver(INodeTypeResolver nodeTypeResolver)
        {
            return WithNodeTypeResolver(nodeTypeResolver, w => w.OnTop());
        }

        /// <summary>
        /// Registers an additional <see cref="INodeTypeResolver" /> to be used by the deserializer.
        /// </summary>
        /// <param name="nodeTypeResolver"></param>
        /// <param name="where">Configures the location where to insert the <see cref="INodeTypeResolver" /></param>
        public StaticDeserializerBuilder WithNodeTypeResolver(
            INodeTypeResolver nodeTypeResolver,
            Action<IRegistrationLocationSelectionSyntax<INodeTypeResolver>> where
        )
        {
            if (nodeTypeResolver == null)
            {
                throw new ArgumentNullException(nameof(nodeTypeResolver));
            }

            if (where == null)
            {
                throw new ArgumentNullException(nameof(where));
            }

            where(nodeTypeResolverFactories.CreateRegistrationLocationSelector(nodeTypeResolver.GetType(), _ => nodeTypeResolver));
            return this;
        }

        /// <summary>
        /// Registers an additional <see cref="INodeTypeResolver" /> to be used by the deserializer.
        /// </summary>
        /// <param name="nodeTypeResolverFactory">A factory that creates the <see cref="INodeTypeResolver" /> based on a previously registered <see cref="INodeTypeResolver" />.</param>
        /// <param name="where">Configures the location where to insert the <see cref="INodeTypeResolver" /></param>
        public StaticDeserializerBuilder WithNodeTypeResolver<TNodeTypeResolver>(
            WrapperFactory<INodeTypeResolver, TNodeTypeResolver> nodeTypeResolverFactory,
            Action<ITrackingRegistrationLocationSelectionSyntax<INodeTypeResolver>> where
        )
            where TNodeTypeResolver : INodeTypeResolver
        {
            if (nodeTypeResolverFactory == null)
            {
                throw new ArgumentNullException(nameof(nodeTypeResolverFactory));
            }

            if (where == null)
            {
                throw new ArgumentNullException(nameof(where));
            }

            where(nodeTypeResolverFactories.CreateTrackingRegistrationLocationSelector(typeof(TNodeTypeResolver), (wrapped, _) => nodeTypeResolverFactory(wrapped)));
            return this;
        }

        /// <summary>
        /// Unregisters an existing <see cref="INodeTypeResolver" /> of type <typeparam name="TNodeTypeResolver" />.
        /// </summary>
        public StaticDeserializerBuilder WithoutNodeTypeResolver<TNodeTypeResolver>()
            where TNodeTypeResolver : INodeTypeResolver
        {
            return WithoutNodeTypeResolver(typeof(TNodeTypeResolver));
        }

        /// <summary>
        /// Unregisters an existing <see cref="INodeTypeResolver" /> of type <param name="nodeTypeResolverType" />.
        /// </summary>
        public StaticDeserializerBuilder WithoutNodeTypeResolver(Type nodeTypeResolverType)
        {
            if (nodeTypeResolverType == null)
            {
                throw new ArgumentNullException(nameof(nodeTypeResolverType));
            }

            nodeTypeResolverFactories.Remove(nodeTypeResolverType);
            return this;
        }

        /// <summary>
        /// Registers a tag mapping.
        /// </summary>
        public override StaticDeserializerBuilder WithTagMapping(TagName tag, Type type)
        {
            if (tag.IsEmpty)
            {
                throw new ArgumentException("Non-specific tags cannot be maped");
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (tagMappings.TryGetValue(tag, out var alreadyRegisteredType))
            {
                throw new ArgumentException($"Type already has a registered type '{alreadyRegisteredType.FullName}' for tag '{tag}'", nameof(tag));
            }

            tagMappings.Add(tag, type);
            return this;
        }

        /// <summary>
        /// Registers a type mapping using the default object factory.
        /// </summary>
        public StaticDeserializerBuilder WithTypeMapping<TInterface, TConcrete>()
            where TConcrete : TInterface
        {
            var interfaceType = typeof(TInterface);
            var concreteType = typeof(TConcrete);

            if (!interfaceType.IsAssignableFrom(concreteType))
            {
                throw new InvalidOperationException($"The type '{concreteType.Name}' does not implement interface '{interfaceType.Name}'.");
            }

            typeMappings[interfaceType] = concreteType;

            return this;
        }

        /// <summary>
        /// Unregisters an existing tag mapping.
        /// </summary>
        public StaticDeserializerBuilder WithoutTagMapping(TagName tag)
        {
            if (tag.IsEmpty)
            {
                throw new ArgumentException("Non-specific tags cannot be maped");
            }

            if (!tagMappings.Remove(tag))
            {
                throw new KeyNotFoundException($"Tag '{tag}' is not registered");
            }
            return this;
        }

        /// <summary>
        /// Instructs the deserializer to ignore unmatched properties instead of throwing an exception.
        /// </summary>
        public StaticDeserializerBuilder IgnoreUnmatchedProperties()
        {
            ignoreUnmatched = true;
            return this;
        }

        /// <summary>
        /// Instructs the deserializer to check for duplicate keys and throw an exception if duplicate keys are found.
        /// </summary>
        /// <returns></returns>
        public StaticDeserializerBuilder WithDuplicateKeyChecking()
        {
            duplicateKeyChecking = true;
            return this;
        }

        /// <summary>
        /// Creates a new <see cref="Deserializer" /> according to the current configuration.
        /// </summary>
        public IDeserializer Build()
        {
            return Deserializer.FromValueDeserializer(BuildValueDeserializer());
        }

        /// <summary>
        /// Creates a new <see cref="IValueDeserializer" /> that implements the current configuration.
        /// This method is available for advanced scenarios. The preferred way to customize the behavior of the
        /// deserializer is to use the <see cref="Build" /> method.
        /// </summary>
        public IValueDeserializer BuildValueDeserializer()
        {
            return new AliasValueDeserializer(
                new NodeValueDeserializer(
                    nodeDeserializerFactories.BuildComponentList(),
                    nodeTypeResolverFactories.BuildComponentList(),
                    typeConverter,
                    enumNamingConvention,
                    BuildTypeInspector()
                )
            );
        }
    }
}
