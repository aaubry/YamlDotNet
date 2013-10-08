using System.IO;
using Xunit;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test.Serialization
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
			var settings = new SerializerSettings();
			settings.TagTypes.AddTagMapping("!foo", typeof(FooBase));
			var serializer = new Serializer(settings);
			var result = serializer.Deserialize(new StringReader("!foo {}"));

			Assert.IsType<FooBase>(result);
		}

		[Fact]
		public void ObjectFactoryIsInvoked()
		{
			var settings = new SerializerSettings()
				{
					ObjectFactory = new LambdaObjectFactory(t => new FooDerived(), new DefaultObjectFactory())
				};
			settings.TagTypes.AddTagMapping("!foo", typeof(FooBase));

			var serializer = new Serializer(settings);

			var result = serializer.Deserialize(new StringReader("!foo {}"));

			Assert.IsType<FooDerived>(result);
		}
	}
}
