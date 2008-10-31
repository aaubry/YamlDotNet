using System;
using System.IO;
using System.Reflection;

namespace YamlDotNet.UnitTests
{
	public class YamlTest
	{
		protected static TextReader YamlFile(string name)
		{
			Stream resource =
				Assembly.GetExecutingAssembly().GetManifestResourceStream(name) ??
				Assembly.GetExecutingAssembly().GetManifestResourceStream("YamlDotNet.UnitTests." + name);

			return new StreamReader(resource);
		}
	}
}
