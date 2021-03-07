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
using YamlDotNet.Core;
using YamlDotNet.Representation;
using YamlDotNet.Representation.Schemas;
using YamlDotNet.Serialization.Converters;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.Schemas;
using YamlDotNet.Serialization.TypeInspectors;

namespace YamlDotNet.Serialization
{
    /// <summary>
    /// Common implementation of <see cref="SerializerBuilder" /> and <see cref="DeserializerBuilder" />.
    /// </summary>
    public abstract class BuilderSkeleton<TBuilder>where TBuilder : BuilderSkeleton<TBuilder>
    {
        internal ISchema schema = new CompositeSchema(
            DotNetSchema.Instance,
            CoreSchema.Scalars
        );

        internal INamingConvention namingConvention = NullNamingConvention.Instance;
        internal ITypeResolver typeResolver;
        internal readonly YamlAttributeOverrides overrides;
        internal readonly LazyComponentRegistrationList<Nothing, IYamlTypeConverter> typeConverterFactories;
        internal readonly LazyComponentRegistrationList<ITypeInspector, ITypeInspector> typeInspectorFactories;
        private bool ignoreFields;
        private bool includeNonPublicProperties = false;

        internal BuilderSkeleton(ITypeResolver typeResolver)
        {
            overrides = new YamlAttributeOverrides();

            typeConverterFactories = new LazyComponentRegistrationList<Nothing, IYamlTypeConverter>
            {
                { typeof(GuidConverter), _ => new GuidConverter(false) },
                { typeof(SystemTypeConverter), _ => new SystemTypeConverter() }
            };

            typeInspectorFactories = new LazyComponentRegistrationList<ITypeInspector, ITypeInspector>();
            this.typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));
        }

        protected abstract TBuilder Self { get; }

        internal ITypeInspector BuildTypeInspector()
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

        /// <summary>
        /// Specifies the base schema that will be used.
        /// </summary>
        public TBuilder WithSchema(ISchema schema)
        {
            this.schema = schema;
            return Self;
        }

        /// <summary>
        /// Prevents serialization and deserialization of fields.
        /// </summary>
        public TBuilder IgnoreFields()
        {
            ignoreFields = true;
            return Self;
        }

        /// <summary>
        /// Allows serialization and deserialization of non-public properties.
        /// </summary>
        public TBuilder IncludeNonPublicProperties()
        {
            includeNonPublicProperties = true;
            return Self;
        }

        /// <summary>
        /// Sets the <see cref="INamingConvention" /> that will be used by the (de)serializer.
        /// </summary>
        public TBuilder WithNamingConvention(INamingConvention namingConvention)
        {
            this.namingConvention = namingConvention ?? throw new ArgumentNullException(nameof(namingConvention));
            return Self;
        }

        /// <summary>
        /// Sets the <see cref="ITypeResolver" /> that will be used by the (de)serializer.
        /// </summary>
        public TBuilder WithTypeResolver(ITypeResolver typeResolver)
        {
            this.typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));
            return Self;
        }

        public abstract TBuilder WithTagMapping(TagName tag, Type type);

#if !NET20
        /// <summary>
        /// Register an <see cref="Attribute"/> for a given property.
        /// </summary>
        /// <typeparam name="TClass"></typeparam>
        /// <param name="propertyAccessor">An expression in the form: x => x.SomeProperty</param>
        /// <param name="attribute">The attribute to register.</param>
        /// <returns></returns>
        public TBuilder WithAttributeOverride<TClass>(Expression<Func<TClass, object?>> propertyAccessor, Attribute attribute)
        {
            overrides.Add(propertyAccessor, attribute);
            return Self;
        }
#endif

        /// <summary>
        /// Register an <see cref="Attribute"/> for a given property.
        /// </summary>
        public TBuilder WithAttributeOverride(Type type, string member, Attribute attribute)
        {
            overrides.Add(type, member, attribute);
            return Self;
        }

        /// <summary>
        /// Registers an additional <see cref="IYamlTypeConverter" /> to be used by the (de)serializer.
        /// </summary>
        public TBuilder WithTypeConverter(IYamlTypeConverter typeConverter)
        {
            return WithTypeConverter(typeConverter, w => w.OnTop());
        }

        /// <summary>
        /// Registers an additional <see cref="IYamlTypeConverter" /> to be used by the (de)serializer.
        /// </summary>
        /// <param name="typeConverter"></param>
        /// <param name="where">Configures the location where to insert the <see cref="IYamlTypeConverter" /></param>
        public TBuilder WithTypeConverter(
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

            where(typeConverterFactories.CreateRegistrationLocationSelector(typeConverter.GetType(), _ => typeConverter));
            return Self;
        }

        /// <summary>
        /// Registers an additional <see cref="IYamlTypeConverter" /> to be used by the (de)serializer.
        /// </summary>
        /// <param name="typeConverterFactory">A factory that creates the <see cref="IYamlTypeConverter" /> based on a previously registered <see cref="IYamlTypeConverter" />.</param>
        /// <param name="where">Configures the location where to insert the <see cref="IYamlTypeConverter" /></param>
        public TBuilder WithTypeConverter<TYamlTypeConverter>(
            WrapperFactory<IYamlTypeConverter, IYamlTypeConverter> typeConverterFactory,
            Action<ITrackingRegistrationLocationSelectionSyntax<IYamlTypeConverter>> where
        )
            where TYamlTypeConverter : IYamlTypeConverter
        {
            if (typeConverterFactory == null)
            {
                throw new ArgumentNullException(nameof(typeConverterFactory));
            }

            if (where == null)
            {
                throw new ArgumentNullException(nameof(where));
            }

            where(typeConverterFactories.CreateTrackingRegistrationLocationSelector(typeof(TYamlTypeConverter), (wrapped, _) => typeConverterFactory(wrapped)));
            return Self;
        }

        /// <summary>
        /// Unregisters an existing <see cref="IYamlTypeConverter" /> of type <typeparam name="TYamlTypeConverter" />.
        /// </summary>
        public TBuilder WithoutTypeConverter<TYamlTypeConverter>()
            where TYamlTypeConverter : IYamlTypeConverter
        {
            return WithoutTypeConverter(typeof(TYamlTypeConverter));
        }

        /// <summary>
        /// Unregisters an existing <see cref="IYamlTypeConverter" /> of type <param name="converterType" />.
        /// </summary>
        public TBuilder WithoutTypeConverter(Type converterType)
        {
            if (converterType == null)
            {
                throw new ArgumentNullException(nameof(converterType));
            }

            typeConverterFactories.Remove(converterType);
            return Self;
        }

        /// <summary>
        /// Registers an additional <see cref="ITypeInspector" /> to be used by the (de)serializer.
        /// </summary>
        /// <param name="typeInspectorFactory">A function that instantiates the type inspector.</param>
        public TBuilder WithTypeInspector<TTypeInspector>(Func<ITypeInspector, TTypeInspector> typeInspectorFactory)
            where TTypeInspector : ITypeInspector
        {
            return WithTypeInspector(typeInspectorFactory, w => w.OnTop());
        }

        /// <summary>
        /// Registers an additional <see cref="ITypeInspector" /> to be used by the (de)serializer.
        /// </summary>
        /// <param name="typeInspectorFactory">A function that instantiates the type inspector.</param>
        /// <param name="where">Configures the location where to insert the <see cref="ITypeInspector" /></param>
        public TBuilder WithTypeInspector<TTypeInspector>(
            Func<ITypeInspector, TTypeInspector> typeInspectorFactory,
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
            return Self;
        }

        /// <summary>
        /// Registers an additional <see cref="ITypeInspector" /> to be used by the (de)serializer.
        /// </summary>
        /// <param name="typeInspectorFactory">A function that instantiates the type inspector based on a previously registered <see cref="ITypeInspector" />..</param>
        /// <param name="where">Configures the location where to insert the <see cref="ITypeInspector" /></param>
        public TBuilder WithTypeInspector<TTypeInspector>(
            WrapperFactory<ITypeInspector, ITypeInspector, TTypeInspector> typeInspectorFactory,
            Action<ITrackingRegistrationLocationSelectionSyntax<ITypeInspector>> where
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

            where(typeInspectorFactories.CreateTrackingRegistrationLocationSelector(typeof(TTypeInspector), (wrapped, inner) => typeInspectorFactory(wrapped, inner)));
            return Self;
        }

        /// <summary>
        /// Unregisters an existing <see cref="ITypeInspector" /> of type <typeparam name="TTypeInspector" />.
        /// </summary>
        public TBuilder WithoutTypeInspector<TTypeInspector>()
            where TTypeInspector : ITypeInspector
        {
            return WithoutTypeInspector(typeof(TTypeInspector));
        }

        /// <summary>
        /// Unregisters an existing <see cref="ITypeInspector" /> of type <param name="inspectorType" />.
        /// </summary>
        public TBuilder WithoutTypeInspector(Type inspectorType)
        {
            if (inspectorType == null)
            {
                throw new ArgumentNullException(nameof(inspectorType));
            }

            typeInspectorFactories.Remove(inspectorType);
            return Self;
        }

        protected IEnumerable<IYamlTypeConverter> BuildTypeConverters()
        {
            return typeConverterFactories.BuildComponentList();
        }


        protected Func<Type, ISchema> BuildSchemaFactory(IReadOnlyDictionary<Type, TagName> tagMappings, bool ignoreUnmatched)
        {
            var tagNameResolver = new CompositeTagNameResolver(
                new TableTagNameResolver(tagMappings),
                TypeNameTagNameResolver.Instance
            );

            var typeInspector = BuildTypeInspector();

            var typeMatchers = new TypeMatcherTable(requireThreadSafety: true) // TODO: Configure requireThreadSafety
            {
                {
                    typeof(IEnumerable<>),
                    (concrete, iCollection, lookupMatcher) =>
                    {
                        var tag = tagNameResolver.Resolve(concrete);

                        var genericArguments = iCollection.GetGenericArguments();
                        var itemType = genericArguments[0];

                        var implementation = concrete;
                        if (concrete.IsInterface())
                        {
                            implementation = typeof(List<>).MakeGenericType(genericArguments);
                        }

                        var matcher = NodeMatcher
                            .ForSequences(SequenceMapper.Create(tag, implementation, itemType), concrete)
                            .Either(
                                s => s.MatchEmptyTags(),
                                s => s.MatchTag(tag)
                            )
                            .Create();

                        return (
                            matcher,
                            () => matcher.AddItemMatcher(lookupMatcher(itemType))
                        );
                    }
                },
                {
                    typeof(IDictionary<,>),
                    (concrete, iDictionary, lookupMatcher) =>
                    {
                        var tag = tagNameResolver.Resolve(concrete);

                        var genericArguments = iDictionary.GetGenericArguments();
                        var keyType = genericArguments[0];
                        var valueType = genericArguments[1];

                        var implementation = concrete;
                        if (concrete.IsInterface())
                        {
                            implementation = typeof(Dictionary<,>).MakeGenericType(genericArguments);
                        }

                        var matcher = NodeMatcher
                            .ForMappings(MappingMapper.Create(tag, implementation, keyType, valueType), concrete)
                            .Either(
                                s => s.MatchEmptyTags(),
                                s => s.MatchTag(tag)
                            )
                            .Create();

                        return (
                            matcher,
                            () =>
                            {
                                matcher.AddItemMatcher(
                                    keyMatcher: lookupMatcher(keyType),
                                    valueMatchers: lookupMatcher(valueType)
                                );
                            }
                        );
                    }
                },
                {
                    typeof(object),
                    (concrete, _, lookupMatcher) =>
                    {
                        if (concrete == typeof(object))
                        {
                            return (NodeMatcher.NoMatch, null);
                        }

                        var tag = tagNameResolver.Resolve(concrete);

                        var properties = typeInspector.GetProperties(concrete, null).OrderBy(p => p.Order);
                        var mapper = new ObjectMapper2(concrete, properties, tag, ignoreUnmatched);

                        var matcher = NodeMatcher
                            .ForMappings(mapper, concrete)
                            .Either(
                                s => s.MatchEmptyTags(),
                                s => s.MatchTag(tag)
                            )
                            .Create();

                        return (
                            matcher,
                            () =>
                            {
                                // TODO: Update the object mapper with the specific properties that exist (or create it complete from the start)

                                {
                                    foreach (var property in properties)
                                    {
                                        var keyName = namingConvention.Apply(property.Name);

                                        // TODO: Use the following:
                                        //        - property.CanWrite
                                        //        - property.ScalarStyle
                                        //        - property.TypeOverride

                                        matcher.AddItemMatcher(
                                            keyMatcher: NodeMatcher
                                                .ForScalars(new TranslateStringMapper(keyName, property.Name))
                                                .MatchValue(keyName)
                                                .Create(),
                                            valueMatchers: lookupMatcher(property.Type)
                                        );
                                    }
                                }
                                {
                                    //// TODO: Type inspector
                                    //var properties = concrete.GetPublicProperties();
                                    //foreach (var property in properties)
                                    //{
                                    //    var keyName = namingConvention.Apply(property.Name);

                                    //    matcher.AddItemMatcher(
                                    //        keyMatcher: NodeMatcher
                                    //            .ForScalars(new TranslateStringMapper(keyName, property.Name))
                                    //            .MatchValue(keyName)
                                    //            .Create(),
                                    //        valueMatchers: lookupMatcher(property.PropertyType)
                                    //    );
                                    //}
                                }
                            }
                        );
                    }
                }
            };

            foreach (var knownType in schema.KnownTypes)
            {
                typeMatchers.Add(knownType, (_, __, ___) => NodeMatcher.NoMatch);
            }

            return root => new CompositeSchema(
                new TypeSchema(typeMatchers, root, tagMappings.Keys),
                schema
            );
        }
    }

    class BreakMapper : Representation.INodeMapper
    {
        public TagName Tag => YamlTagRepository.String;

        public INodeMapper Canonical => this;

        public object? Construct(Node node)
        {
            throw new NotImplementedException();
        }

        public Node Represent(object? native, ISchemaIterator iterator, IRepresentationState state)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// A factory that creates instances of <typeparamref name="TComponent" /> based on an existing <typeparamref name="TComponentBase" />.
    /// </summary>
    /// <typeparam name="TComponentBase">The type of the wrapped component.</typeparam>
    /// <typeparam name="TComponent">The type of the component that this factory creates.</typeparam>
    /// <param name="wrapped">The component that is to be wrapped.</param>
    /// <returns>Returns a new instance of <typeparamref name="TComponent" /> that is based on <paramref name="wrapped" />.</returns>
    public delegate TComponent WrapperFactory<TComponentBase, TComponent>(TComponentBase wrapped) where TComponent : TComponentBase;

    /// <summary>
    /// A factory that creates instances of <typeparamref name="TComponent" /> based on an existing <typeparamref name="TComponentBase" /> and an argument.
    /// </summary>
    /// <typeparam name="TArgument">The type of the argument.</typeparam>
    /// <typeparam name="TComponentBase">The type of the wrapped component.</typeparam>
    /// <typeparam name="TComponent">The type of the component that this factory creates.</typeparam>
    /// <param name="wrapped">The component that is to be wrapped.</param>
    /// <param name="argument">The argument of the factory.</param>
    /// <returns>Returns a new instance of <typeparamref name="TComponent" /> that is based on <paramref name="wrapped" /> and <paramref name="argument" />.</returns>
    public delegate TComponent WrapperFactory<TArgument, TComponentBase, TComponent>(TComponentBase wrapped, TArgument argument) where TComponent : TComponentBase;
}