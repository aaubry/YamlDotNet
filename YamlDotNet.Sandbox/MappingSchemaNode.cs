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
    public class MappingSchemaNode<TMapping, TKey, TValue> : SchemaNode<Mapping, TMapping>
        where TMapping : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Expression<Func<IEnumerable<KeyValuePair<TKey, TValue>>, Mapping, TMapping>> constructor;
        private readonly Expression<Func<TMapping, IEnumerable<KeyValuePair<TKey, TValue>>>> deconstructor;
        private readonly Expression<Func<TMapping, IEnumerable<KeyValuePair<Node, Node>>, Mapping>> representer;
        private readonly ISchemaNode<Node, TKey> keySchema;
        private readonly ISchemaNode<Node, TValue> valueSchema;

        public MappingSchemaNode(
            Expression<Func<IEnumerable<KeyValuePair<TKey, TValue>>, Mapping, TMapping>> constructor,
            Expression<Func<TMapping, IEnumerable<KeyValuePair<TKey, TValue>>>> deconstructor,
            Expression<Func<TMapping, IEnumerable<KeyValuePair<Node, Node>>, Mapping>> representer,
            ISchemaNode<Node, TKey> keySchema,
            ISchemaNode<Node, TValue> valueSchema
        )
        {
            this.constructor = constructor;
            this.deconstructor = deconstructor;
            this.representer = representer;
            this.keySchema = keySchema;
            this.valueSchema = valueSchema;
        }

        public override ISchemaNode EnterScalar(ref TagName tag, string value, ScalarStyle style) => keySchema;
        public override ISchemaNode EnterSequence(ref TagName tag, SequenceStyle style) => keySchema;
        public override ISchemaNode EnterMapping(ref TagName tag, MappingStyle style) => keySchema;

        public override ISchemaNode EnterMappingKey(Node key, ISchemaNode schemaNode) => valueSchema;

        public override Expr<TMapping> GenerateConstructor(Expr<Mapping> node, IVariableAllocator variableAllocator)
        {
            Expression<Func<IEnumerable<KeyValuePair<TKey, TValue>>>> itemsConstructorTemplate =
                () => E.Inject(node)
                    .Select(
                        p => new KeyValuePair<TKey, TValue>(
                            E.Inject(keySchema.GenerateConstructor(E.Wrap(p.Key), variableAllocator)),
                            E.Inject(valueSchema.GenerateConstructor(E.Wrap(p.Value), variableAllocator))
                        )
                    );

            var itemsConstructorBody = itemsConstructorTemplate.Inject();
            return constructor.Apply(itemsConstructorBody, node);
        }

        public override Expr<Mapping> GenerateRepresenter(Expr<TMapping> value, IVariableAllocator variableAllocator)
        {
            var pairs = deconstructor.Apply(value);

            Expression<Func<IEnumerable<KeyValuePair<Node, Node>>>> pairsRepresenterTemplate =
                () => E.Inject(pairs)
                    .Select(
                        p => new KeyValuePair<Node, Node>(
                            E.Inject(keySchema.GenerateRepresenter(E.Wrap(p.Key), variableAllocator)),
                            E.Inject(valueSchema.GenerateRepresenter(E.Wrap(p.Value), variableAllocator))
                        )
                    );

            var pairsRepresenterBody = pairsRepresenterTemplate.Inject();
            return representer.Apply(value, pairsRepresenterBody);
        }

        public override void RenderGraph(SchemaNodeRenderer renderer, string id)
        {
            renderer
                .WriteLine($"{id} [label = \"{typeof(TMapping).Name}\"];")
                .WriteLine($"{id} -> {renderer.GetNodeId(keySchema)};")
                .WriteLine($"{id} -> {renderer.GetNodeId(valueSchema)};");
        }
    }
}
