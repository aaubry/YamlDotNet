using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace YamlDotNet.RepresentationModel.Serialization
{
	internal static class ReflectionUtility
	{
		/// <summary>
		/// Determines whether the specified type has a default constructor.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>
		/// 	<c>true</c> if the type has a default constructor; otherwise, <c>false</c>.
		/// </returns>
		public static bool HasDefaultConstructor(Type type)
		{
			return type.IsValueType || type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null) != null;
		}

		public static Type GetImplementedGenericInterface(Type type, Type genericInterfaceType)
		{
			foreach (var interfacetype in GetImplementedInterfaces(type))
			{
				if (interfacetype.IsGenericType && interfacetype.GetGenericTypeDefinition() == genericInterfaceType)
				{
					return interfacetype;
				}
			}
			return null;
		}

		public static IEnumerable<Type> GetImplementedInterfaces(Type type)
		{
			if (type.IsInterface)
			{
				yield return type;
			}

			foreach (var implementedInterface in type.GetInterfaces())
			{
				yield return implementedInterface;
			}
		}

		public static MethodInfo GetMethod(Expression<Action> methodAccess)
		{
			var method = ((MethodCallExpression) methodAccess.Body).Method;
			if(method.IsGenericMethod)
			{
				method = method.GetGenericMethodDefinition();
			}
			return method;
		}

		public static MethodInfo GetMethod<T>(Expression<Action<T>> methodAccess)
		{
			var method = ((MethodCallExpression)methodAccess.Body).Method;
			if (method.IsGenericMethod)
			{
				method = method.GetGenericMethodDefinition();
			}
			return method;
		}
	}
}
