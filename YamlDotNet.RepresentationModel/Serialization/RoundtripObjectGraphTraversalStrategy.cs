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
		public RoundtripObjectGraphTraversalStrategy(Serializer serializer, ITypeInspector typeDescriptor, ITypeResolver typeResolver, int maxRecursion)
			: base(serializer, typeDescriptor, typeResolver, maxRecursion)
		{
		}

		protected override void TraverseProperties(IObjectDescriptor value, IObjectGraphVisitor visitor, int currentDepth)
		{
			if (!ReflectionUtility.HasDefaultConstructor(value.Type) && !serializer.Converters.Any(c => c.Accepts(value.Type)))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Type '{0}' cannot be deserialized because it does not have a default constructor or a type converter.", value.Type));
			}

			base.TraverseProperties(value, visitor, currentDepth);
		}
	}
}