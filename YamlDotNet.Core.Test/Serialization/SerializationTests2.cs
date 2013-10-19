using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test.Serialization
{
	public class SerializationTests2
	{
		public enum MyEnum
		{
			A,
			B,
		}

		public class MyObject
		{
			public MyObject()
			{
				ArrayContent = new int[2];
			}

			public string String { get; set; }

			public sbyte SByte { get; set; }

			public byte Byte { get; set; }

			public short Int16 { get; set; }

			public ushort UInt16 { get; set; }

			public int Int32 { get; set; }

			public uint UInt32 { get; set; }

			public long Int64 { get; set; }

			public ulong UInt64 { get; set; }

			public float Float { get; set; }

			public double Double { get; set; }

			public MyEnum Enum { get; set; }

			public bool Bool { get; set; }

			public bool BoolFalse { get; set; }

			public string A0Anchor { get; set; }

			public string A1Alias { get; set; }

			public int[] Array { get; set; }

			public int[] ArrayContent { get; private set; }
		}

		[Fact]
		public void TestSimpleObjectAndPrimitive()
		{
			var text = @"!MyObject
A0Anchor: &o1 Test
A1Alias: *o1
Array: [1, 2, 3]
ArrayContent: [1, 2]
Bool: true
BoolFalse: false
Byte: 2
Double: 6.6
Enum: B
Float: 5.5
Int16: 3
Int32: 5
Int64: 7
SByte: 1
String: This is a test
UInt16: 4
UInt32: 6
UInt64: 8
".Trim();

			var settings = new SerializerSettings() {LimitPrimitiveFlowSequence = 20};
			settings.RegisterTagMapping("MyObject", typeof (MyObject));
			SerialRoundTrip(settings, text);
		}

		public class MyObjectAndCollection
		{
			public MyObjectAndCollection()
			{
				Values = new List<string>();
			}

			public string Name { get; set; }

			public List<string> Values { get; set; }
		}


		/// <summary>
		/// Tests the serialization of an object that contains a property with list
		/// </summary>
		[Fact]
		public void TestObjectWithCollection()
		{
			var text = @"!MyObjectAndCollection
Name: Yes
Values: [a, b, c]
".Trim();

			var settings = new SerializerSettings() {LimitPrimitiveFlowSequence = 20};
			settings.RegisterTagMapping("MyObjectAndCollection", typeof (MyObjectAndCollection));
			SerialRoundTrip(settings, text);
		}

		public class MyCustomCollectionWithProperties : List<string>
		{
			public string Name { get; set; }

			public int Value { get; set; }
		}

		/// <summary>
		/// Tests the serialization of a custom collection with some custom properties.
		/// In this specific case, the collection cannot be serialized as a simple list
		/// so the serializer is serializing the list as a YAML mapping, using the mapping
		/// to store the usual propertis and using the special member '~Items' to serialzie 
		/// the real content of the list
		/// </summary>
		[Fact]
		public void TestCustomCollectionWithProperties()
		{
			var text = @"!MyCustomCollectionWithProperties
Name: Yes
Value: 1
~Items:
  - a
  - b
  - c
".Trim();

			var settings = new SerializerSettings() {LimitPrimitiveFlowSequence = 0};
			settings.RegisterTagMapping("MyCustomCollectionWithProperties", typeof (MyCustomCollectionWithProperties));
			SerialRoundTrip(settings, text);
		}


		public class MyCustomDictionaryWithProperties : Dictionary<string, bool>
		{
			public string Name { get; set; }

			public int Value { get; set; }
		}


		/// <summary>
		/// Tests the serialization of a custom dictionary with some custom properties.
		/// In this specific case, the dictionary cannot be serialized as a simple mapping
		/// so the serializer is serializing the dictionary as a YAML !!map, using the mapping
		/// to store the usual propertis and using the special member '~Items' to serialize 
		/// the real content of the dictionary as a sub YAML !!map
		/// </summary>
		[Fact]
		public void TestCustomDictionaryWithProperties()
		{
			var text = @"!MyCustomDictionaryWithProperties
Name: Yes
Value: 1
~Items:
  a: true
  b: false
  c: true
".Trim();

			var settings = new SerializerSettings() {LimitPrimitiveFlowSequence = 0};
			settings.RegisterTagMapping("MyCustomDictionaryWithProperties", typeof (MyCustomDictionaryWithProperties));
			SerialRoundTrip(settings, text);
		}

		public class MyCustomClassWithSpecialMembers
		{
			public MyCustomClassWithSpecialMembers()
			{
				StringListByContent = new List<string>();
				StringMapbyContent = new Dictionary<string, object>();
				ListByContent = new List<string>();
			}

			public string Name { get; set; }

			public int Value { get; set; }

			/// <summary>
			/// Gets or sets the basic list.
			/// </summary>
			/// <value>The basic list.</value>
			public IList BasicList { get; set; }

			/// <summary>
			/// For this property, the deserializer is instantiating
			/// automatically a default List&lt;string&gtl instance.
			/// </summary>
			public IList<string> StringList { get; set; }

			/// <summary>
			/// For this property, the deserializer is using the actual
			/// value of the list stored in this instance instead of 
			/// creating a new List&lt;T&gtl instance.
			/// </summary>
			public List<string> StringListByContent { get; private set; }

			/// <summary>
			/// Gets or sets the basic map.
			/// </summary>
			/// <value>The basic map.</value>
			public IDictionary BasicMap { get; set; }

			/// <summary>
			/// Idem as for <see cref="StringList"/> but for dictionary.
			/// </summary>
			/// <value>The string map.</value>
			public IDictionary<string, object> StringMap { get; set; }


			/// <summary>
			/// Idem as for <see cref="StringListByContent"/> but for dictionary.
			/// </summary>
			/// <value>The content of the string mapby.</value>
			public Dictionary<string, object> StringMapbyContent { get; private set; }

			/// <summary>
			/// For this property, the deserializer is using the actual
			/// value of the list stored in this instance instead of 
			/// creating a new List&lt;T&gtl instance.
			/// </summary>
			/// <value>The content of the list by.</value>
			public IList ListByContent { get; private set; }
		}

		/// <summary>
		/// Tests the serialization of a custom dictionary with some custom properties.
		/// In this specific case, the dictionary cannot be serialized as a simple mapping
		/// so the serializer is serializing the dictionary as a YAML !!map, using the mapping
		/// to store the usual propertis and using the special member '~Items' to serialize 
		/// the real content of the dictionary as a sub YAML !!map
		/// </summary>
		[Fact]
		public void TestMyCustomClassWithSpecialMembers()
		{
			var text = @"!MyCustomClassWithSpecialMembers
BasicList:
  - 1
  - 2
BasicMap:
  a: 1
  b: 2
ListByContent:
  - a
  - b
Name: Yes
StringList:
  - 1
  - 2
StringListByContent:
  - 3
  - 4
StringMap:
  c: yes
  d: 3
StringMapbyContent:
  e: 4
  f: no
Value: 0
".Trim();

			var settings = new SerializerSettings() {LimitPrimitiveFlowSequence = 0};
			settings.RegisterTagMapping("MyCustomClassWithSpecialMembers", typeof (MyCustomClassWithSpecialMembers));
			SerialRoundTrip(settings, text);
		}


		/// <summary>
		/// Tests the serialization of ordered members.
		/// </summary>
		public class ClassMemberOrder
		{
			public ClassMemberOrder()
			{
				First = 1;
				Second = 2;
				BeforeName = 3;
				Name = 4;
				NameAfter = 5;
			}

			/// <summary>
			/// Sets an explicit order
			/// </summary>
			[YamlMember(1)]
			public int Second { get; set; }

			/// <summary>
			/// Sets an explicit order
			/// </summary>
			[YamlMember(0)]
			public int First { get; set; }

			/// <summary>
			/// This property will be sorted after 
			/// the explicit order by alphabetical order
			/// </summary>
			/// <value>The name after.</value>
			public int NameAfter { get; set; }

			/// <summary>
			/// This property will be sorted after 
			/// the explicit order by alphabetical order
			/// </summary>
			public int Name { get; set; }

			/// <summary>
			/// Sets an explicit order
			/// </summary>
			[YamlMember(2)]
			public int BeforeName { get; set; }
		}

		/// <summary>
		/// Tests the serialization of ordered members in the class <see cref="ClassMemberOrder"/>.
		/// </summary>
		[Fact]
		public void TestClassMemberOrder()
		{
			var settings = new SerializerSettings() { LimitPrimitiveFlowSequence = 0 };
			settings.RegisterTagMapping("ClassMemberOrder", typeof(ClassMemberOrder));
			SerialRoundTrip(settings, new ClassMemberOrder());
		}

		public class ClassWithMemberIEnumerable
		{
			public IEnumerable<int> Keys
			{
				get { return Enumerable.Range(0, 10); }
			}
		}

		[Fact]
		public void TestIEnumerable()
		{
			var serializer = new Serializer();
			var text = serializer.Serialize(new ClassWithMemberIEnumerable(), typeof (ClassWithMemberIEnumerable));
			Assert.Throws<YamlException>(() => serializer.Deserialize(text, typeof(ClassWithMemberIEnumerable)));
			var value = serializer.Deserialize(text);

			Assert.True(value is IDictionary<object, object>);
			var dictionary = (IDictionary<object, object>) value;
			Assert.True(dictionary.ContainsKey("Keys"));
			Assert.True( dictionary["Keys"] is IList<object>);
			var list = (IList<object>) dictionary["Keys"];
			Assert.Equal(list.OfType<int>(), new ClassWithMemberIEnumerable().Keys);

			// Test simple IEnumerable
			var iterator = Enumerable.Range(0, 10);
			var values = serializer.Deserialize(serializer.Serialize(iterator, iterator.GetType()));
			Assert.True(value is IEnumerable);
			Assert.Equal(((IEnumerable<object>)values).OfType<int>(), iterator);
		}

		public class ClassWithObjectAndScalar
		{
			public ClassWithObjectAndScalar()
			{
				Value1 = 1;
				Value2 = 2.0f;
				Value3 = "3";
				Value4 = (byte)4;
			}

			public object Value1;

			public object Value2;

			public object Value3;

			public object Value4;
		}

		[Fact]
		public void TestClassWithObjectAndScalar()
		{
			var settings = new SerializerSettings() { LimitPrimitiveFlowSequence = 0 };
			settings.RegisterTagMapping("ClassWithObjectAndScalar", typeof(ClassWithObjectAndScalar));
			SerialRoundTrip(settings, new ClassWithObjectAndScalar());
		}

		[Fact]
		public void TestImplicitDictionaryAndList()
		{
			var settings = new SerializerSettings() { LimitPrimitiveFlowSequence = 0 };

			var text = @"BasicList:
  - 1
  - 2
BasicMap:
  a: 1
  b: 2
ListByContent:
  - a
  - b
Name: Yes
StringList:
  - 1
  - 2
StringListByContent:
  - 3
  - 4
StringMap:
  c: yes
  d: 3
StringMapbyContent:
  e: 4
  f: no
Value: 0
".Trim();

			SerialRoundTrip(settings, text, typeof(Dictionary<object, object>));
		}


		public interface IMemberInterface
		{
			string Name { get; set; }
		}

		public class MemberInterface : IMemberInterface 
		{
			protected bool Equals(MemberInterface other)
			{
				return string.Equals(Name, other.Name) && string.Equals(Value, other.Value);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != this.GetType()) return false;
				return Equals((MemberInterface) obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return ((Name != null ? Name.GetHashCode() : 0)*397) ^ (Value != null ? Value.GetHashCode() : 0);
				}
			}

			public MemberInterface()
			{
				Name = "name1";
				Value = "value1";
			}

			public string Name { get; set; }

			public string Value { get; set; }
		}

		public class MemberObject : MemberInterface 
		{
			public MemberObject()
			{
				Object = "object1";
			}

			public string Object { get; set; }

			protected bool Equals(MemberObject other)
			{
				return base.Equals(other) && string.Equals(Object, other.Object);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != this.GetType()) return false;
				return Equals((MemberObject) obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return (base.GetHashCode()*397) ^ (Object != null ? Object.GetHashCode() : 0);
				}
			}
		}

		public class ClassMemberWithInheritance
		{
			public ClassMemberWithInheritance()
			{
				ThroughObject = new MemberObject() {Object = "throughObject"};
				ThroughInterface = new MemberInterface();
				ThroughBase = new MemberObject() {Object = "throughBase"};
				Direct = new MemberObject() {Object = "direct"};
			}

			public object ThroughObject { get; set; }

			public IMemberInterface ThroughInterface { get; set; }

			public MemberInterface ThroughBase { get; set; }

			public MemberObject Direct { get; set; }

			protected bool Equals(ClassMemberWithInheritance other)
			{
				return Equals(ThroughObject, other.ThroughObject) && Equals(ThroughInterface, other.ThroughInterface) && Equals(ThroughBase, other.ThroughBase) && Equals(Direct, other.Direct);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != this.GetType()) return false;
				return Equals((ClassMemberWithInheritance) obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int hashCode = (ThroughObject != null ? ThroughObject.GetHashCode() : 0);
					hashCode = (hashCode*397) ^ (ThroughInterface != null ? ThroughInterface.GetHashCode() : 0);
					hashCode = (hashCode*397) ^ (ThroughBase != null ? ThroughBase.GetHashCode() : 0);
					hashCode = (hashCode*397) ^ (Direct != null ? Direct.GetHashCode() : 0);
					return hashCode;
				}
			}
		}

		[Fact]
		public void TestClassMemberWithInheritance()
		{
			var settings = new SerializerSettings() { LimitPrimitiveFlowSequence = 0 };
			settings.RegisterTagMapping("ClassMemberWithInheritance", typeof(ClassMemberWithInheritance));
			settings.RegisterTagMapping("MemberInterface", typeof(MemberInterface));
			settings.RegisterTagMapping("MemberObject", typeof(MemberObject));
			var original = new ClassMemberWithInheritance();
			var obj = SerialRoundTrip(settings, original);
			Assert.True(obj is ClassMemberWithInheritance);
			Assert.Equal(original, obj);
		}

		[Fact]
		public void TestEmitShortTypeName()
		{
			var settings = new SerializerSettings() { EmitShortTypeName = true };
			SerialRoundTrip(settings, new ClassWithObjectAndScalar());
		}

		public class ClassWithChars
		{
			public char Start;

			public char End;
		}

		[Fact]
		public void TestClassWithChars()
		{
			var settings = new SerializerSettings() { EmitShortTypeName = true };
			SerialRoundTrip(settings, new ClassWithChars()
			{
				Start = ' ',
				End = '\x7f'
			});
		}


		[YamlStyle(YamlStyle.Flow)]
		public class ClassWithStyle
		{
			public string Name { get; set; }

			public object Value { get; set; }
		}

		public class ClassNoStyle
		{
			public ClassNoStyle()
			{
				A_ListWithCustomStyle = new List<string>();
				D_ListHandleByDynamicStyleFormat = new List<object>();
				E_ListDefaultPrimitiveLimit = new List<int>();
				E_ListDefaultPrimitiveLimitExceed = new List<int>();
				F_ListClassWithStyleDefaultFormat = new List<ClassWithStyle>();
				G_ListCustom = new CustomList();
			}

			[YamlStyle(YamlStyle.Flow)]
			public List<string> A_ListWithCustomStyle { get; set; }

			public ClassWithStyle B_ClassWithStyle { get; set; }

			[YamlStyle(YamlStyle.Block)]
			public ClassWithStyle C_ClassWithStyleOverridenByLocalYamlStyle { get; set; }

			public List<object> D_ListHandleByDynamicStyleFormat { get; set; }

			public List<int> E_ListDefaultPrimitiveLimit { get; set; }

			public List<int> E_ListDefaultPrimitiveLimitExceed { get; set; }

			public List<ClassWithStyle> F_ListClassWithStyleDefaultFormat { get; set; }

			public CustomList G_ListCustom { get; set; }
		}

		[YamlStyle(YamlStyle.Flow)]
		public class CustomList : List<object>
		{
			public string Name { get; set; }
		}


		private class FormatListObject : IDynamicStyleFormat
		{
			public YamlStyle GetStyle(SerializerContext context, object value, ITypeDescriptor descriptor)
			{
				return value is List<object> ? YamlStyle.Flow : YamlStyle.Any;
			}
		}

		/// <summary>
		/// Tests formatting styles.
		/// </summary>
		[Fact]
		public void TestStyles()
		{
			var settings = new SerializerSettings() {LimitPrimitiveFlowSequence = 4};
			settings.RegisterTagMapping("ClassNoStyle", typeof(ClassNoStyle));
			settings.RegisterTagMapping("ClassWithStyle", typeof(ClassWithStyle));
			settings.DynamicStyleFormat = new FormatListObject();

			var classNoStyle = new ClassNoStyle();
			classNoStyle.A_ListWithCustomStyle.Add("a");
			classNoStyle.A_ListWithCustomStyle.Add("b");
			classNoStyle.A_ListWithCustomStyle.Add("c");
			classNoStyle.B_ClassWithStyle = new ClassWithStyle() {Name = "name1", Value = 1};
			classNoStyle.C_ClassWithStyleOverridenByLocalYamlStyle = new ClassWithStyle() {Name = "name2", Value = 2};
			classNoStyle.D_ListHandleByDynamicStyleFormat.Add(1);
			classNoStyle.D_ListHandleByDynamicStyleFormat.Add(2);
			classNoStyle.D_ListHandleByDynamicStyleFormat.Add(3);
			classNoStyle.E_ListDefaultPrimitiveLimit.Add(1);
			classNoStyle.E_ListDefaultPrimitiveLimit.Add(2);
			classNoStyle.E_ListDefaultPrimitiveLimitExceed.Add(1);
			classNoStyle.E_ListDefaultPrimitiveLimitExceed.Add(2);
			classNoStyle.E_ListDefaultPrimitiveLimitExceed.Add(3);
			classNoStyle.E_ListDefaultPrimitiveLimitExceed.Add(4);
			classNoStyle.E_ListDefaultPrimitiveLimitExceed.Add(5);
			classNoStyle.F_ListClassWithStyleDefaultFormat.Add(new ClassWithStyle() {Name = "name3", Value = 3});
			classNoStyle.G_ListCustom.Name = "name4";
			classNoStyle.G_ListCustom.Add(1);
			classNoStyle.G_ListCustom.Add(2);
			classNoStyle.G_ListCustom.Add(3);
			classNoStyle.G_ListCustom.Add(4);
			classNoStyle.G_ListCustom.Add(5);
			classNoStyle.G_ListCustom.Add(6);
			classNoStyle.G_ListCustom.Add(7);

			var serializer = new Serializer(settings);
			var text = serializer.Serialize(classNoStyle).Trim();

			var textReference = @"!ClassNoStyle
A_ListWithCustomStyle: [a, b, c]
B_ClassWithStyle: {Name: name1, Value: 1}
C_ClassWithStyleOverridenByLocalYamlStyle:
  Name: name2
  Value: 2
D_ListHandleByDynamicStyleFormat: [1, 2, 3]
E_ListDefaultPrimitiveLimit: [1, 2]
E_ListDefaultPrimitiveLimitExceed:
  - 1
  - 2
  - 3
  - 4
  - 5
F_ListClassWithStyleDefaultFormat:
  - {Name: name3, Value: 3}
G_ListCustom: {Name: name4, ~Items: [1, 2, 3, 4, 5, 6, 7]}";

			Assert.Equal(textReference, text);
		}
		
		private void SerialRoundTrip(SerializerSettings settings, string text, Type serializedType = null)
		{
			var serializer = new Serializer(settings);
			// not working yet, scalar read/write are not yet implemented
			Console.WriteLine("Text to serialize:");
			Console.WriteLine("------------------");
			Console.WriteLine(text);
			var value = serializer.Deserialize(text);

			Console.WriteLine();

			var text2 = serializer.Serialize(value, serializedType).Trim();
			Console.WriteLine("Text deserialized:");
			Console.WriteLine("------------------");
			Console.WriteLine(text2);

			Assert.Equal(text, text2);
		}

		private object SerialRoundTrip(SerializerSettings settings, object value, Type expectedType = null)
		{
			var serializer = new Serializer(settings);

			var text = serializer.Serialize(value, expectedType);
			Console.WriteLine("Text serialize:");
			Console.WriteLine("------------------");
			Console.WriteLine(text);

			// not working yet, scalar read/write are not yet implemented
			Console.WriteLine("Text to deserialize/serialize:");
			Console.WriteLine("------------------");
			var valueDeserialized = serializer.Deserialize(text);
			var text2 = serializer.Serialize(valueDeserialized);
			Console.WriteLine(text2);

			Assert.Equal(text, text2);

			return valueDeserialized;
		}

	}
}