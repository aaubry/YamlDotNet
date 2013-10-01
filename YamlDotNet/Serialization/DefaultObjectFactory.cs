using System;
using System.Collections.Generic;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Creates objects using Activator.CreateInstance.
	/// </summary>
	public sealed class DefaultObjectFactory : IObjectFactory
	{
		private static readonly Dictionary<Type, Type> defaultInterfaceImplementations = new Dictionary<Type, Type>
		{
			{ typeof(IEnumerable<>), typeof(List<>) },
			{ typeof(ICollection<>), typeof(List<>) },
			{ typeof(IList<>), typeof(List<>) },
			{ typeof(IDictionary<,>), typeof(Dictionary<,>) },
		};

		public object Create(Type type)
		{
			if (type.IsInterface)
			{
				Type implementationType;
				if (defaultInterfaceImplementations.TryGetValue(type.GetGenericTypeDefinition(), out implementationType))
				{
					type = implementationType.MakeGenericType(type.GetGenericArguments());
				}
			}

			try
			{
				return Activator.CreateInstance(type);
			}
			catch (MissingMethodException err)
			{
				var message = string.Format("Failed to create an instance of type '{0}'.", type);
				throw new InvalidOperationException(message, err);
			}
		}
	}
}