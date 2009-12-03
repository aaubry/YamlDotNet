using System;
using NUnit.Framework;
using System.Drawing;
using YamlDotNet.RepresentationModel.Serialization;

namespace YamlDotNet.UnitTests.RepresentationModel
{
	[TestFixture]
	public class ObjectConverterTests
	{
		[Test]
		public void StringToColor()
		{
			Color color = ObjectConverter.Convert<string, Color>("white");
			Assert.AreEqual(unchecked((int)0xFFFFFFFF), color.ToArgb());
		}
	}
}