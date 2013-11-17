using System;
using System.IO;

namespace YamlDotNet.PerformanceTests.Lib
{
	public interface ISerializerAdapter
	{
		void Serialize (TextWriter writer, object graph);
	}
}