using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using YamlDotNet.RepresentationModel.Serialization;

namespace YamlDotNet.UnitTests.RepresentationModel
{
	[TestClass]
	public class ObjectConverterTests
	{
		[TestMethod]
		public void StringToColor()
		{
			Color color = ObjectConverter.Convert<string, Color>("white");
			Assert.AreEqual(unchecked((int)0xFFFFFFFF), color.ToArgb());
		}
	}
}