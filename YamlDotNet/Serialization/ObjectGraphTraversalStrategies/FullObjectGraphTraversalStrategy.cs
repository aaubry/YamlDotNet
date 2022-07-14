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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Core;
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
        private readonly INamingConvention namingConvention;

        public FullObjectGraphTraversalStrategy(ITypeInspector typeDescriptor, ITypeResolver typeResolver, int maxRecursion, INamingConvention namingConvention)
        {
            if (maxRecursion <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRecursion), maxRecursion, "maxRecursion must be greater than 1");
            }

            this.typeDescriptor = typeDescriptor ?? throw new ArgumentNullException(nameof(typeDescriptor));
            this.typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));

            this.maxRecursion = maxRecursion;
            this.namingConvention = namingConvention ?? throw new ArgumentNullException(nameof(namingConvention));
        }

        void IObjectGraphTraversalStrategy.Traverse<TContext>(IObjectDescriptor graph, IObjectGraphVisitor<TContext> visitor, TContext context)
        {
            Traverse("<root>", graph, visitor, context, new Stack<ObjectPathSegment>(maxRecursion));
        }

        protected struct ObjectPathSegment
        {
            public readonly object Name;
            public readonly IObjectDescriptor Value;

            public ObjectPathSegment(object name, IObjectDescriptor value)
            {
                this.Name = name;
                this.Value = value;
            }
        }

        protected virtual void Traverse<TContext>(object name, IObjectDescriptor value, IObjectGraphVisitor<TContext> visitor, TContext context, Stack<ObjectPathSegment> path)
        {
            if (path.Count >= maxRecursion)
            {
                var message = new StringBuilder();
                message.AppendLine("Too much recursion when traversing the object graph.");
                message.AppendLine("The path to reach this recursion was:");

                var lines = new Stack<KeyValuePair<string, string>>(path.Count);
                var maxNameLength = 0;
                foreach (var segment in path)
                {
                    var segmentName = TypeConverter.ChangeType<string>(segment.Name);
                    maxNameLength = Math.Max(maxNameLength, segmentName.Length);
                    lines.Push(new KeyValuePair<string, string>(segmentName, segment.Value.Type.FullName!));
                }

                foreach (var line in lines)
                {
                    message
                        .Append(" -> ")
                        .Append(line.Key.PadRight(maxNameLength))
                        .Append("  [")
                        .Append(line.Value)
                        .AppendLine("]");
                }

                throw new MaximumRecursionLevelReachedException(message.ToString());
            }

            if (!visitor.Enter(value, context))
            {
                return;
            }

            path.Push(new ObjectPathSegment(name, value));
            try
            {
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

                    case TypeCode.Empty:
                        throw new NotSupportedException($"TypeCode.{typeCode} is not supported.");

                    default:
                        if (value.IsDbNull())
                        {
                            visitor.VisitScalar(new ObjectDescriptor(null, typeof(object), typeof(object)), context);
                        }

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
                            Traverse("Value", new ObjectDescriptor(value.Value, underlyingType, value.Type, value.ScalarStyle), visitor, context, path);
                        }
                        else
                        {
                            TraverseObject(value, visitor, context, path);
                        }
                        break;
                }
            }
            finally
            {
                path.Pop();
            }
        }

        protected virtual void TraverseObject<TContext>(IObjectDescriptor value, IObjectGraphVisitor<TContext> visitor, TContext context, Stack<ObjectPathSegment> path)
        {
            if (typeof(IDictionary).IsAssignableFrom(value.Type))
            {
                TraverseDictionary(value, visitor, typeof(object), typeof(object), context, path);
                return;
            }

            var genericDictionaryType = ReflectionUtility.GetImplementedGenericInterface(value.Type, typeof(IDictionary<,>));
            if (genericDictionaryType != null)
            {
                var genericArguments = genericDictionaryType.GetGenericArguments();
                var adaptedDictionary = Activator.CreateInstance(typeof(GenericDictionaryToNonGenericAdapter<,>).MakeGenericType(genericArguments), value.Value)!;
                TraverseDictionary(new ObjectDescriptor(adaptedDictionary, value.Type, value.StaticType, value.ScalarStyle), visitor, genericArguments[0], genericArguments[1], context, path);
                return;
            }

            if (typeof(IEnumerable).IsAssignableFrom(value.Type))
            {
                TraverseList(value, visitor, context, path);
                return;
            }

            TraverseProperties(value, visitor, context, path);
        }

        protected virtual void TraverseDictionary<TContext>(IObjectDescriptor dictionary, IObjectGraphVisitor<TContext> visitor, Type keyType, Type valueType, TContext context, Stack<ObjectPathSegment> path)
        {
            visitor.VisitMappingStart(dictionary, keyType, valueType, context);

            var isDynamic = dictionary.Type.FullName!.Equals("System.Dynamic.ExpandoObject");
            foreach (DictionaryEntry? entry in (IDictionary)dictionary.NonNullValue())
            {
                var entryValue = entry!.Value;
                var keyValue = isDynamic ? namingConvention.Apply(entryValue.Key.ToString()!) : entryValue.Key;
                var key = GetObjectDescriptor(keyValue, keyType);
                var value = GetObjectDescriptor(entryValue.Value, valueType);

                if (visitor.EnterMapping(key, value, context))
                {
                    Traverse(keyValue, key, visitor, context, path);
                    Traverse(keyValue, value, visitor, context, path);
                }
            }

            visitor.VisitMappingEnd(dictionary, context);
        }

        private void TraverseList<TContext>(IObjectDescriptor value, IObjectGraphVisitor<TContext> visitor, TContext context, Stack<ObjectPathSegment> path)
        {
            var enumerableType = ReflectionUtility.GetImplementedGenericInterface(value.Type, typeof(IEnumerable<>));
            var itemType = enumerableType != null ? enumerableType.GetGenericArguments()[0] : typeof(object);

            visitor.VisitSequenceStart(value, itemType, context);

            var index = 0;

            foreach (var item in (IEnumerable)value.NonNullValue())
            {
                Traverse(index, GetObjectDescriptor(item, itemType), visitor, context, path);
                ++index;
            }

            visitor.VisitSequenceEnd(value, context);
        }

        protected virtual void TraverseProperties<TContext>(IObjectDescriptor value, IObjectGraphVisitor<TContext> visitor, TContext context, Stack<ObjectPathSegment> path)
        {
            visitor.VisitMappingStart(value, typeof(string), typeof(object), context);

            var source = value.NonNullValue();
            foreach (var propertyDescriptor in typeDescriptor.GetProperties(value.Type, source))
            {
                var propertyValue = propertyDescriptor.Read(source);
                if (visitor.EnterMapping(propertyDescriptor, propertyValue, context))
                {
                    Traverse(propertyDescriptor.Name, new ObjectDescriptor(propertyDescriptor.Name, typeof(string), typeof(string), ScalarStyle.Plain), visitor, context, path);
                    Traverse(propertyDescriptor.Name, propertyValue, visitor, context, path);
                }
            }

            visitor.VisitMappingEnd(value, context);
        }

        private IObjectDescriptor GetObjectDescriptor(object? value, Type staticType)
        {
            return new ObjectDescriptor(value, typeResolver.Resolve(staticType, value), staticType);
        }
    }
}
