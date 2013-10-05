using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Xunit;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test
{
	public class DescriptorTests
	{

		public class TestObject
		{
			// unused, not public
			private string name;
			internal string InternalName { get; set; }

			public TestObject()
			{
				Collection = new List<string>();
				CollectionReadOnly = new ReadOnlyCollection<string>(new List<string>());
				DefaultValue = 5;
			}

			public object Value { get; set; }

			public string Name;

			public string Property { get; set; }

			public ICollection<string> Collection { get; set; }

			public ICollection<string> CollectionReadOnly { get; private set; }

			[YamlIgnore]
			public string DontSerialize { get; set; }

			[YamlMember("Item1")]
			public string ItemRenamed1 { get; set; }

			// This property is renamed to Item2 by an external attribute
			public int ItemRenamed2 { get; set; }

			[DefaultValue(5)]
			public int DefaultValue { get; set; }

			public bool ShouldSerializeValue()
			{
				return Value != null;
			}
		}

		[Fact]
		public void TestObjectDescriptor()
		{
			var settings = new YamlSerializerSettings();

			// Rename ItemRenamed2 to Item2
			settings.AttributeRegistry.Register(typeof(TestObject).GetProperty("ItemRenamed2"), new YamlMemberAttribute("Item2"));

			var descriptorFactory = settings.TypeDescriptorFactory;
			var descriptor = descriptorFactory.Find(typeof(TestObject));

			// Verify members
			Assert.Equal(descriptor.Count, 8);

			// Check names and their orders
			Assert.Equal(descriptor.Members.Select(memberDescriptor => memberDescriptor.Name), new []
				{
					"Collection",
					"CollectionReadOnly",
					"DefaultValue",
					"Item1",
					"Item2",
					"Name",
					"Property",
					"Value"
				});

			var instance = new TestObject {Name = "Yes", Property = "property"};

			// Check field accessor
			Assert.Equal(descriptor["Name"].Get(instance), "Yes");
			descriptor["Name"].Set(instance, "No");
			Assert.Equal(instance.Name, "No");

			// Check property accessor
			Assert.Equal(descriptor["Property"].Get(instance), "property");
			descriptor["Property"].Set(instance, "property1");
			Assert.Equal(instance.Property, "property1");

			// Check ShouldSerialize
			Assert.True(descriptor["Name"].ShouldSerialize(instance));

			Assert.False(descriptor["Value"].ShouldSerialize(instance));
			instance.Value = 1;
			Assert.True(descriptor["Value"].ShouldSerialize(instance));

			Assert.False(descriptor["DefaultValue"].ShouldSerialize(instance));
			instance.DefaultValue++;
			Assert.True(descriptor["DefaultValue"].ShouldSerialize(instance));

			// Check HasSet
			Assert.True(descriptor["Collection"].HasSet);
			Assert.False(descriptor["CollectionReadOnly"].HasSet);
		}
	}
}