using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.RepresentationModel;

namespace YamlDotNet.UnitTests
{
	public class TracingVisitor : YamlVisitor
	{
		private int indent = 0;

		private void WriteIndent()
		{
			for (int i = 0; i < indent; ++i)
			{
				Console.Write("  ");
			}
		}

		protected override void Visit(YamlDocument document)
		{
			WriteIndent();
			Console.WriteLine("Visit(YamlDocument)");
			++indent;
		}

		protected override void Visit(YamlMappingNode mapping)
		{
			WriteIndent();
			Console.WriteLine("Visit(YamlMapping, {0}, {1})", mapping.Anchor, mapping.Tag);
			++indent;
		}

		protected override void Visit(YamlScalarNode scalar)
		{
			WriteIndent();
			Console.WriteLine("Visit(YamlScalarNode, {0}, {1}) - {2}", scalar.Anchor, scalar.Tag, scalar.Value);
			++indent;
		}

		protected override void Visit(YamlSequenceNode sequence)
		{
			WriteIndent();
			Console.WriteLine("Visit(YamlSequenceNode, {0}, {1})", sequence.Anchor, sequence.Tag);
			++indent;
		}

		protected override void Visit(YamlStream stream)
		{
			WriteIndent();
			Console.WriteLine("Visit(YamlStream)");
			++indent;
		}

		protected override void Visited(YamlDocument document)
		{
			--indent;
			WriteIndent();
			Console.WriteLine("Visited(YamlDocument)");
		}

		protected override void Visited(YamlMappingNode mapping)
		{
			--indent;
			WriteIndent();
			Console.WriteLine("Visited(YamlMappingNode)");
		}

		protected override void Visited(YamlScalarNode scalar)
		{
			--indent;
			WriteIndent();
			Console.WriteLine("Visited(YamlScalarNode)");
		}

		protected override void Visited(YamlSequenceNode sequence)
		{
			--indent;
			WriteIndent();
			Console.WriteLine("Visited(YamlSequenceNode)");
		}

		protected override void Visited(YamlStream stream)
		{
			--indent;
			WriteIndent();
			Console.WriteLine("Visited(YamlStream)");
		}
	}
}
