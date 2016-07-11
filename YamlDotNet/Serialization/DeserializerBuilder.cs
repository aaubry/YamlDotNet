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
using System.Linq;
using System.Linq.Expressions;
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
    /// Creates and configures instances of <see cref="Deserializer" />.
    /// This class is used to customize the behavior of <see cref="Deserializer" />. Use the relevant methods
    /// to apply customizations, then call <see cref="Build" /> to create an instance of the deserializer
    /// with the desired customizations.
    /// </summary>
    public sealed class DeserializerBuilder
    {
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

            typeInspectorFactories = new LazyComponentRegistrationList<ITypeInspector, ITypeInspector>
            {
                { typeof(CachedTypeInspector), inner => new CachedTypeInspector(inner) },
                { typeof(YamlAttributesTypeInspector), inner => new YamlAttributesTypeInspector(inner) },
                { typeof(YamlAttributeOverridesInspector), inner => overrides != null ? new YamlAttributeOverridesInspector(inner, overrides.Clone()) : inner },
                { typeof(NamingConventionTypeInspector), inner => namingConvention != null ? new NamingConventionTypeInspector(inner, namingConvention) : inner },
                { typeof(ReadableAndWritablePropertiesTypeInspector), inner => new ReadableAndWritablePropertiesTypeInspector(inner) }
            };

            typeConverters = new LazyComponentRegistrationList<object, IYamlTypeConverter>();
            foreach (var converter in YamlTypeConverters.GetBuiltInConverters(false))
            {
                typeConverters.Add(converter.GetType(), _ => converter);
            }

            nodeDeserializerFactories = new LazyComponentRegistrationList<object, INodeDeserializer>
            {
                { typeof(YamlConvertibleNodeDeserializer), _ => new YamlConvertibleNodeDeserializer(objectFactory) },
                { typeof(YamlSerializableNodeDeserializer), _ => new YamlSerializableNodeDeserializer(objectFactory) },
                { typeof(TypeConverterNodeDeserializer), _ => new TypeConverterNodeDeserializer(typeConverters.Select(r => r.Factory(null)).ToList()) },
                { typeof(NullNodeDeserializer), _ => new NullNodeDeserializer() },
                { typeof(ScalarNodeDeserializer), _ => new ScalarNodeDeserializer() },
                { typeof(ArrayNodeDeserializer), _ => new ArrayNodeDeserializer() },
                { typeof(DictionaryNodeDeserializer), _ => new DictionaryNodeDeserializer(objectFactory) },
                { typeof(CollectionNodeDeserializer), _ => new CollectionNodeDeserializer(objectFactory) },
                { typeof(EnumerableNodeDeserializer), _ => new EnumerableNodeDeserializer() },
                { typeof(ObjectNodeDeserializer), _ => new ObjectNodeDeserializer(objectFactory, BuildTypeInspector(), ignoreUnmatched) }
            };

            nodeTypeResolverFactories = new LazyComponentRegistrationList<object, INodeTypeResolver>
            {
                { typeof(YamlConvertibleTypeResolver), _ => new YamlConvertibleTypeResolver() },
                { typeof(YamlSerializableTypeResolver), _ => new YamlSerializableTypeResolver() },
                { typeof(TagNodeTypeResolver), _ => new TagNodeTypeResolver(tagMappings) },
                { typeof(TypeNameInTagNodeTypeResolver), _ => new TypeNameInTagNodeTypeResolver() },
                { typeof(DefaultContainersNodeTypeResolver), _ => new DefaultContainersNodeTypeResolver() }
            };

            overrides = new YamlAttributeOverrides();
        }

        private ITypeInspector BuildTypeInspector()
        {
            ITypeInspector outerTypeInspector = new ReadablePropertiesTypeInspector(new StaticTypeResolver());
            for (int i = typeInspectorFactories.Count - 1; i >= 0; --i)
            {
                outerTypeInspector = typeInspectorFactories[i].Factory(outerTypeInspector);
            }
            return outerTypeInspector;
        }

        private INamingConvention namingConvention;
        private IObjectFactory objectFactory = new DefaultObjectFactory();
        private readonly LazyComponentRegistrationList<ITypeInspector, ITypeInspector> typeInspectorFactories;
        private readonly LazyComponentRegistrationList<object, INodeDeserializer> nodeDeserializerFactories;
        private readonly LazyComponentRegistrationList<object, INodeTypeResolver> nodeTypeResolverFactories;
        private readonly LazyComponentRegistrationList<object, IYamlTypeConverter> typeConverters;
        private readonly Dictionary<string, Type> tagMappings;
        private readonly YamlAttributeOverrides overrides;
        private bool ignoreUnmatched;

        private sealed class LazyComponentRegistration<TArgument, TComponent>
        {
            public readonly Type ComponentType;
            public readonly Func<TArgument, TComponent> Factory;

            public LazyComponentRegistration(Type componentType, Func<TArgument, TComponent> factory)
            {
                ComponentType = componentType;
                Factory = factory;
            }
        }

        private sealed class LazyComponentRegistrationList<TArgument, TComponent> : List<LazyComponentRegistration<TArgument, TComponent>>
        {
            public void Add(Type componentType, Func<TArgument, TComponent> factory)
            {
                Add(new LazyComponentRegistration<TArgument, TComponent>(componentType, factory));
            }

            public IRegistrationLocationSelectionSyntax<TComponent> CreateRegistrationLocationSelector(
                Type componentType,
                Func<TArgument, TComponent> factory
            )
            {
                return new RegistrationLocationSelector(
                    this,
                    new LazyComponentRegistration<TArgument, TComponent>(componentType, factory)
                );
            }

            private class RegistrationLocationSelector : IRegistrationLocationSelectionSyntax<TComponent>
            {
                private readonly LazyComponentRegistrationList<TArgument, TComponent> registrations;
                private readonly LazyComponentRegistration<TArgument, TComponent> newRegistration;

                public RegistrationLocationSelector(LazyComponentRegistrationList<TArgument, TComponent> registrations, LazyComponentRegistration<TArgument, TComponent> newRegistration)
                {
                    this.registrations = registrations;
                    this.newRegistration = newRegistration;
                }

                private int IndexOfRegistration(Type registrationType)
                {
                    for (int i = 0; i < registrations.Count; ++i)
                    {
                        if (registrationType == registrations[i].ComponentType)
                        {
                            return i;
                        }
                    }
                    return -1;
                }

                private void EnsureNoDuplicateRegistrationType()
                {
                    if (IndexOfRegistration(newRegistration.ComponentType) != -1)
                    {
                        throw new InvalidOperationException(string.Format("A component of type '{0}' has already been registered.", newRegistration.ComponentType.FullName));
                    }
                }

                private int EnsureRegistrationExists<TRegistrationType>()
                {
                    var registrationIndex = IndexOfRegistration(typeof(TRegistrationType));
                    if (registrationIndex == -1)
                    {
                        throw new InvalidOperationException(string.Format("A component of type '{0}' has not been registered.", typeof(TRegistrationType).FullName));
                    }
                    return registrationIndex;
                }

                void IRegistrationLocationSelectionSyntax<TComponent>.InsteadOf<TRegistrationType>()
                {
                    if (newRegistration.ComponentType != typeof(TRegistrationType))
                    {
                        EnsureNoDuplicateRegistrationType();
                    }

                    var registrationIndex = EnsureRegistrationExists<TRegistrationType>();
                    registrations[registrationIndex] = newRegistration;
                }

                void IRegistrationLocationSelectionSyntax<TComponent>.After<TRegistrationType>()
                {
                    EnsureNoDuplicateRegistrationType();
                    var registrationIndex = EnsureRegistrationExists<TRegistrationType>();
                    registrations.Insert(registrationIndex + 1, newRegistration);
                }

                void IRegistrationLocationSelectionSyntax<TComponent>.Before<TRegistrationType>()
                {
                    EnsureNoDuplicateRegistrationType();
                    var registrationIndex = EnsureRegistrationExists<TRegistrationType>();
                    registrations.Insert(registrationIndex, newRegistration);
                }

                void IRegistrationLocationSelectionSyntax<TComponent>.OnBottom()
                {
                    EnsureNoDuplicateRegistrationType();
                    registrations.Add(newRegistration);
                }

                void IRegistrationLocationSelectionSyntax<TComponent>.OnTop()
                {
                    EnsureNoDuplicateRegistrationType();
                    registrations.Insert(0, newRegistration);
                }
            }
        }

        public delegate TTypeInspector TypeInspectorFactoryDelegate<TTypeInspector>(ITypeInspector innerTypeInspector) where TTypeInspector : ITypeInspector;

        public interface IRegistrationLocationSelectionSyntax<TBaseRegistrationType>
        {
            /// <summary>
            /// Registers the component in place of the already registered component of type <typeparamref name="TRegistrationType" />.
            /// </summary>
            void InsteadOf<TRegistrationType>() where TRegistrationType : TBaseRegistrationType;

            /// <summary>
            /// Registers the component before the already registered component of type <typeparamref name="TRegistrationType" />.
            /// </summary>
            void Before<TRegistrationType>() where TRegistrationType : TBaseRegistrationType;

            /// <summary>
            /// Registers the component after the already registered component of type <typeparamref name="TRegistrationType" />.
            /// </summary>
            void After<TRegistrationType>() where TRegistrationType : TBaseRegistrationType;

            /// <summary>
            /// Registers the component before every other previously registered component.
            /// </summary>
            void OnTop();

            /// <summary>
            /// Registers the component after every other previously registered component.
            /// </summary>
            void OnBottom();
        }

        /// <summary>
        /// Sets the <see cref="IObjectFactory" /> that will be used by the deserializer.
        /// </summary>
        public DeserializerBuilder WithObjectFactory(IObjectFactory objectFactory)
        {
            if (objectFactory == null)
            {
                throw new ArgumentNullException(nameof(objectFactory));
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
                throw new ArgumentNullException(nameof(objectFactory));
            }

            return WithObjectFactory(new LambdaObjectFactory(objectFactory));
        }

        /// <summary>
        /// Sets the <see cref="INamingConvention" /> that will be used by the deserializer.
        /// </summary>
        public DeserializerBuilder WithNamingConvention(INamingConvention namingConvention)
        {
            if (namingConvention == null)
            {
                throw new ArgumentNullException(nameof(namingConvention));
            }

            this.namingConvention = namingConvention;
            return this;
        }

        /// <summary>
        /// Registers an additional <see cref="ITypeInspector" /> to be used by the deserializer.
        /// </summary>
        /// <param name="typeInspectorFactory">A function that instantiates the type inspector.</param>
        public DeserializerBuilder WithTypeInspector<TTypeInspector>(TypeInspectorFactoryDelegate<TTypeInspector> typeInspectorFactory)
            where TTypeInspector : ITypeInspector
        {
            return WithTypeInspector<TTypeInspector>(typeInspectorFactory, w => w.OnTop());
        }

        /// <summary>
        /// Registers an additional <see cref="ITypeInspector" /> to be used by the deserializer.
        /// </summary>
        /// <param name="typeInspectorFactory">A function that instantiates the type inspector.</param>
        /// <param name="where">Configures the location where to insert the <see cref="ITypeInspector" /></param>
        public DeserializerBuilder WithTypeInspector<TTypeInspector>(
            TypeInspectorFactoryDelegate<TTypeInspector> typeInspectorFactory,
            Action<IRegistrationLocationSelectionSyntax<ITypeInspector>> where
        )
            where TTypeInspector : ITypeInspector
        {
            if (typeInspectorFactory == null)
            {
                throw new ArgumentNullException(nameof(typeInspectorFactory));
            }

            if (where == null)
            {
                throw new ArgumentNullException(nameof(where));
            }

            where(typeInspectorFactories.CreateRegistrationLocationSelector(typeof(TTypeInspector), inner => typeInspectorFactory(inner)));
            return this;
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
        /// Registers an additional <see cref="IYamlTypeConverter" /> to be used by the deserializer.
        /// </summary>
        public DeserializerBuilder WithTypeConverter(IYamlTypeConverter typeConverter)
        {
            return WithTypeConverter(typeConverter, w => w.OnTop());
        }

        /// <summary>
        /// Registers an additional <see cref="IYamlTypeConverter" /> to be used by the deserializer.
        /// </summary>
        /// <param name="typeConverter"></param>
        /// <param name="where">Configures the location where to insert the <see cref="IYamlTypeConverter" /></param>
        public DeserializerBuilder WithTypeConverter(
            IYamlTypeConverter typeConverter,
            Action<IRegistrationLocationSelectionSyntax<IYamlTypeConverter>> where
        )
        {
            if (typeConverter == null)
            {
                throw new ArgumentNullException(nameof(typeConverter));
            }

            if (where == null)
            {
                throw new ArgumentNullException(nameof(where));
            }

            where(typeConverters.CreateRegistrationLocationSelector(typeConverter.GetType(), _ => typeConverter));
            return this;
        }

        /// <summary>
        /// Registers a tag mapping.
        /// </summary>
        public DeserializerBuilder WithTagMapping(string tag, Type type)
        {
            if (tag == null)
            {
                throw new ArgumentNullException(nameof(tag));
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            tagMappings.Add(tag, type);
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
        /// Register an <see cref="Attribute"/> for for a given property.
        /// </summary>
        /// <typeparam name="TClass"></typeparam>
        /// <param name="propertyAccessor">An expression in the form: x => x.SomeProperty</param>
        /// <param name="attribute">The sttribute to register.</param>
        /// <returns></returns>
        public DeserializerBuilder WithAttributeOverride<TClass>(Expression<Func<TClass, object>> propertyAccessor, Attribute attribute)
        {
            overrides.Add(propertyAccessor, attribute);
            return this;
        }

        /// <summary>
        /// Register an <see cref="Attribute"/> for for a given property.
        /// </summary>
        public DeserializerBuilder WithAttributeOverride(Type type, string member, Attribute attribute)
        {
            overrides.Add(type, member, attribute);
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
                    nodeDeserializerFactories.Select(r => r.Factory(null)).ToList(),
                    nodeTypeResolverFactories.Select(r => r.Factory(null)).ToList()
                )
            );
        }
    }
}