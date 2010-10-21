using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.UnitTests.RepresentationModel;

namespace YamlDotNet.UnitTests.Runner
{
	class Program
	{
		static void Main(string[] args)
		{
			new YamlStreamTests().RoundtripSample();
		}
	}
}
