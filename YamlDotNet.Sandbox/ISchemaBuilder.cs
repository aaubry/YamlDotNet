using YamlDotNet.Representation;

namespace YamlDotNet.Sandbox
{
    public interface ISchemaBuilder
    {
        ISchemaNode<Node, TValue> BuildSchema<TValue>();
    }
}
