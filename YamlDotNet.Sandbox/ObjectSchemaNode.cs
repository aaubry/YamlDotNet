using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Helpers;
using YamlDotNet.Representation;

namespace YamlDotNet.Sandbox
{
    public class ObjectSchemaNode<TValue> : SchemaNode<Mapping, TValue>
    {
        private readonly ISchemaNode<Node, string> stringSchemaNode;
        private readonly Dictionary<string, (PropertyInfo property, ISchemaNode schemaNode)> properties;

        public ObjectSchemaNode(ISchemaBuilder schemaBuidler)
        {
            this.stringSchemaNode = schemaBuidler.BuildSchema<string>();

            this.properties = typeof(TValue)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(
                    p => p.Name,
                    p => (p, (ISchemaNode)new PropertySchemaNode(stringSchemaNode, schemaBuidler.BuildSchema(p.PropertyType)))
                    //p => (p, schemaBuidler.BuildSchema(p.PropertyType))
                );
        }

        private sealed class PropertySchemaNode : ISchemaNode
        {
            private readonly ISchemaNode<Node, string> keySchema;

            public PropertySchemaNode(ISchemaNode<Node, string> keySchema, ISchemaNode valueSchema)
            {
                this.keySchema = keySchema;
                ValueSchema = valueSchema;
            }

            public ISchemaNode ValueSchema { get; }

            public object Identity => ValueSchema;

            public ISchemaNode EnterScalar(ref TagName tag, string value, ScalarStyle style) => keySchema.EnterScalar(ref tag, value, style);
            public ISchemaNode EnterMapping(ref TagName tag, MappingStyle style) => keySchema.EnterMapping(ref tag, style);
            public ISchemaNode EnterSequence(ref TagName tag, SequenceStyle style) => keySchema.EnterSequence(ref tag, style);

            public ISchemaNode EnterMappingKey(Node key, ISchemaNode schemaNode) => ValueSchema;

            public Expression GenerateConstructor(Expression node, IVariableAllocator variableAllocator) => keySchema.GenerateConstructor(node, variableAllocator);
            public Expression GenerateRepresenter(Expression value, IVariableAllocator variableAllocator) => keySchema.GenerateRepresenter(value, variableAllocator);
            public void RenderGraph(SchemaNodeRenderer renderer, string id) => ValueSchema.RenderGraph(renderer, id);
        }

        public override ISchemaNode EnterScalar(ref TagName tag, string value, ScalarStyle style)
        {
            // TODO: Naming convention
            // TODO: Unknown key handling
            var property = properties[value];

            return property.schemaNode;
        }

        //public override ISchemaNode EnterMappingKey(Node key, ISchemaNode schemaNode)
        //{
        //    return ((PropertySchemaNode)schemaNode).ValueSchema;
        //}

        private static readonly PropertyInfo mappingIndexer = typeof(Mapping)
            .GetProperty("Item", typeof(Node), new[] { typeof(string) })
            ?? throw new InvalidOperationException($"Indexer not found in class '{nameof(Mapping)}'.");

        private static readonly MethodInfo mappingGetEnumerator = typeof(IEnumerable<KeyValuePair<Node, Node>>).GetMethod(nameof(IEnumerable<KeyValuePair<Node, Node>>.GetEnumerator))
            ?? throw new InvalidOperationException($"Method '{nameof(IEnumerable<KeyValuePair<Node, Node>>.GetEnumerator)}' not found in class '{nameof(IEnumerable<KeyValuePair<Node, Node>>)}'.");

        private static readonly MethodInfo enumeratorMoveNext = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext))
            ?? throw new InvalidOperationException($"Method '{nameof(IEnumerator.MoveNext)}' not found in class '{nameof(IEnumerator)}'.");

        private static readonly PropertyInfo mappingEnumeratorCurrent = typeof(IEnumerator<KeyValuePair<Node, Node>>).GetProperty(nameof(IEnumerator<KeyValuePair<Node, Node>>.Current))
            ?? throw new InvalidOperationException($"Property '{nameof(IEnumerator<KeyValuePair<Node, Node>>.Current)}' not found in class '{nameof(IEnumerator<KeyValuePair<Node, Node>>)}'.");

        private static readonly PropertyInfo keyValuePairKey = typeof(KeyValuePair<Node, Node>).GetProperty(nameof(KeyValuePair<Node, Node>.Key))
            ?? throw new InvalidOperationException($"Property '{nameof(KeyValuePair<Node, Node>.Key)}' not found in class '{nameof(KeyValuePair<Node, Node>)}'.");

        private static readonly PropertyInfo keyValuePairValue = typeof(KeyValuePair<Node, Node>).GetProperty(nameof(KeyValuePair<Node, Node>.Value))
            ?? throw new InvalidOperationException($"Property '{nameof(KeyValuePair<Node, Node>.Value)}' not found in class '{nameof(KeyValuePair<Node, Node>)}'.");

        private static readonly MethodInfo stringComparison = typeof(string).GetMethod(nameof(string.Equals), new[] { typeof(string), typeof(string) })
            ?? throw new InvalidOperationException($"Method '{nameof(string.Equals)}(string, string)' not found in class '{nameof(String)}'.");

        public override Expr<TValue> GenerateConstructor(Expr<Mapping> node, IVariableAllocator variableAllocator)
        {
            var enumerator = Expression.Variable(typeof(IEnumerator<KeyValuePair<Node, Node>>), "en");
            var result = Expression.Variable(typeof(TValue), "result");
            var loopBreak = Expression.Label();
            var currentVariable = Expression.Variable(typeof(KeyValuePair<Node, Node>), "current");
            var keyVariable = Expression.Variable(typeof(string), "key");
            var valueVariable = Expression.Variable(typeof(Node), "value");

            var constructor = Expression.Block(
                typeof(TValue),
                new[] { enumerator, result },
                Expression.Assign(
                    result,
                    Expression.New(typeof(TValue)) // TODO: Select constructor
                ),
                Expression.Assign(
                    enumerator,
                    Expression.Call(node, mappingGetEnumerator)
                ),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.Call(enumerator, enumeratorMoveNext),
                        Expression.Block(
                            new[] { currentVariable, keyVariable, valueVariable },
                            //Debug<IEnumerator<KeyValuePair<Node, Node>>>(n => Console.WriteLine(n), enumerator),
                            Expression.Assign(
                                currentVariable,
                                Expression.Property(enumerator, mappingEnumeratorCurrent)
                            ),
                            Expression.Assign(
                                keyVariable,
                                stringSchemaNode.GenerateConstructor(
                                    Expression.Property(currentVariable, keyValuePairKey).As<Node>(),
                                    variableAllocator
                                )
                            ),
                            Expression.Assign(
                                valueVariable,
                                Expression.Property(currentVariable, keyValuePairValue)
                            ),
                            Expression.Switch(
                                typeof(void),
                                keyVariable,
                                Expression.Throw(Expression.New(typeof(KeyNotFoundException))), // TODO: Throw a good exception
                                stringComparison,
                                properties.Select(p => Expression.SwitchCase(
                                     Expression.Assign(
                                         Expression.Property(result, p.Value.property),
                                         p.Value.schemaNode.GenerateConstructor(valueVariable, variableAllocator)
                                     ),
                                     Expression.Constant(p.Key) // TODO: Naming convention
                                 ))
                            )
                        ),
                        Expression.Break(loopBreak)
                    ),
                    loopBreak
                ),
                result
            );

            return constructor.As<TValue>();
        }

        public override Expr<Mapping> GenerateRepresenter(Expr<TValue> value, IVariableAllocator variableAllocator)
        {
            throw new NotImplementedException();
        }

        private Expression Debug<T>(Action<T> callback, Expression value)
        {
            return Expression.Invoke(Expression.Constant(callback), value);
        }

        public override void RenderGraph(SchemaNodeRenderer renderer, string id)
        {
            var renderedProperties = properties
                .Select((p, i) => $"  <tr><td port='K{i}'>k</td><td>{p.Key}</td><td port='V{i}'>v</td></tr>");

            renderer
                .WriteLine($"{id} [shape=plaintext,label=<<table border='0' cellborder='1' cellspacing='0'>")
                .WriteLine($"  <tr><td colspan='3'><b>{typeof(TValue).Name}</b></td></tr>")
                .WriteLine(string.Join('\n', renderedProperties))
                .WriteLine("</table>>];")
                .WriteLine($"{id}_KEYS [shape=point,style=invis,width=0.0];")
                .WriteLine($"{id}_KEYS -> {renderer.GetNodeId(stringSchemaNode)};");

            for (int i = 0; i < properties.Count; ++i)
            {
                renderer.WriteLine($"{id}:K{i} -> {id}_KEYS [arrowhead=none];");
            }

            var valueReferences = properties
                .Select((p, i) => (from: i, to: renderer.GetNodeId(p.Value.schemaNode)))
                .GroupBy(p => p.to, (to, g) => (from: g.Select(p => p.from).ToList(), to));

            foreach (var referenceGroup in valueReferences)
            {
                if (referenceGroup.from.Count == 1)
                {
                    renderer.WriteLine($"{id}:V{referenceGroup.from[0]} -> {referenceGroup.to};");
                }
                else
                {
                    var groupId = $"{id}_V_{referenceGroup.to}";
                    renderer
                        .WriteLine($"{groupId} [shape=point,style=invis,width=0.0];")
                        .WriteLine($"{groupId} -> {referenceGroup.to};");

                    foreach (var from in referenceGroup.from)
                    {
                        renderer.WriteLine($"{id}:V{from}-> {groupId} [arrowhead=none];");
                    }
                }
            }
        }
    }
}
