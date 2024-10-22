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
using YamlDotNet.Core;
using YamlDotNet.Serialization.Converters;
using YamlDotNet.Serialization.NamingConventions;

namespace YamlDotNet.Serialization
{
#pragma warning disable IDE0055 // Fix formatting
    /// <summary>
    /// Common implementation of <see cref="SerializerBuilder" /> and <see cref="DeserializerBuilder" />.
    /// </summary>
#pragma warning disable CA1708 // Identifiers should differ by more than case
    public abstract class BuilderSkeleton<TBuilder>
#pragma warning restore CA1708
        where TBuilder : BuilderSkeleton<TBuilder>
    {
#pragma warning restore IDE0055
        internal INamingConvention namingConvention = NullNamingConvention.Instance;
        internal INamingConvention enumNamingConvention = NullNamingConvention.Instance;
        internal ITypeResolver typeResolver;
        internal readonly YamlAttributeOverrides overrides;
        internal readonly LazyComponentRegistrationList<Nothing, IYamlTypeConverter> typeConverterFactories;
        internal readonly LazyComponentRegistrationList<ITypeInspector, ITypeInspector> typeInspectorFactories;
        internal bool ignoreFields;
        internal bool includeNonPublicProperties;
        internal Settings settings;
        internal YamlFormatter yamlFormatter = YamlFormatter.Default;

        internal BuilderSkeleton(ITypeResolver typeResolver)
        {
            overrides = new YamlAttributeOverrides();

            typeConverterFactories = new LazyComponentRegistrationList<Nothing, IYamlTypeConverter>
            {
                { typeof(GuidConverter), _ => new GuidConverter(false) },
                { typeof(SystemTypeConverter), _ => new SystemTypeConverter() }
            };

            typeInspectorFactories = new ();
            this.typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));
            settings = new Settings();
        }

        protected abstract TBuilder Self { get; }

        /// <summary>
        /// Prevents serialization and deserialization of fields.
        /// </summary>
        /// <returns></returns>
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
        /// Calling this will enable the support for private constructors when considering serialization and deserialization.
        /// </summary>
        public TBuilder EnablePrivateConstructors()
        {
            settings.AllowPrivateConstructors = true;
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
        /// Sets the <see cref="INamingConvention"/> to use when handling enum's.
        /// </summary>
        /// <param name="enumNamingConvention">Naming convention to use when handling enum's</param>
        /// <returns></returns>
        public TBuilder WithEnumNamingConvention(INamingConvention enumNamingConvention)
        {
            this.enumNamingConvention = enumNamingConvention;
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
        public TBuilder WithAttributeOverride<TClass>(System.Linq.Expressions.Expression<Func<TClass, object>> propertyAccessor, Attribute attribute)
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

        /// <summary>
        /// Override the default yaml formatter with the one passed in
        /// </summary>
        /// <param name="formatter"><seealso cref="YamlFormatter"/>to use when serializing and deserializing objects.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public TBuilder WithYamlFormatter(YamlFormatter formatter)
        {
            yamlFormatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            return Self;
        }

        protected IEnumerable<IYamlTypeConverter> BuildTypeConverters()
        {
            return typeConverterFactories.BuildComponentList();
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
