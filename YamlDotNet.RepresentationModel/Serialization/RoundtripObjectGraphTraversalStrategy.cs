using System;
using System.Globalization;
using System.Reflection;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// An implementation of <see cref="IObjectGraphTraversalStrategy"/> that traverses
	/// properties that are read/write, collections and dictionaries, while ensuring that
	/// the graph can be regenerated from the resulting document.
	/// </summary>
	public class RoundtripObjectGraphTraversalStrategy : FullObjectGraphTraversalStrategy
	{
		// TODO: Do we need this? It was present in the original implementation...
		//protected override void TraverseObject(object value, Type type, IObjectGraphVisitor visitor)
		//{
		//    if (!ReflectionUtility.HasDefaultConstructor(type))
		//    {
		//        throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Type '{0}' cannot be deserialized because it does not have a default constructor.", type));
		//    }
			
		//    base.TraverseObject(value, type, visitor);
		//}

		public RoundtripObjectGraphTraversalStrategy(int maxRecursion)
			: base(maxRecursion)
		{
		}

		protected override void SerializeProperties(object value, Type type, IObjectGraphVisitor visitor, int currentDepth)
		{
			if (!ReflectionUtility.HasDefaultConstructor(type))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Type '{0}' cannot be deserialized because it does not have a default constructor.", type));
			}

			base.SerializeProperties(value, type, visitor, currentDepth);
		}

		protected override bool IsTraversableProperty(PropertyInfo property)
		{
			return property.CanWrite && base.IsTraversableProperty(property);
		}
	}
}