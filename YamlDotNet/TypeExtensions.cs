using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace YamlDotNet
{
	internal static class TypeExtensions
	{

		public static bool HasInterface(this Type type, Type lookInterfaceType)
		{
			return type.GetInterface(lookInterfaceType) != null;
		}

		public static Type GetInterface(this Type type, Type lookInterfaceType)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			if (lookInterfaceType == null)
				throw new ArgumentNullException("lookInterfaceType");

			if (lookInterfaceType.IsGenericTypeDefinition)
			{
				if (lookInterfaceType.IsInterface)
					foreach (var interfaceType in type.GetInterfaces())
						if (interfaceType.IsGenericType
							&& interfaceType.GetGenericTypeDefinition() == lookInterfaceType)
							return interfaceType;

				for (Type t = type; t != null; t = t.BaseType)
					if (t.IsGenericType && t.GetGenericTypeDefinition() == lookInterfaceType)
						return t;
			}
			else
			{
				if (lookInterfaceType.IsAssignableFrom(type))
					return lookInterfaceType;
			}

			return null;
		}

		public static bool IsAnonymous(this Type type)
		{
			if (type == null)
				return false;

			return type.GetCustomAttributes(typeof (CompilerGeneratedAttribute), false).Length > 0
				&& type.Namespace == null
				&& type.FullName.Contains("AnonymousType");
		}

		/// <summary>
		/// Check if the type is a ValueType and does not contain any non ValueType members.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsPureValueType(this Type type)
		{
			if (type == null)
				return false;
			if (type == typeof(IntPtr))
				return false;
			if (type.IsPrimitive)
				return true;
			if (type.IsEnum)
				return true;
			if (!type.IsValueType)
				return false;
			// struct
			foreach (var f in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
				if (!IsPureValueType(f.FieldType))
					return false;
			return true;
		}

		/// <summary>
		/// Returnes true if the specified <paramref name="type"/> is a struct type.
		/// </summary>
		/// <param name="type"><see cref="Type"/> to be analyzed.</param>
		/// <returns>true if the specified <paramref name="type"/> is a struct type; otehrwise false.</returns>
		public static bool IsStruct(this Type type)
		{
			return type != null && type.IsValueType && !type.IsPrimitive;
		}

		/// <summary>
		/// Return if an object is a numeric value.
		/// </summary>
		/// <param name="obj">Any object to be tested.</param>
		/// <returns>True if object is a numeric value.</returns>
		public static bool IsNumeric(this Type type)
		{
			return type != null && (type == typeof(sbyte) || type == typeof(short) || type == typeof(int) || type == typeof(long) ||
				   type == typeof(byte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong) ||
				   type == typeof(float) || type == typeof(double) || type == typeof(decimal));
		}

		/// <summary>
		/// Compare two objects to see if they are equal or not. Null is acceptable.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool AreEqual(object a, object b)
		{
			if (a == null)
				return b == null;
			if (b == null)
				return false;
			return a.Equals(b) || b.Equals(a);
		}

		/// <summary>
		/// Cast an object to a specified numeric type.
		/// </summary>
		/// <param name="obj">Any object</param>
		/// <param name="type">Numric type</param>
		/// <returns>Numeric value or null if the object is not a numeric value.</returns>
		public static object CastToNumericType(this Type type, object obj)
		{
			var doubleValue = CastToDouble(obj);
			if (double.IsNaN(doubleValue))
				return null;

			if (obj is decimal && type == typeof(decimal))
				return obj; // do not convert into double

			object result = null;
			if (type == typeof(sbyte))
				result = (sbyte)doubleValue;
			if (type == typeof(byte))
				result = (byte)doubleValue;
			if (type == typeof(short))
				result = (short)doubleValue;
			if (type == typeof(ushort))
				result = (ushort)doubleValue;
			if (type == typeof(int))
				result = (int)doubleValue;
			if (type == typeof(uint))
				result = (uint)doubleValue;
			if (type == typeof(long))
				result = (long)doubleValue;
			if (type == typeof(ulong))
				result = (ulong)doubleValue;
			if (type == typeof(float))
				result = (float)doubleValue;
			if (type == typeof(double))
				result = doubleValue;
			if (type == typeof(decimal))
				result = (decimal)doubleValue;
			return result;
		}

		/// <summary>
		/// Cast boxed numeric value to double
		/// </summary>
		/// <param name="obj">boxed numeric value</param>
		/// <returns>Numeric value in double. Double.Nan if obj is not a numeric value.</returns>
		public static double CastToDouble(object obj)
		{
			var result = double.NaN;
			var type = obj != null ? obj.GetType() : null;
			if (type == typeof(sbyte))
				result = (double)(sbyte)obj;
			if (type == typeof(byte))
				result = (double)(byte)obj;
			if (type == typeof(short))
				result = (double)(short)obj;
			if (type == typeof(ushort))
				result = (double)(ushort)obj;
			if (type == typeof(int))
				result = (double)(int)obj;
			if (type == typeof(uint))
				result = (double)(uint)obj;
			if (type == typeof(long))
				result = (double)(long)obj;
			if (type == typeof(ulong))
				result = (double)(ulong)obj;
			if (type == typeof(float))
				result = (double)(float)obj;
			if (type == typeof(double))
				result = (double)obj;
			if (type == typeof(decimal))
				result = (double)(decimal)obj;
			return result;
		}
	}
}