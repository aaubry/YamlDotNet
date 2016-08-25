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
using System.Linq.Expressions;
using YamlDotNet.Serialization.Converters;
using YamlDotNet.Serialization.TypeInspectors;

namespace YamlDotNet.Serialization
{
    /// <summary>
    /// Common implementation of <see cref="SerializerBuilder" /> and <see cref="DeserializerBuilder" />.
    /// </summary>
    public abstract class BuilderSkeleton<TBuilder>
        where TBuilder : BuilderSkeleton<TBuilder>
    {
        internal INamingConvention namingConvention;
        internal ITypeResolver typeResolver;
        internal readonly YamlAttributeOverrides overrides;
        internal readonly LazyComponentRegistrationList<Nothing, IYamlTypeConverter> typeConverterFactories;
        internal readonly LazyComponentRegistrationList<ITypeInspector, ITypeInspector> typeInspectorFactories;

        internal BuilderSkeleton()
        {
            overrides = new YamlAttributeOverrides();

            typeConverterFactories = new LazyComponentRegistrationList<Nothing, IYamlTypeConverter>();
            typeConverterFactories.Add(typeof(GuidConverter), _ => new GuidConverter(false));

            typeInspectorFactories = new LazyComponentRegistrationList<ITypeInspector, ITypeInspector>();
        }

        protected abstract TBuilder Self { get; }

        internal ITypeInspector BuildTypeInspector()
        {
            return typeInspectorFactories.BuildComponentChain(
                new ReadablePropertiesTypeInspector(typeResolver)
            );
        }

        /// <summary>
        /// Sets the <see cref="INamingConvention" /> that will be used by the (de)serializer.
        /// </summary>
        public TBuilder WithNamingConvention(INamingConvention namingConvention)
        {
            if (namingConvention == null)
            {
                throw new ArgumentNullException(nameof(namingConvention));
            }

            this.namingConvention = namingConvention;
            return Self;
        }

        /// <summary>
        /// Sets the <see cref="ITypeResolver" /> that will be used by the (de)serializer.
        /// </summary>
        public TBuilder WithTypeResolver(ITypeResolver typeResolver)
        {
            if (typeResolver == null)
            {
                throw new ArgumentNullException(nameof(typeResolver));
            }

            this.typeResolver = typeResolver;
            return Self;
        }

        /// <summary>
        /// Register an <see cref="Attribute"/> for for a given property.
        /// </summary>
        /// <typeparam name="TClass"></typeparam>
        /// <param name="propertyAccessor">An expression in the form: x => x.SomeProperty</param>
        /// <param name="attribute">The attribute to register.</param>
        /// <returns></returns>
        public TBuilder WithAttributeOverride<TClass>(Expression<Func<TClass, object>> propertyAccessor, Attribute attribute)
        {
            overrides.Add(propertyAccessor, attribute);
            return Self;
        }

        /// <summary>
        /// Register an <see cref="Attribute"/> for for a given property.
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

            typeConvertersCache = null;

            where(typeConverterFactories.CreateRegistrationLocationSelector(typeConverter.GetType(), _ => typeConverter));
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

        private IEnumerable<IYamlTypeConverter> typeConvertersCache;

        protected IEnumerable<IYamlTypeConverter> BuildTypeConverters()
        {
            if (typeConvertersCache == null)
            {
                typeConvertersCache = typeConverterFactories.BuildComponentList();
            }
            return typeConvertersCache;
        }
    }
}