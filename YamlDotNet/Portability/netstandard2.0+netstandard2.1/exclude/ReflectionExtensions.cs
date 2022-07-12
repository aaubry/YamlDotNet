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
            return type.BaseType;
        }

        public static bool IsValueType(this Type type)
        {
            return type.IsValueType;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.IsGenericType;
        }

        public static bool IsGenericTypeDefinition(this Type type)
        {
            return type.IsGenericTypeDefinition;
        }

        public static bool IsInterface(this Type type)
        {
            return type.IsInterface;
        }

        public static bool IsEnum(this Type type)
        {
            return type.IsEnum;
        }

        public static bool IsDbNull(this object value)
        {
            return value is DBNull;
        }

        /// <summary>
        /// Determines whether the specified type has a default constructor.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///     <c>true</c> if the type has a default constructor; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasDefaultConstructor(this Type type)
        {
            return type.IsValueType || type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null) != null;
        }

        public static TypeCode GetTypeCode(this Type type)
        {
            return Type.GetTypeCode(type);
        }

        public static PropertyInfo? GetPublicProperty(this Type type, string name)
        {
            return type.GetProperty(name);
        }

        public static FieldInfo? GetPublicStaticField(this Type type, string name)
        {
            return type.GetField(name, BindingFlags.Static | BindingFlags.Public);
        }

        public static IEnumerable<PropertyInfo> GetProperties(this Type type, bool includeNonPublic)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;

            if (includeNonPublic)
            {
                bindingFlags |= BindingFlags.NonPublic;
            }

            return type.IsInterface
                ? (new Type[] { type })
                    .Concat(type.GetInterfaces())
                    .SelectMany(i => i.GetProperties(bindingFlags))
                : type.GetProperties(bindingFlags);
        }

        public static IEnumerable<PropertyInfo> GetPublicProperties(this Type type) => GetProperties(type, false);

        public static IEnumerable<FieldInfo> GetPublicFields(this Type type)
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public);
        }

        public static IEnumerable<MethodInfo> GetPublicStaticMethods(this Type type)
        {
            return type.GetMethods(BindingFlags.Static | BindingFlags.Public);
        }

        public static MethodInfo GetPrivateStaticMethod(this Type type, string name)
        {
            return type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)
                ?? throw new MissingMethodException($"Expected to find a method named '{name}' in '{type.FullName}'.");
        }

        public static MethodInfo? GetPublicStaticMethod(this Type type, string name, params Type[] parameterTypes)
        {
            return type.GetMethod(name, BindingFlags.Public | BindingFlags.Static, null, parameterTypes, null);
        }

        public static MethodInfo? GetPublicInstanceMethod(this Type type, string name)
        {
            return type.GetMethod(name, BindingFlags.Public | BindingFlags.Instance);
        }

        private static readonly FieldInfo? RemoteStackTraceField = typeof(Exception)
                .GetField("_remoteStackTraceString", BindingFlags.Instance | BindingFlags.NonPublic);

        public static Exception Unwrap(this TargetInvocationException ex)
        {
            var result = ex.InnerException;
            if (result == null)
            {
                return ex;
            }

            if (RemoteStackTraceField != null)
            {
                RemoteStackTraceField.SetValue(result, result.StackTrace + "\r\n");
            }
            return result;
        }

        public static bool IsInstanceOf(this Type type, object o)
        {
            return type.IsInstanceOfType(o);
        }

        public static Attribute[] GetAllCustomAttributes<TAttribute>(this PropertyInfo property)
        {
            // Don't use IMemberInfo.GetCustomAttributes, it ignores the inherit parameter
            return Attribute.GetCustomAttributes(property, typeof(TAttribute));
        }
    }
}
