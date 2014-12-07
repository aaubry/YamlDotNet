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
using System.Reflection;
using System.Linq;

namespace YamlDotNet.Serialization.Utilities
{
	internal static class ReflectionUtility
	{
		public static Type GetImplementedGenericInterface(Type type, Type genericInterfaceType)
		{
			foreach (var interfacetype in GetImplementedInterfaces(type))
			{
				if (interfacetype.IsGenericType() && interfacetype.GetGenericTypeDefinition() == genericInterfaceType)
				{
					return interfacetype;
				}
			}
			return null;
		}

		public static IEnumerable<Type> GetImplementedInterfaces(Type type)
		{
			if (type.IsInterface())
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
			var method = ((MethodCallExpression)methodAccess.Body).Method;
			if (method.IsGenericMethod)
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

	public sealed class GenericStaticMethod
	{
		private readonly MethodInfo methodToCall;

		public GenericStaticMethod(Expression<Action> methodCall)
		{
			var callExpression = (MethodCallExpression)methodCall.Body;
			methodToCall = callExpression.Method.GetGenericMethodDefinition();
		}

		public object Invoke(Type[] genericArguments, params object[] arguments)
		{
			try
			{
				return methodToCall
					.MakeGenericMethod(genericArguments)
					.Invoke(null, arguments);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.Unwrap();
			}
		}
	}

	public sealed class GenericInstanceMethod<TInstance>
	{
		private readonly MethodInfo methodToCall;

		public GenericInstanceMethod(Expression<Action<TInstance>> methodCall)
		{
			var callExpression = (MethodCallExpression)methodCall.Body;
			methodToCall = callExpression.Method.GetGenericMethodDefinition();
		}

		public object Invoke(Type[] genericArguments, TInstance instance, params  object[] arguments)
		{
			try
			{
				return methodToCall
					.MakeGenericMethod(genericArguments)
					.Invoke(instance, arguments);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.Unwrap();
			}
		}
	}
}
