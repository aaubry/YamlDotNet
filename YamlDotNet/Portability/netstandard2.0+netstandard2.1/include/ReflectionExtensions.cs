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

        public static bool IsInterface(this Type type)
        {
            return type.GetTypeInfo().IsInterface;
        }

        public static bool IsEnum(this Type type)
        {
            return type.GetTypeInfo().IsEnum;
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
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsValueType || typeInfo.DeclaredConstructors
                .Any(c => (c.IsPublic || (allowPrivateConstructors && c.IsPrivate)) && !c.IsStatic && c.GetParameters().Length == 0);
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
            return type.GetRuntimeProperty(name);
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

        public static MethodInfo GetSetMethod(this PropertyInfo property)
        {
            return property.SetMethod;
        }

        public static IEnumerable<Type> GetInterfaces(this Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces;
        }

        public static Exception Unwrap(this TargetInvocationException ex)
        {
            return ex.InnerException;
        }

        public static bool IsInstanceOf(this Type type, object o)
        {
            return o.GetType() == type || o.GetType().GetTypeInfo().IsSubclassOf(type);
        }

        public static Attribute[] GetAllCustomAttributes<TAttribute>(this PropertyInfo member)
        {
            // IMemberInfo.GetCustomAttributes ignores it's "inherit" parameter for properties,
            // and the suggested replacement (Attribute.GetCustomAttributes) is not available
            // on netstandard1.3
            var result = new List<Attribute>();
            var type = member.DeclaringType;

            while (type != null)
            {
                type.GetPublicProperty(member.Name);
                result.AddRange(member.GetCustomAttributes(typeof(TAttribute)));

                type = type.BaseType();
            }

            return result.ToArray();
        }
    }
}
