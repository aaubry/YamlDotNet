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

// Remarks: This file is imported from the SixPack library. This is ok because
// the copyright holder has agreed to redistribute this file under the license
// used in YamlDotNet.

using System;
using System.Globalization;
using System.Reflection;
using System.ComponentModel;
using YamlDotNet.Helpers;

namespace YamlDotNet.Serialization.Utilities
{
    /// <summary>
    /// Performs type conversions using every standard provided by the .NET library.
    /// </summary>
    public static partial class TypeConverter
    {
        /// <summary>
        /// Converts the specified value.
        /// </summary>
        /// <typeparam name="T">The type to which the value is to be converted.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <param name="enumNamingConvention">Naming convention to apply to enums.</param>
        /// <param name="typeInspector">The type inspector to use when getting information about a type.</param>
        /// <returns></returns>
        public static T ChangeType<T>(object? value, INamingConvention enumNamingConvention, ITypeInspector typeInspector)
        {
            return (T)ChangeType(value, typeof(T), enumNamingConvention, typeInspector)!; // This cast should always be valid
        }

        /// <summary>
        /// Converts the specified value using the invariant culture.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="destinationType">The type to which the value is to be converted.</param>
        /// <param name="enumNamingConvention">Naming convention to apply to enums.</param>
        /// <param name="typeInspector">The type inspector to use when getting information about a type.</param>
        /// <returns></returns>
        public static object? ChangeType(object? value, Type destinationType, INamingConvention enumNamingConvention, ITypeInspector typeInspector)
        {
            return ChangeType(value, destinationType, CultureInfo.InvariantCulture, enumNamingConvention, typeInspector);
        }

        /// <summary>
        /// Converts the specified value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="destinationType">The type to which the value is to be converted.</param>
        /// <param name="provider">The format provider.</param>
        /// <param name="enumNamingConvention">Naming convention to apply to enums.</param>
        /// <param name="typeInspector">The type inspector to use when getting information about a type.</param>
        /// <returns></returns>
        public static object? ChangeType(object? value, Type destinationType, IFormatProvider provider, INamingConvention enumNamingConvention, ITypeInspector typeInspector)
        {
            return ChangeType(value, destinationType, new CultureInfoAdapter(CultureInfo.CurrentCulture, provider), enumNamingConvention, typeInspector);
        }

        /// <summary>
        /// Converts the specified value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="destinationType">The type to which the value is to be converted.</param>
        /// <param name="culture">The culture.</param>
        /// <param name="enumNamingConvention">Naming convention to apply to enums.</param>
        /// <param name="typeInspector">The type inspector to use when getting information about a type.</param>
        /// <returns></returns>
        public static object? ChangeType(object? value, Type destinationType, CultureInfo culture, INamingConvention enumNamingConvention, ITypeInspector typeInspector)
        {
            // Handle null and DBNull
            if (value == null || value.IsDbNull())
            {
                return destinationType.IsValueType() ? Activator.CreateInstance(destinationType) : null;
            }

            var sourceType = value.GetType();

            // If the source type is compatible with the destination type, no conversion is needed
            if (destinationType == sourceType || destinationType.IsAssignableFrom(sourceType))
            {
                return value;
            }

            // Nullable & fsharp option types get a special treatment
            if (destinationType.IsGenericType())
            {
                var genericTypeDefinition = destinationType.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(Nullable<>) || FsharpHelper.IsOptionType(genericTypeDefinition))
                {
                    var innerType = destinationType.GetGenericArguments()[0];
                    var convertedValue = ChangeType(value, innerType, culture, enumNamingConvention, typeInspector);
                    return Activator.CreateInstance(destinationType, convertedValue);
                }
            }

            // Enums also require special handling
            if (destinationType.IsEnum())
            {
                var result = value;

                if (value is string valueText)
                {
                    valueText = enumNamingConvention.Reverse(valueText);
                    valueText = typeInspector.GetEnumName(destinationType, valueText);
                    result = Enum.Parse(destinationType, valueText, true);
                }

                return result;
            }

            // Special case for booleans to support parsing "1" and "0". This is
            // necessary for compatibility with XML Schema.
            if (destinationType == typeof(bool))
            {
                if ("0".Equals(value))
                {
                    return false;
                }

                if ("1".Equals(value))
                {
                    return true;
                }
            }

            // Try with the source type's converter
            var sourceConverter = TypeDescriptor.GetConverter(sourceType);
            if (sourceConverter != null && sourceConverter.CanConvertTo(destinationType))
            {
                return sourceConverter.ConvertTo(null, culture, value, destinationType);
            }

            // Try with the destination type's converter
            var destinationConverter = TypeDescriptor.GetConverter(destinationType);
            if (destinationConverter != null && destinationConverter.CanConvertFrom(sourceType))
            {
                return destinationConverter.ConvertFrom(null, culture, value);
            }

            // Try to find a casting operator in the source or destination type
            foreach (var type in new[] { sourceType, destinationType })
            {
                foreach (var method in type.GetPublicStaticMethods())
                {
                    var isCastingOperator =
                        method.IsSpecialName &&
                        (method.Name == "op_Implicit" || method.Name == "op_Explicit") &&
                        destinationType.IsAssignableFrom(method.ReturnParameter.ParameterType);

                    if (isCastingOperator)
                    {
                        var parameters = method.GetParameters();

                        var isCompatible =
                            parameters.Length == 1 &&
                            parameters[0].ParameterType.IsAssignableFrom(sourceType);

                        if (isCompatible)
                        {
                            try
                            {
                                return method.Invoke(null, new[] { value });
                            }
                            catch (TargetInvocationException ex)
                            {
                                throw ex.InnerException!;
                            }
                        }
                    }
                }
            }

            // If source type is string, try to find a Parse or TryParse method
            if (sourceType == typeof(string))
            {
                try
                {
                    // Try with - public static T Parse(string, IFormatProvider)
                    var parseMethod = destinationType.GetPublicStaticMethod("Parse", typeof(string), typeof(IFormatProvider));
                    if (parseMethod != null)
                    {
                        return parseMethod.Invoke(null, new object[] { value, culture });
                    }

                    // Try with - public static T Parse(string)
                    parseMethod = destinationType.GetPublicStaticMethod("Parse", typeof(string));
                    if (parseMethod != null)
                    {
                        return parseMethod.Invoke(null, new object[] { value });
                    }
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException!;
                }
            }

            // Handle TimeSpan
            if (destinationType == typeof(TimeSpan))
            {
                return TimeSpan.Parse((string)ChangeType(value, typeof(string), CultureInfo.InvariantCulture, enumNamingConvention, typeInspector)!, CultureInfo.InvariantCulture);
            }

            // Default to the Convert class
            return Convert.ChangeType(value, destinationType, CultureInfo.InvariantCulture);
        }
    }
}

#if !(NETSTANDARD1_3 || UNITY)
namespace YamlDotNet.Serialization.Utilities
{
    using System.Linq;

    partial class TypeConverter
    {
        /// <summary>
        /// Registers a <see cref="System.ComponentModel.TypeConverter"/> dynamically.
        /// </summary>
        /// <typeparam name="TConvertible">The type to which the converter should be associated.</typeparam>
        /// <typeparam name="TConverter">The type of the converter.</typeparam>
#if NET20 || NET35 || NET45
        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.LinkDemand, Name = "FullTrust")]
#endif
        public static void RegisterTypeConverter<TConvertible, TConverter>()
            where TConverter : System.ComponentModel.TypeConverter
        {
            var alreadyRegistered = TypeDescriptor.GetAttributes(typeof(TConvertible))
                .OfType<TypeConverterAttribute>()
                .Any(a => a.ConverterTypeName == typeof(TConverter).AssemblyQualifiedName);

            if (!alreadyRegistered)
            {
                TypeDescriptor.AddAttributes(typeof(TConvertible), new TypeConverterAttribute(typeof(TConverter)));
            }
        }

    }
}
#endif
