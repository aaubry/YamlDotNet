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
using YamlDotNet.Serialization.NodeDeserializers;
using YamlDotNet.Serialization.NodeTypeResolvers;
using YamlDotNet.Serialization.ObjectFactories;
using YamlDotNet.Serialization.TypeInspectors;
using YamlDotNet.Serialization.TypeResolvers;
using YamlDotNet.Serialization.ValueDeserializers;

namespace YamlDotNet.Serialization
{
    /// <summary>
    /// Creates and configures instances of <see cref="Deserializer" />.
    /// This class is used to customize the behavior of <see cref="Deserializer" />. Use the relevant methods
    /// to apply customizations, then call <see cref="Build" /> to create an instance of the deserializer
    /// with the desired customizations.
    /// </summary>
    public sealed class DeserializerBuilder : BuilderSkeleton<DeserializerBuilder>
    {
        private IObjectFactory objectFactory = new DefaultObjectFactory();
        private readonly LazyComponentRegistrationList<Nothing, INodeDeserializer> nodeDeserializerFactories;
        private readonly LazyComponentRegistrationList<Nothing, INodeTypeResolver> nodeTypeResolverFactories;
        private readonly Dictionary<string, Type> tagMappings;
        private bool ignoreUnmatched;

        /// <summary>
        /// Initializes a new <see cref="DeserializerBuilder" /> using the default component registrations.
        /// </summary>
        public DeserializerBuilder()
        {
            tagMappings = new Dictionary<string, Type>
            {
                { "tag:yaml.org,2002:map", typeof(Dictionary<object, object>) },
                { "tag:yaml.org,2002:bool", typeof(bool) },
                { "tag:yaml.org,2002:float", typeof(double) },
                { "tag:yaml.org,2002:int", typeof(int) },
                { "tag:yaml.org,2002:str", typeof(string) },
                { "tag:yaml.org,2002:timestamp", typeof(DateTime) }
            };

            typeInspectorFactories.Add(typeof(CachedTypeInspector), inner => new CachedTypeInspector(inner));
            typeInspectorFactories.Add(typeof(NamingConventionTypeInspector), inner => namingConvention != null ? new NamingConventionTypeInspector(inner, namingConvention) : inner);
            typeInspectorFactories.Add(typeof(YamlAttributesTypeInspector), inner => new YamlAttributesTypeInspector(inner));
            typeInspectorFactories.Add(typeof(YamlAttributeOverridesInspector), inner => overrides != null ? new YamlAttributeOverridesInspector(inner, overrides.Clone()) : inner);
            typeInspectorFactories.Add(typeof(ReadableAndWritablePropertiesTypeInspector), inner => new ReadableAndWritablePropertiesTypeInspector(inner));

            nodeDeserializerFactories = new LazyComponentRegistrationList<Nothing, INodeDeserializer>
            {
                { typeof(YamlConvertibleNodeDeserializer), _ => new YamlConvertibleNodeDeserializer(objectFactory) },
                { typeof(YamlSerializableNodeDeserializer), _ => new YamlSerializableNodeDeserializer(objectFactory) },
                { typeof(TypeConverterNodeDeserializer), _ => new TypeConverterNodeDeserializer(BuildTypeConverters()) },
                { typeof(NullNodeDeserializer), _ => new NullNodeDeserializer() },
                { typeof(ScalarNodeDeserializer), _ => new ScalarNodeDeserializer() },
                { typeof(ArrayNodeDeserializer), _ => new ArrayNodeDeserializer() },
                { typeof(DictionaryNodeDeserializer), _ => new DictionaryNodeDeserializer(objectFactory) },
                { typeof(CollectionNodeDeserializer), _ => new CollectionNodeDeserializer(objectFactory) },
                { typeof(EnumerableNodeDeserializer), _ => new EnumerableNodeDeserializer() },
                { typeof(ObjectNodeDeserializer), _ => new ObjectNodeDeserializer(objectFactory, BuildTypeInspector(), ignoreUnmatched) }
            };

            nodeTypeResolverFactories = new LazyComponentRegistrationList<Nothing, INodeTypeResolver>
            {
                { typeof(YamlConvertibleTypeResolver), _ => new YamlConvertibleTypeResolver() },
                { typeof(YamlSerializableTypeResolver), _ => new YamlSerializableTypeResolver() },
                { typeof(TagNodeTypeResolver), _ => new TagNodeTypeResolver(tagMappings) },
                { typeof(TypeNameInTagNodeTypeResolver), _ => new TypeNameInTagNodeTypeResolver() },
                { typeof(DefaultContainersNodeTypeResolver), _ => new DefaultContainersNodeTypeResolver() }
            };

            WithTypeResolver(new StaticTypeResolver());
        }

        protected override DeserializerBuilder Self { get { return this; } }

        /// <summary>
        /// Sets the <see cref="IObjectFactory" /> that will be used by the deserializer.
        /// </summary>
        public DeserializerBuilder WithObjectFactory(IObjectFactory objectFactory)
        {
            if (objectFactory == null)
            {
                throw new ArgumentNullException("objectFactory");
            }

            this.objectFactory = objectFactory;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="IObjectFactory" /> that will be used by the deserializer.
        /// </summary>
        public DeserializerBuilder WithObjectFactory(Func<Type, object> objectFactory)
        {
            if (objectFactory == null)
            {
                throw new ArgumentNullException("objectFactory");
            }

            return WithObjectFactory(new LambdaObjectFactory(objectFactory));
        }

        /// <summary>
        /// Registers an additional <see cref="INodeDeserializer" /> to be used by the deserializer.
        /// </summary>
        public DeserializerBuilder WithNodeDeserializer(INodeDeserializer nodeDeserializer)
        {
            return WithNodeDeserializer(nodeDeserializer, w => w.OnTop());
        }

        /// <summary>
        /// Registers an additional <see cref="INodeDeserializer" /> to be used by the deserializer.
        /// </summary>
        /// <param name="nodeDeserializer"></param>
        /// <param name="where">Configures the location where to insert the <see cref="INodeDeserializer" /></param>
        public DeserializerBuilder WithNodeDeserializer(
            INodeDeserializer nodeDeserializer,
            Action<IRegistrationLocationSelectionSyntax<INodeDeserializer>> where
        )
        {
            if (nodeDeserializer == null)
            {
                throw new ArgumentNullException("nodeDeserializer");
            }

            if (where == null)
            {
                throw new ArgumentNullException("where");
            }

            where(nodeDeserializerFactories.CreateRegistrationLocationSelector(nodeDeserializer.GetType(), _ => nodeDeserializer));
            return this;
        }

        /// <summary>
        /// Registers an additional <see cref="INodeDeserializer" /> to be used by the deserializer.
        /// </summary>
        /// <param name="nodeDeserializerFactory">A factory that creates the <see cref="INodeDeserializer" /> based on a previously registered <see cref="INodeDeserializer" />.</param>
        /// <param name="where">Configures the location where to insert the <see cref="INodeDeserializer" /></param>
        public DeserializerBuilder WithNodeDeserializer<TNodeDeserializer>(
            WrapperFactory<INodeDeserializer, TNodeDeserializer> nodeDeserializerFactory,
            Action<ITrackingRegistrationLocationSelectionSyntax<INodeDeserializer>> where
        )
            where TNodeDeserializer : INodeDeserializer
        {
            if (nodeDeserializerFactory == null)
            {
                throw new ArgumentNullException("nodeDeserializerFactory");
            }

            if (where == null)
            {
                throw new ArgumentNullException("where");
            }

            where(nodeDeserializerFactories.CreateTrackingRegistrationLocationSelector(typeof(TNodeDeserializer), (wrapped, _) => nodeDeserializerFactory(wrapped)));
            return this;
        }

        /// <summary>
        /// Unregisters an existing <see cref="INodeDeserializer" /> of type <typeparam name="TNodeDeserializer" />.
        /// </summary>
        public DeserializerBuilder WithoutNodeDeserializer<TNodeDeserializer>()
            where TNodeDeserializer : INodeDeserializer
        {
            return WithoutNodeDeserializer(typeof(TNodeDeserializer));
        }

        /// <summary>
        /// Unregisters an existing <see cref="INodeDeserializer" /> of type <param name="nodeDeserializerType" />.
        /// </summary>
        public DeserializerBuilder WithoutNodeDeserializer(Type nodeDeserializerType)
        {
            if (nodeDeserializerType == null)
            {
                throw new ArgumentNullException("nodeDeserializerType");
            }

            nodeDeserializerFactories.Remove(nodeDeserializerType);
            return this;
        }

        /// <summary>
        /// Registers an additional <see cref="INodeTypeResolver" /> to be used by the deserializer.
        /// </summary>
        public DeserializerBuilder WithNodeTypeResolver(INodeTypeResolver nodeTypeResolver)
        {
            return WithNodeTypeResolver(nodeTypeResolver, w => w.OnTop());
        }

        /// <summary>
        /// Registers an additional <see cref="INodeTypeResolver" /> to be used by the deserializer.
        /// </summary>
        /// <param name="nodeTypeResolver"></param>
        /// <param name="where">Configures the location where to insert the <see cref="INodeTypeResolver" /></param>
        public DeserializerBuilder WithNodeTypeResolver(
            INodeTypeResolver nodeTypeResolver,
            Action<IRegistrationLocationSelectionSyntax<INodeTypeResolver>> where
        )
        {
            if (nodeTypeResolver == null)
            {
                throw new ArgumentNullException("nodeTypeResolver");
            }

            if (where == null)
            {
                throw new ArgumentNullException("where");
            }

            where(nodeTypeResolverFactories.CreateRegistrationLocationSelector(nodeTypeResolver.GetType(), _ => nodeTypeResolver));
            return this;
        }

        /// <summary>
        /// Registers an additional <see cref="INodeTypeResolver" /> to be used by the deserializer.
        /// </summary>
        /// <param name="nodeTypeResolverFactory">A factory that creates the <see cref="INodeTypeResolver" /> based on a previously registered <see cref="INodeTypeResolver" />.</param>
        /// <param name="where">Configures the location where to insert the <see cref="INodeTypeResolver" /></param>
        public DeserializerBuilder WithNodeTypeResolver<TNodeTypeResolver>(
            WrapperFactory<INodeTypeResolver, TNodeTypeResolver> nodeTypeResolverFactory,
            Action<ITrackingRegistrationLocationSelectionSyntax<INodeTypeResolver>> where
        )
            where TNodeTypeResolver : INodeTypeResolver
        {
            if (nodeTypeResolverFactory == null)
            {
                throw new ArgumentNullException("nodeTypeResolverFactory");
            }

            if (where == null)
            {
                throw new ArgumentNullException("where");
            }

            where(nodeTypeResolverFactories.CreateTrackingRegistrationLocationSelector(typeof(TNodeTypeResolver), (wrapped, _) => nodeTypeResolverFactory(wrapped)));
            return this;
        }

        /// <summary>
        /// Unregisters an existing <see cref="INodeTypeResolver" /> of type <typeparam name="TNodeTypeResolver" />.
        /// </summary>
        public DeserializerBuilder WithoutNodeTypeResolver<TNodeTypeResolver>()
            where TNodeTypeResolver : INodeTypeResolver
        {
            return WithoutNodeTypeResolver(typeof(TNodeTypeResolver));
        }

        /// <summary>
        /// Unregisters an existing <see cref="INodeTypeResolver" /> of type <param name="nodeTypeResolverType" />.
        /// </summary>
        public DeserializerBuilder WithoutNodeTypeResolver(Type nodeTypeResolverType)
        {
            if (nodeTypeResolverType == null)
            {
                throw new ArgumentNullException("nodeTypeResolverType");
            }

            nodeTypeResolverFactories.Remove(nodeTypeResolverType);
            return this;
        }

        /// <summary>
        /// Registers a tag mapping.
        /// </summary>
        public DeserializerBuilder WithTagMapping(string tag, Type type)
        {
            if (tag == null)
            {
                throw new ArgumentNullException("tag");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            Type alreadyRegisteredType;
            if (tagMappings.TryGetValue(tag, out alreadyRegisteredType))
            {
                throw new ArgumentException(string.Format("Type already has a registered type '{0}' for tag '{1}'", alreadyRegisteredType.FullName, tag), "tag");
            }

            tagMappings.Add(tag, type);
            return this;
        }

        /// <summary>
        /// Unregisters an existing tag mapping.
        /// </summary>
        public DeserializerBuilder WithoutTagMapping(string tag)
        {
            if (tag == null)
            {
                throw new ArgumentNullException("tag");
            }

            if (!tagMappings.Remove(tag))
            {
                throw new KeyNotFoundException(string.Format("Tag '{0}' is not registered", tag));
            }
            return this;
        }

        /// <summary>
        /// Instructs the deserializer to ignore unmatched properties instead of throwing an exception.
        /// </summary>
        public DeserializerBuilder IgnoreUnmatchedProperties()
        {
            ignoreUnmatched = true;
            return this;
        }

        /// <summary>
        /// Creates a new <see cref="Deserializer" /> according to the current configuration.
        /// </summary>
        public Deserializer Build()
        {
            return Deserializer.FromValueDeserializer(BuildValueDeserializer());
        }

        /// <summary>
        /// Creates a new <see cref="IValueDeserializer" /> that implements the current configuration.
        /// This method is available for advanced scenarios. The preferred way to customize the bahavior of the
        /// deserializer is to use the <see cref="Build" /> method.
        /// </summary>
        public IValueDeserializer BuildValueDeserializer()
        {
            return new AliasValueDeserializer(
                new NodeValueDeserializer(
                    nodeDeserializerFactories.BuildComponentList(),
                    nodeTypeResolverFactories.BuildComponentList()
                )
            );
        }
    }
}