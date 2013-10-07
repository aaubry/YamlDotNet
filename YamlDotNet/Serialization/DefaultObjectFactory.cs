using System;
using System.Collections.Generic;
using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// Creates objects using Activator.CreateInstance.
	/// </summary>
	public sealed class DefaultObjectFactory : IObjectFactory
	{
		private static readonly Dictionary<Type, Type> DefaultInterfaceImplementations = new Dictionary<Type, Type>
			{
				{typeof (IEnumerable<>), typeof (List<>)},
				{typeof (ICollection<>), typeof (List<>)},
				{typeof (IList<>), typeof (List<>)},
				{typeof (IDictionary<,>), typeof (Dictionary<,>)},
			};

		public object Create(Type type)
		{
			if (type.IsInterface)
			{
				Type implementationType;
				if (DefaultInterfaceImplementations.TryGetValue(type.GetGenericTypeDefinition(), out implementationType))
				{
					type = implementationType.MakeGenericType(type.GetGenericArguments());
				}
			}
			try
			{
				// We can't instantiate primitive or arrays
				if (PrimitiveDescriptor.IsPrimitive(type) || type.IsArray)
					return null;

				return Activator.CreateInstance(type);
			}
			catch (Exception err)
			{
				var message = string.Format("Failed to create an instance of type '{0}'", type);
				throw new InvalidOperationException(message, err);
			}
		}
	}
}