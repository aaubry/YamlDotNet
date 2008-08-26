using System;
using System.IO;
using System.Text;
using YamlDotNet.Core;

namespace YamlDotNet.UnitTests
{
	public class YamlTest
	{
		protected static TextReader YamlFile(string content)
		{
			string[] lines = content.Split('\n');
			StringBuilder buffer = new StringBuilder();
			int indent = -1;
			for (int i = 1; i < lines.Length - 1; ++i)
			{
				if (indent < 0)
				{
					indent = 0;
					while (lines[i][indent] == '\t')
					{
						++indent;
					}
				}
				else
				{
					buffer.Append('\n');
				}
				buffer.Append(lines[i].Substring(indent));
			}
			return new StringReader(buffer.ToString());
		}
	}
}
