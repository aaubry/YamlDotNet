using System;
using System.Linq.Expressions;
using YamlDotNet.Core;
using YamlDotNet.Helpers;
using YamlDotNet.Representation;

namespace YamlDotNet.Sandbox
{
    public class ScalarSchemaNode<TValue> : SchemaNode<Scalar, TValue>
    {
        private readonly Expression<Func<Scalar, TValue>> constructor;
        private readonly Expression<Func<TValue, Scalar>> representer;

        public ScalarSchemaNode(
            TagName tag,
            Expression<Func<Scalar, TValue>> constructor,
            Expression<Func<TValue, Scalar>> representer)
        {
            Tag = tag;
            this.constructor = constructor;
            this.representer = representer;
        }

        public TagName Tag { get; }

        public override Expr<TValue> GenerateConstructor(Expr<Scalar> node, IVariableAllocator variableAllocator) => constructor.Apply(node);
        public override Expr<Scalar> GenerateRepresenter(Expr<TValue> value, IVariableAllocator variableAllocator) => representer.Apply(value);

        public override void RenderGraph(SchemaNodeRenderer renderer, string id)
        {
            renderer.WriteLine($"{id} [shape=box, label = \"{typeof(TValue).Name}\"];");
        }
    }
}
