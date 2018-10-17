using System.Collections.Generic;

namespace YamlDotNet.Serialization
{
    /// <summary>
    /// A factory method for creating <see cref="IObjectGraphTraversalStrategy"/> instances
    /// </summary>
    /// <param name="typeInspector">The type inspector to be used by the traversal strategy.</param>
    /// <param name="typeResolver">The type resolver to be used by the traversal strategy.</param>
    /// <param name="typeConverters">The type converters to be used by the traversal strategy.</param>
    /// <param name="maximumRecursion">The maximum object depth to be supported by the traversal strategy.</param>
    /// <returns></returns>
    public delegate IObjectGraphTraversalStrategy
        ObjectGraphTraversalStrategyFactory(
            ITypeInspector typeInspector,
            ITypeResolver typeResolver,
            IEnumerable<IYamlTypeConverter> typeConverters,
            int maximumRecursion
        );
}
