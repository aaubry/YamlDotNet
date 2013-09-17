using System.Diagnostics;

namespace YamlDotNet.Core.Test
{
	public class Dump
	{
		[Conditional("TEST_DUMP")]
		public static void Write(object value)
		{
			Debug.Write(value);
		}

		[Conditional("TEST_DUMP")]
		public static void Write(string format, params object[] args)
		{
			Debug.Write(string.Format(format, args));
		}

		[Conditional("TEST_DUMP")]
		public static void WriteLine()
		{
			Debug.WriteLine(string.Empty);
		}

		[Conditional("TEST_DUMP")]
		public static void WriteLine(string value)
		{
			WriteLine((object)value);
		}

		[Conditional("TEST_DUMP")]
		public static void WriteLine(object value)
		{
			WriteLine("{0}", value);
		}

		[Conditional("TEST_DUMP")]
		public static void WriteLine(string format, params object[] args)
		{
			Debug.WriteLine(format, args);
		}
	}
}