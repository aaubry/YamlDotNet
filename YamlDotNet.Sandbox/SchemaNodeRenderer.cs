using System.Collections.Generic;
using System.Text;
using YamlDotNet.Representation;

namespace YamlDotNet.Sandbox
{
    public class SchemaNodeRenderer
    {
        private readonly Dictionary<object, string> renderedNodes = new();
        private readonly StringBuilder renderedGraph = new();

        private SchemaNodeRenderer() { }

        public string GetNodeId(ISchemaNode node)
        {
            if (!renderedNodes.TryGetValue(node.Identity, out var id))
            {
                id = $"N{renderedNodes.Count}";
                renderedNodes.Add(node.Identity, id);

                node.RenderGraph(this, id);
                id = renderedNodes[node.Identity];
            }
            return id;
        }

        public SchemaNodeRenderer WriteLine(string text)
        {
            renderedGraph.AppendLine(text);
            return this;
        }

        public static string Render(ISchemaNode node)
        {
            var renderer = new SchemaNodeRenderer();
            renderer
                .WriteLine("digraph G {")
                .WriteLine("graph [compound=true];")
                .WriteLine("R [shape=point];")
                .WriteLine($"R -> {renderer.GetNodeId(node)};")
                .WriteLine("}");

            return renderer.renderedGraph.ToString();
        }
    }
}
