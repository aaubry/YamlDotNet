using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Test
{
	public class DescriptorTests
	{

		public class TestObject
		{
			// unused, not public
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
			var attributeRegistry = new AttributeRegistry();

			// Rename ItemRenamed2 to Item2
			attributeRegistry.Register(typeof(TestObject).GetProperty("ItemRenamed2"), new YamlMemberAttribute("Item2"));

			var descriptor = new ObjectDescriptor(attributeRegistry, typeof(TestObject), false);

			// Verify members
			Assert.Equal(8, descriptor.Count);

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
			Assert.Equal("Yes", descriptor["Name"].Get(instance));
			descriptor["Name"].Set(instance, "No");
			Assert.Equal("No", instance.Name);

			// Check property accessor
			Assert.Equal("property", descriptor["Property"].Get(instance));
			descriptor["Property"].Set(instance, "property1");
			Assert.Equal("property1", instance.Property);

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

		/// <summary>
		/// This is a non pure collection: It has at least one public get/set member.
		/// </summary>
		public class NonPureCollection : List<int>
		{
			public string Name { get; set; }
		}

		[Fact]
		public void TestCollectionDescriptor()
		{
			var attributeRegistry = new AttributeRegistry();
			var descriptor = new CollectionDescriptor(attributeRegistry, typeof (List<string>), false);

			// Only Capacity as a member
			Assert.Equal(1, descriptor.Count);
			Assert.True(descriptor.HasOnlyCapacity);
			Assert.False(descriptor.IsPureCollection);
			Assert.Equal(typeof(string), descriptor.ElementType);

			descriptor = new CollectionDescriptor(attributeRegistry, typeof(NonPureCollection), false);

			// Only Capacity as a member
			Assert.Equal(2, descriptor.Count);
			Assert.False(descriptor.HasOnlyCapacity);
			Assert.False(descriptor.IsPureCollection);
			Assert.Equal(typeof(int), descriptor.ElementType);

			descriptor = new CollectionDescriptor(attributeRegistry, typeof(ArrayList), false);
			// Only Capacity as a member
			Assert.Equal(1, descriptor.Count);
			Assert.True(descriptor.HasOnlyCapacity);
			Assert.False(descriptor.IsPureCollection);
			Assert.Equal(typeof(object), descriptor.ElementType);		
		}

		/// <summary>
		/// This is a non pure collection: It has at least one public get/set member.
		/// </summary>
		public class NonPureDictionary : Dictionary<float, object>
		{
			public string Name { get; set; }
		}

		[Fact]
		public void TestDictionaryDescriptor()
		{
			var attributeRegistry = new AttributeRegistry();
			var descriptor = new DictionaryDescriptor(attributeRegistry, typeof(Dictionary<int, string>), false);

			Assert.Equal(0, descriptor.Count);
			Assert.True(descriptor.IsPureDictionary);
			Assert.Equal(typeof(int), descriptor.KeyType);
			Assert.Equal(typeof(string), descriptor.ValueType);

			descriptor = new DictionaryDescriptor(attributeRegistry, typeof(NonPureDictionary), false);
			Assert.Equal(1, descriptor.Count);
			Assert.False(descriptor.IsPureDictionary);
			Assert.Equal(typeof(float), descriptor.KeyType);
			Assert.Equal(typeof(object), descriptor.ValueType);
		}

		[Fact]
		public void TestArrayDescriptor()
		{
			var attributeRegistry = new AttributeRegistry();
			var descriptor = new ArrayDescriptor(attributeRegistry, typeof(int[]));

			Assert.Equal(0, descriptor.Count);
			Assert.Equal(typeof(int), descriptor.ElementType);
		}

		public enum MyEnum
		{
			A,
			B
		}

		[Fact]
		public void TestPrimitiveDescriptor()
		{
			var attributeRegistry = new AttributeRegistry();
			var descriptor = new PrimitiveDescriptor(attributeRegistry, typeof(int));
			Assert.Equal(0, descriptor.Count);

			Assert.True(PrimitiveDescriptor.IsPrimitive(typeof(MyEnum)));
			Assert.True(PrimitiveDescriptor.IsPrimitive(typeof (object)));
			Assert.True(PrimitiveDescriptor.IsPrimitive(typeof(DateTime)));
			Assert.True(PrimitiveDescriptor.IsPrimitive(typeof(TimeSpan)));
			Assert.False(PrimitiveDescriptor.IsPrimitive(typeof(IList)));
		}
	}
}