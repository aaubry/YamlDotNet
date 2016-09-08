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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using YamlDotNet.Helpers;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.ObjectGraphTraversalStrategies
{
    /// <summary>
    /// An implementation of <see cref="IObjectGraphTraversalStrategy"/> that traverses
    /// readable properties, collections and dictionaries.
    /// </summary>
    public class FullObjectGraphTraversalStrategy : IObjectGraphTraversalStrategy
    {
        private readonly int maxRecursion;
        private readonly ITypeInspector typeDescriptor;
        private readonly ITypeResolver typeResolver;
        private INamingConvention namingConvention;

        public FullObjectGraphTraversalStrategy(ITypeInspector typeDescriptor, ITypeResolver typeResolver, int maxRecursion, INamingConvention namingConvention)
        {
            if (maxRecursion <= 0)
            {
                throw new ArgumentOutOfRangeException("maxRecursion", maxRecursion, "maxRecursion must be greater than 1");
            }

            if (typeDescriptor == null)
            {
                throw new ArgumentNullException("typeDescriptor");
            }

            this.typeDescriptor = typeDescriptor;

            if (typeResolver == null)
            {
                throw new ArgumentNullException("typeResolver");
            }

            this.typeResolver = typeResolver;

            this.maxRecursion = maxRecursion;
            this.namingConvention = namingConvention;
        }

        void IObjectGraphTraversalStrategy.Traverse<TContext>(IObjectDescriptor graph, IObjectGraphVisitor<TContext> visitor, TContext context)
        {
            Traverse(graph, visitor, 0, context);
        }

        protected virtual void Traverse<TContext>(IObjectDescriptor value, IObjectGraphVisitor<TContext> visitor, int currentDepth, TContext context)
        {
            if (++currentDepth > maxRecursion)
            {
                throw new InvalidOperationException("Too much recursion when traversing the object graph");
            }

            if (!visitor.Enter(value, context))
            {
                return;
            }

            var typeCode = value.Type.GetTypeCode();
            switch (typeCode)
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.String:
                case TypeCode.Char:
                case TypeCode.DateTime:
                    visitor.VisitScalar(value, context);
                    break;

                case TypeCode.DBNull:
                    visitor.VisitScalar(new ObjectDescriptor(null, typeof(object), typeof(object)), context);
                    break;

                case TypeCode.Empty:
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "TypeCode.{0} is not supported.", typeCode));

                default:
                    if (value.Value == null || value.Type == typeof(TimeSpan))
                    {
                        visitor.VisitScalar(value, context);
                        break;
                    }

                    var underlyingType = Nullable.GetUnderlyingType(value.Type);
                    if (underlyingType != null)
                    {
                        // This is a nullable type, recursively handle it with its underlying type.
                        // Note that if it contains null, the condition above already took care of it
                        Traverse(new ObjectDescriptor(value.Value, underlyingType, value.Type, value.ScalarStyle), visitor, currentDepth, context);
                    }
                    else
                    {
                        TraverseObject(value, visitor, currentDepth, context);
                    }
                    break;
            }
        }

        protected virtual void TraverseObject<TContext>(IObjectDescriptor value, IObjectGraphVisitor<TContext> visitor, int currentDepth, TContext context)
        {
            if (typeof(IDictionary).IsAssignableFrom(value.Type))
            {
                TraverseDictionary(value, visitor, currentDepth, typeof(object), typeof(object), context);
                return;
            }

            var genericDictionaryType = ReflectionUtility.GetImplementedGenericInterface(value.Type, typeof(IDictionary<,>));
            if (genericDictionaryType != null)
            {
                var adaptedDictionary = new GenericDictionaryToNonGenericAdapter(value.Value, genericDictionaryType);
                var genericArguments = genericDictionaryType.GetGenericArguments();
                TraverseDictionary(new ObjectDescriptor(adaptedDictionary, value.Type, value.StaticType, value.ScalarStyle), visitor, currentDepth, genericArguments[0], genericArguments[1], context);
                return;
            }

            if (typeof(IEnumerable).IsAssignableFrom(value.Type))
            {
                TraverseList(value, visitor, currentDepth, context);
                return;
            }

            TraverseProperties(value, visitor, currentDepth, context);
        }

        protected virtual void TraverseDictionary<TContext>(IObjectDescriptor dictionary, IObjectGraphVisitor<TContext> visitor, int currentDepth, Type keyType, Type valueType, TContext context)
        {
            visitor.VisitMappingStart(dictionary, keyType, valueType, context);

            var isDynamic = dictionary.Type.FullName.Equals("System.Dynamic.ExpandoObject");
            foreach (DictionaryEntry entry in (IDictionary)dictionary.Value)
            {
                var keyString = isDynamic ? namingConvention.Apply(entry.Key.ToString()) : entry.Key.ToString();
                var key = GetObjectDescriptor(keyString, keyType);
                var value = GetObjectDescriptor(entry.Value, valueType);

                if (visitor.EnterMapping(key, value, context))
                {
                    Traverse(key, visitor, currentDepth, context);
                    Traverse(value, visitor, currentDepth, context);
                }
            }

            visitor.VisitMappingEnd(dictionary, context);
        }

        private void TraverseList<TContext>(IObjectDescriptor value, IObjectGraphVisitor<TContext> visitor, int currentDepth, TContext context)
        {
            var enumerableType = ReflectionUtility.GetImplementedGenericInterface(value.Type, typeof(IEnumerable<>));
            var itemType = enumerableType != null ? enumerableType.GetGenericArguments()[0] : typeof(object);

            visitor.VisitSequenceStart(value, itemType, context);

            foreach (var item in (IEnumerable)value.Value)
            {
                Traverse(GetObjectDescriptor(item, itemType), visitor, currentDepth, context);
            }

            visitor.VisitSequenceEnd(value, context);
        }

        protected virtual void TraverseProperties<TContext>(IObjectDescriptor value, IObjectGraphVisitor<TContext> visitor, int currentDepth, TContext context)
        {
            visitor.VisitMappingStart(value, typeof(string), typeof(object), context);

            foreach (var propertyDescriptor in typeDescriptor.GetProperties(value.Type, value.Value))
            {
                var propertyValue = propertyDescriptor.Read(value.Value);

                if (visitor.EnterMapping(propertyDescriptor, propertyValue, context))
                {
                    Traverse(new ObjectDescriptor(propertyDescriptor.Name, typeof(string), typeof(string)), visitor, currentDepth, context);
                    Traverse(propertyValue, visitor, currentDepth, context);
                }
            }

            visitor.VisitMappingEnd(value, context);
        }

        private IObjectDescriptor GetObjectDescriptor(object value, Type staticType)
        {
            return new ObjectDescriptor(value, typeResolver.Resolve(staticType, value), staticType);
        }
    }
}