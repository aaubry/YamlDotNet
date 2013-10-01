using System;
using System.Globalization;
using System.Linq;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// An implementation of <see cref="IObjectGraphTraversalStrategy"/> that traverses
	/// properties that are read/write, collections and dictionaries, while ensuring that
	/// the graph can be regenerated from the resulting document.
	/// </summary>
	public class RoundtripObjectGraphTraversalStrategy : FullObjectGraphTraversalStrategy
	{
		public RoundtripObjectGraphTraversalStrategy(Serializer serializer, ITypeDescriptor typeDescriptor, int maxRecursion)
			: base(serializer, typeDescriptor, maxRecursion)
		{
		}

		protected override void SerializeProperties(object value, Type type, IObjectGraphVisitor visitor, int currentDepth)
		{
			if (!ReflectionUtility.HasDefaultConstructor(type) && !serializer.Converters.Any(c => c.Accepts(type)))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Type '{0}' cannot be deserialized because it does not have a default constructor or a type converter.", type));
			}

			base.SerializeProperties(value, type, visitor, currentDepth);
		}
	}
}