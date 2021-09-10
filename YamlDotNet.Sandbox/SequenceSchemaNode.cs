using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using YamlDotNet.Core;
using YamlDotNet.Helpers;
using YamlDotNet.Representation;
using E = YamlDotNet.Helpers.ExpressionBuilder;

namespace YamlDotNet.Sandbox
{
    public class SequenceSchemaNode<TSequence, TItem> : SchemaNode<Sequence, TSequence>
        where TSequence : IEnumerable<TItem>
    {
        private readonly Expression<Func<IEnumerable<TItem>, Sequence, TSequence>> constructor;
        private readonly Expression<Func<TSequence, IEnumerable<TItem>>> deconstructor;
        private readonly Expression<Func<TSequence, IEnumerable<Node>, Sequence>> representer;
        private readonly ISchemaNode<Node, TItem> itemSchema;

        public SequenceSchemaNode(// TODO: Func<IScalar, bool> predicate,
            Expression<Func<IEnumerable<TItem>, Sequence, TSequence>> constructor,
            Expression<Func<TSequence, IEnumerable<TItem>>> deconstructor,
            Expression<Func<TSequence, IEnumerable<Node>, Sequence>> representer,
            ISchemaNode<Node, TItem> itemSchema
        )
        {
            this.constructor = constructor;
            this.deconstructor = deconstructor;
            this.representer = representer;
            this.itemSchema = itemSchema;
        }

        public override ISchemaNode EnterScalar(ref TagName tag, string value, ScalarStyle style) => itemSchema.EnterScalar(ref tag, value, style);
        public override ISchemaNode EnterSequence(ref TagName tag, SequenceStyle style) => itemSchema.EnterSequence(ref tag, style);
        public override ISchemaNode EnterMapping(ref TagName tag, MappingStyle style) => itemSchema.EnterMapping(ref tag, style);

        public override Expr<TSequence> GenerateConstructor(Expr<Sequence> node, IVariableAllocator variableAllocator)
        {
            Expression<Func<IEnumerable<TItem>>> itemsConstructorTemplate =
                () => E.Inject(node)
                    .Select(
                        s => E.Inject(itemSchema.GenerateConstructor(E.Wrap(s), variableAllocator))
                    );

            var itemsConstructorBody = itemsConstructorTemplate.Inject();
            return constructor.Apply(itemsConstructorBody, node);
        }

        public override Expr<Sequence> GenerateRepresenter(Expr<TSequence> value, IVariableAllocator variableAllocator)
        {
            var items = deconstructor.Apply(value);

            Expression<Func<IEnumerable<Node>>> itemsRepresenterTemplate =
                () => E.Inject(items)
                    .Select(
                        v => E.Inject(itemSchema.GenerateRepresenter(E.Wrap(v), variableAllocator))
                    );

            var itemsRepresenterBody = itemsRepresenterTemplate.Inject();
            return representer.Apply(value, itemsRepresenterBody);
        }

        public override void RenderGraph(SchemaNodeRenderer renderer, string id)
        {
            renderer
                .WriteLine($"{id} [shape=box,label = \"{typeof(TSequence).Name}\"];")
                .WriteLine($"{id} -> {renderer.GetNodeId(itemSchema)};");
        }
    }
}
