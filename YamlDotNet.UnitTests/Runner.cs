using System;
using YamlDotNet.Core;
using System.IO;

namespace YamlDotNet.UnitTests
{
	public class Runner
	{
		public static void Main()
		{
			/*
			YamlStreamTests test = new YamlStreamTests();
			test.BackwardAliasReferenceWorks();
			test.ForwardAliasReferenceWorks();
			test.LoadSimpleDocument();
			*/
			/*
			XmlConverterTests test = new XmlConverterTests();
			test.ScalarToXml();
			Console.WriteLine();
			Console.WriteLine("---");
			test.SequenceOfScalarsToXml();
			Console.WriteLine();
			Console.WriteLine("---");
			test.MappingOfScalarsToXml();
			Console.WriteLine();
			Console.WriteLine("---");
			test.SequenceOfMappingAndSequencesToXml();
			Console.WriteLine();
			Console.WriteLine("---");
			*/

			YamlDotNet.UnitTests.RepresentationModel.YamlStreamTests tests = new YamlDotNet.UnitTests.RepresentationModel.YamlStreamTests();
			tests.Test2();
		}
	}
}