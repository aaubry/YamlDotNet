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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace YamlDotNet
{
    internal static class ReflectionExtensions
    {
        public static Type? BaseType(this Type type)
        {
            return type.GetTypeInfo().BaseType;
        }

        public static bool IsValueType(this Type type)
        {
            return type.GetTypeInfo().IsValueType;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }

        public static bool IsGenericTypeDefinition(this Type type)
        {
            return type.GetTypeInfo().IsGenericTypeDefinition;
        }

        public static Type? GetImplementationOfOpenGenericInterface(this Type type, Type openGenericType)
        {
            if (!openGenericType.IsGenericType || !openGenericType.IsInterface)
            {
                // Note we can likely relax this constraint to also allow for matching other types
                throw new ArgumentException("The type must be a generic type definition and an interface", nameof(openGenericType));
            }

            // First check if the type itself is the open generic type
            if (IsGenericDefinitionOfType(type, openGenericType))
            {
                return type;
            }

            // Then check the interfaces
            return type.FindInterfaces(static (t, context) => IsGenericDefinitionOfType(t, context), openGenericType).FirstOrDefault();

            static bool IsGenericDefinitionOfType(Type t, object? context)
            {
                return t.IsGenericType && context is Type type && t.GetGenericTypeDefinition() == type;
            }
        }

        public static bool IsInterface(this Type type)
        {
            return type.GetTypeInfo().IsInterface;
        }

        public static bool IsEnum(this Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }

        public static bool IsRequired(this MemberInfo member)
        {
#if NET8_0_OR_GREATER
            var result = member.GetCustomAttributes<System.Runtime.CompilerServices.RequiredMemberAttribute>().Any();
#else
            var result = member.GetCustomAttributes(true).Any(x => x.GetType().FullName == "System.Runtime.CompilerServices.RequiredMemberAttribute");
#endif
            return result;
        }

        /// <summary>
        /// Determines whether the specified type has a default constructor.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="allowPrivateConstructors">Whether to include private constructors</param>
        /// <returns>
        ///     <c>true</c> if the type has a default constructor; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasDefaultConstructor(this Type type, bool allowPrivateConstructors)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;

            if (allowPrivateConstructors)
            {
                bindingFlags |= BindingFlags.NonPublic;
            }

            return type.IsValueType || type.GetConstructor(bindingFlags, null, Type.EmptyTypes, null) != null;
        }

        public static bool IsAssignableFrom(this Type type, Type source)
        {
            return type.IsAssignableFrom(source.GetTypeInfo());
        }

        public static bool IsAssignableFrom(this Type type, TypeInfo source)
        {
            return type.GetTypeInfo().IsAssignableFrom(source);
        }

        public static TypeCode GetTypeCode(this Type type)
        {
            var isEnum = type.IsEnum();
            if (isEnum)
            {
                type = Enum.GetUnderlyingType(type);
            }

            if (type == typeof(bool))
            {
                return TypeCode.Boolean;
            }
            else if (type == typeof(char))
            {
                return TypeCode.Char;
            }
            else if (type == typeof(sbyte))
            {
                return TypeCode.SByte;
            }
            else if (type == typeof(byte))
            {
                return TypeCode.Byte;
            }
            else if (type == typeof(short))
            {
                return TypeCode.Int16;
            }
            else if (type == typeof(ushort))
            {
                return TypeCode.UInt16;
            }
            else if (type == typeof(int))
            {
                return TypeCode.Int32;
            }
            else if (type == typeof(uint))
            {
                return TypeCode.UInt32;
            }
            else if (type == typeof(long))
            {
                return TypeCode.Int64;
            }
            else if (type == typeof(ulong))
            {
                return TypeCode.UInt64;
            }
            else if (type == typeof(float))
            {
                return TypeCode.Single;
            }
            else if (type == typeof(double))
            {
                return TypeCode.Double;
            }
            else if (type == typeof(decimal))
            {
                return TypeCode.Decimal;
            }
            else if (type == typeof(DateTime))
            {
                return TypeCode.DateTime;
            }
            else if (type == typeof(string))
            {
                return TypeCode.String;
            }
            else
            {
                return TypeCode.Object;
            }
        }

        public static bool IsDbNull(this object value)
        {
            return value?.GetType()?.FullName == "System.DBNull";
        }

        public static Type[] GetGenericArguments(this Type type)
        {
            return type.GetTypeInfo().GenericTypeArguments;
        }

        public static PropertyInfo? GetPublicProperty(this Type type, string name)
        {
            // In the case of property hiding, get the most-derived implementation.
            return type
                .GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
                .FirstOrDefault(p => p.Name == name);
        }

        public static FieldInfo? GetPublicStaticField(this Type type, string name)
        {
            return type.GetRuntimeField(name);
        }


        private static readonly Func<PropertyInfo, bool> IsInstance = (PropertyInfo property) => !(property.GetMethod ?? property.SetMethod).IsStatic;
        private static readonly Func<PropertyInfo, bool> IsInstancePublic = (PropertyInfo property) => IsInstance(property) && (property.GetMethod ?? property.SetMethod).IsPublic;

        public static IEnumerable<PropertyInfo> GetProperties(this Type type, bool includeNonPublic)
        {
            var predicate = includeNonPublic ? IsInstance : IsInstancePublic;

            return type.IsInterface()
                ? (new Type[] { type })
                    .Concat(type.GetInterfaces())
                    .SelectMany(i => i.GetRuntimeProperties().Where(predicate))
                : type.GetRuntimeProperties().Where(predicate);
        }

        public static IEnumerable<PropertyInfo> GetPublicProperties(this Type type) => GetProperties(type, false);

        public static IEnumerable<FieldInfo> GetPublicFields(this Type type)
        {
            return type.GetRuntimeFields().Where(f => !f.IsStatic && f.IsPublic);
        }

        public static IEnumerable<MethodInfo> GetPublicStaticMethods(this Type type)
        {
            return type.GetRuntimeMethods()
                .Where(m => m.IsPublic && m.IsStatic);
        }

        public static MethodInfo GetPrivateStaticMethod(this Type type, string name)
        {
            return type.GetRuntimeMethods()
                .FirstOrDefault(m => !m.IsPublic && m.IsStatic && m.Name.Equals(name))
                ?? throw new MissingMethodException($"Expected to find a method named '{name}' in '{type.FullName}'.");
        }

        public static MethodInfo? GetPublicStaticMethod(this Type type, string name, params Type[] parameterTypes)
        {
            return type.GetRuntimeMethods()
                .FirstOrDefault(m =>
                {
                    if (m.IsPublic && m.IsStatic && m.Name.Equals(name))
                    {
                        var parameters = m.GetParameters();
                        return parameters.Length == parameterTypes.Length
                            && parameters.Zip(parameterTypes, (pi, pt) => pi.ParameterType == pt).All(r => r);
                    }
                    return false;
                });
        }

        public static MethodInfo? GetPublicInstanceMethod(this Type type, string name)
        {
            return type.GetRuntimeMethods()
                .FirstOrDefault(m => m.IsPublic && !m.IsStatic && m.Name.Equals(name));
        }

        public static MethodInfo? GetGetMethod(this PropertyInfo property, bool nonPublic)
        {
            var getter = property.GetMethod;
            if (!nonPublic && !getter.IsPublic)
            {
                getter = null;
            }
            return getter;
        }

        public static MethodInfo? GetSetMethod(this PropertyInfo property)
        {
            return property.SetMethod;
        }

        public static IEnumerable<Type> GetInterfaces(this Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces;
        }

        public static bool IsInstanceOf(this Type type, object o)
        {
            return o.GetType() == type || o.GetType().GetTypeInfo().IsSubclassOf(type);
        }

        public static Attribute[] GetAllCustomAttributes<TAttribute>(this PropertyInfo member)
        {
            return Attribute.GetCustomAttributes(member, typeof(TAttribute), inherit: true);
        }

        private static readonly ConcurrentDictionary<Type, bool> TypesHaveNullContext = new();

        public static bool AcceptsNull(this MemberInfo member)
        {
            var result = true; //default to allowing nulls, this will be set to false if there is a null context on the type
#if NET8_0_OR_GREATER
            var typeHasNullContext = TypesHaveNullContext.GetOrAdd(member.DeclaringType, (Type t) =>
            {
                var attributes = t.GetCustomAttributes(typeof(System.Runtime.CompilerServices.NullableContextAttribute), true);
                return (attributes?.Length ?? 0) > 0;
            });

            if (typeHasNullContext)
            {
                // we have a nullable context on that type, only allow null if the NullableAttribute is on the member.
                var memberAttributes = member.GetCustomAttributes(typeof(System.Runtime.CompilerServices.NullableAttribute), true);
                result = (memberAttributes?.Length ?? 0) > 0;
            }

            return result;
#else
            var typeHasNullContext = TypesHaveNullContext.GetOrAdd(member.DeclaringType, (Type t) =>
            {
                var attributes = t.GetCustomAttributes(true);
                return attributes.Any(x => x.GetType().FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
            });

            if (typeHasNullContext)
            {
                var memberAttributes = member.GetCustomAttributes(true);
                result = memberAttributes.Any(x => x.GetType().FullName == "System.Runtime.CompilerServices.NullableAttribute");
            }

            return result;
#endif
        }
    }
}
