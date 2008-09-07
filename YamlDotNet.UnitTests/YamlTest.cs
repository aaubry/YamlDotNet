using System;
using System.IO;
using System.Reflection;
using System.Text;
using YamlDotNet.Core;

namespace YamlDotNet.UnitTests
{
	public class YamlTest
	{
		protected static TextReader YamlFile(string name)
		{
			Stream resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
			return new StreamReader(resource);
		}
	}
}
