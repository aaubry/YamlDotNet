using System;
using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// Creates objects using Activator.CreateInstance.
	/// </summary>
	public sealed class DefaultObjectFactory : IObjectFactory
	{
		public object Create(Type type)
		{
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