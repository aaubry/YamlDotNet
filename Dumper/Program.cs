using System;
using YamlDotNet.Core;
using System.IO;
using YamlDotNet.Core.Events;

namespace Dumper
{
	class Program
	{
		static void Main(string[] args)
		{
			using (TextReader input = File.OpenText(args[0]))
			{
				int indent = 0;
				Parser parser = new Parser(input);
				while(parser.MoveNext())
				{
					if (parser.Current is StreamEnd || parser.Current is DocumentEnd || parser.Current is SequenceEnd || parser.Current is SequenceEnd || parser.Current is MappingEnd)
					{
						--indent;
					}
					for(int i = 0; i < indent; ++i)
					{
						Console.Write("  ");
					}

					Console.WriteLine(parser.Current.ToString());

					if (parser.Current is StreamStart || parser.Current is DocumentStart || parser.Current is SequenceStart || parser.Current is MappingStart)
					{
						++indent;
					}
				}
			}
		}
	}
}