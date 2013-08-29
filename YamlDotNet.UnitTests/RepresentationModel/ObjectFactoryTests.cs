using System.IO;
using Xunit;
using YamlDotNet.RepresentationModel.Serialization;
using YamlDotNet.RepresentationModel.Serialization.ObjectFactories;

namespace YamlDotNet.UnitTests.RepresentationModel
{
	public class ObjectFactoryTests
	{
		public class FooBase
		{
		}

		public class FooDerived : FooBase
		{
		}

		[Fact]
		public void NotSpecifyingObjectFactoryUsesDefault()
		{
			var deserializer = new Deserializer();
			deserializer.RegisterTagMapping("!foo", typeof(FooBase));
			var result = deserializer.Deserialize(new StringReader("!foo {}"));

			Assert.IsType<FooBase>(result);
		}

		[Fact]
		public void ObjectFactoryIsInvoked()
		{
			var deserializer = new Deserializer(new LambdaObjectFactory(t => new FooDerived()));
			deserializer.RegisterTagMapping("!foo", typeof(FooBase));

			var result = deserializer.Deserialize(new StringReader("!foo {}"));

			Assert.IsType<FooDerived>(result);
		}
	}
}
