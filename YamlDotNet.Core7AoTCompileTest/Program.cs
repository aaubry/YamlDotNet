// This file is part of YamlDotNet - A .NET library for YAML.
// Copyright (c) Antoine Aubry and contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8618 // Possible null reference argument.
#pragma warning disable CS8602 // Possible null reference argument.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Core7AoTCompileTest.Model;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Callbacks;

string yaml = string.Create(CultureInfo.InvariantCulture, $@"MyBool: true
hi: 1
MyChar: h
MyDateTime: {DateTime.Now}
MyDecimal: 123.935
MyDouble: 456.789
MyEnumY: Y
MyEnumZ: 1
MyInt16: {short.MaxValue}
MyInt32: {int.MaxValue}
MyInt64: {long.MaxValue}
MySByte: {sbyte.MaxValue}
MySingle: {float.MaxValue}
MyString: hello world
MyUInt16: {ushort.MaxValue}
MyUInt32: {uint.MaxValue}
MyUInt64: {ulong.MaxValue}
Inner:
  Text: yay
InnerArray:
  - Text: hello
  - Text: world
MyArray:
  myArray:
  - 1
  - 2
  - 3
MyDictionary:
  x: y
  a: b
MyDictionaryOfArrays:
  a:
  - a
  - b
  b:
  - c
  - d
MyList:
- a
- b
Inherited:
  Inherited: hello
  NotInherited: world
External:
  Text: hello
SomeCollectionStrings:
- test
- value
SomeEnumerableStrings:
- test
- value
SomeObject: a
SomeDictionary:
  a: 1
  b: 2
StructField:
  X: 1
  Y: 2
  Nested:
    X: 3
    Y: 4
StructProperty:
  X: 5
  Y: 6
  Nested:
    X: 7
    Y: 8
");

var input = new StringReader(yaml);

var aotContext = new YamlDotNet.Core7AoTCompileTest.StaticContext();
var deserializer = new StaticDeserializerBuilder(aotContext)
    .Build();

var x = deserializer.Deserialize<PrimitiveTypes>(input);
Console.WriteLine("Object read:");
Console.WriteLine("MyBool: <{0}>", x.MyBool);
Console.WriteLine("MyByte: <{0}>", x.MyByte);
Console.WriteLine("MyChar: <{0}>", x.MyChar);
Console.WriteLine("MyDateTime: <{0}>", x.MyDateTime);
Console.WriteLine("MyEnumY: <{0}>", x.MyEnumY);
Console.WriteLine("MyEnumZ: <{0}>", x.MyEnumZ);
Console.WriteLine("MyInt16: <{0}>", x.MyInt16);
Console.WriteLine("MyInt32: <{0}>", x.MyInt32);
Console.WriteLine("MyInt64: <{0}>", x.MyInt64);
Console.WriteLine("MySByte: <{0}>", x.MySByte);
Console.WriteLine("MyString: <{0}>", x.MyString);
Console.WriteLine("MyUInt16: <{0}>", x.MyUInt16);
Console.WriteLine("MyUInt32: <{0}>", x.MyUInt32);
Console.WriteLine("MyUInt64: <{0}>", x.MyUInt64);
Console.WriteLine("Inner == null: <{0}>", x.Inner == null);
Console.WriteLine("Inner.Text: <{0}>", x.Inner?.Text);
Console.WriteLine("External.Text: <{0}>", x.External?.Text);
foreach (var inner in x.InnerArray)
{
    Console.WriteLine("InnerArray.Text: <{0}>", inner.Text);
}
Console.WriteLine("MyArray == null: <{0}>", x.MyArray == null);
Console.WriteLine("MyArray.myArray == null: <{0}>", x.MyArray?.myArray == null);

if (x.MyArray?.myArray != null)
{
    Console.WriteLine("MyArray.myArray: <{0}>", string.Join(',', x.MyArray.myArray));
}

Console.WriteLine("MyDictionary == null: <{0}>", x.MyDictionary == null);
if (x.MyDictionary != null)
{
    foreach (var kvp in x.MyDictionary)
    {
        Console.WriteLine("MyDictionary[{0}] = <{1}>", kvp.Key, kvp.Value);
    }
}

Console.WriteLine("MyDictionaryOfArrays == null: <{0}>", x.MyDictionaryOfArrays == null);
if (x.MyDictionaryOfArrays != null)
{
    foreach (var kvp in x.MyDictionaryOfArrays)
    {
        Console.WriteLine("MyDictionaryOfArrays[{0}] = <{1}>", kvp.Key, string.Join(',', kvp.Value));
    }
}

Console.WriteLine("MyList == null: <{0}>", x.MyList == null);
if (x.MyList != null)
{
    foreach (var value in x.MyList)
    {
        Console.WriteLine("MyList = <{0}>", value);
    }
}
Console.WriteLine("Inherited == null: <{0}>", x.Inherited == null);
Console.WriteLine("Inherited.Inherited: <{0}>", x.Inherited?.Inherited);
Console.WriteLine("Inherited.NotInherited: <{0}>", x.Inherited?.NotInherited);
Console.WriteLine("SomeEnumerableStrings:");
foreach (var s in x.SomeEnumerableStrings)
{
    Console.WriteLine("  {0}", s);
}
Console.ReadLine();
Console.WriteLine("SomeCollectionStrings:");
foreach (var s in x.SomeCollectionStrings)
{
    Console.WriteLine("  {0}", s);
}
Console.WriteLine("Structs:");
Console.WriteLine("  StructField: <{0},{1}>", x.StructField.X, x.StructField.Y);
Console.WriteLine("    Nested: <{0},{1}>", x.StructField.Nested.X, x.StructField.Nested.Y);
Console.WriteLine("  StructProperty: <{0},{1}>", x.StructProperty.X, x.StructProperty.Y);
Console.WriteLine("    Nested: <{0},{1}>", x.StructProperty.Nested.X, x.StructProperty.Nested.Y);

Console.WriteLine("==============");
Console.WriteLine("Serialized:");

var serializer = new StaticSerializerBuilder(aotContext)
    .Build();

var output = serializer.Serialize(x);
Console.WriteLine(output);
Console.WriteLine("============== Done with the primary object");

yaml = @"- myArray:
  - 1
  - 2
- myArray:
  - 3
  - 4
";

var o = deserializer.Deserialize<MyArray[]>(yaml);
Console.WriteLine("Length: <{0}>", o.Length);
Console.WriteLine("Items[0]: <{0}>", string.Join(',', o[0].myArray));
Console.WriteLine("Items[1]: <{0}>", string.Join(',', o[1].myArray));

deserializer = new StaticDeserializerBuilder(aotContext).WithEnforceNullability().Build();
yaml = "Nullable: null";
var nullable = deserializer.Deserialize<NullableTestClass>(yaml);
Console.WriteLine("Nullable Value (should be empty): <{0}>", nullable.Nullable);
yaml = "NotNullable: test";
nullable = deserializer.Deserialize<NullableTestClass>(yaml);
Console.WriteLine("NotNullable Value (should be test): <{0}>", nullable.NotNullable);
try
{
    yaml = "NotNullable: null";
    nullable = deserializer.Deserialize<NullableTestClass>(yaml);
    throw new Exception("NotNullable should not be allowed to be set to null.");
}
catch (YamlException exception)
{
    if (exception.InnerException is NullReferenceException)
    {
        Console.WriteLine("Exception thrown while setting non nullable value to null, as it should.");
    }
    else
    {
        throw new Exception("NotNullable should not be allowed to be set to null.");
    }
}

Console.WriteLine("The next line should say goodbye");
Console.WriteLine(serializer.Serialize(EnumMemberedEnum.Hello));
Console.WriteLine("The next line should say hello");
Console.WriteLine(deserializer.Deserialize<EnumMemberedEnum>("goodbye"));

[YamlSerializable]
public class MyArray
{
    public int[]? myArray { get; set; }
}

[YamlSerializable]
public class Inner
{
    public string? Text { get; set; }
}

[YamlSerializable]
public class NullableTestClass
{
    public string? Nullable { get; set; }
    public string NotNullable { get; set; }
}

[YamlSerializable]
public class PrimitiveTypes
{
    [YamlMember(Description = "hi world!")]
    public bool MyBool { get; set; }
    [YamlMember(Alias = "hi")]
    public byte MyByte { get; set; }
    public char MyChar { get; set; }
    public decimal MyDecimal { get; set; }
    public double MyDouble { get; set; }
    public DateTime MyDateTime { get; set; }
    public MyTestEnum MyEnumY { get; set; }
    public MyTestEnum MyEnumZ { get; set; }
    public short MyInt16 { get; set; }
    public int MyInt32 { get; set; }
    public long MyInt64 { get; set; }
    public sbyte MySByte { get; set; }
    public float MySingle { get; set; }
    [YamlMember(ScalarStyle = ScalarStyle.DoubleQuoted)]
    public string MyString { get; set; } = string.Empty;
    public string? MyNullableString { get; set; }
    public ushort MyUInt16 { get; set; }
    public uint MyUInt32 { get; set; }
    public ulong MyUInt64 { get; set; }
    public Inner? Inner { get; set; }
    public Inner[]? InnerArray { get; set; }
    public MyArray? MyArray { get; set; }
    public Dictionary<string, string>? MyDictionary { get; set; }
    public Dictionary<string, string[]>? MyDictionaryOfArrays { get; set; }
    public List<string>? MyList { get; set; }
    public Inherited Inherited { get; set; }
    public ExternalModel External { get; set; }
    public IEnumerable<string> SomeEnumerableStrings { get; set; }
    public ICollection<string> SomeCollectionStrings { get; set; }
    public object SomeObject { get; set; }
    public object SomeDictionary { get; set; }
    public MyTestStruct StructField;
    public MyTestStruct StructProperty { get; set; }
}

public class InheritedBase
{
    public string Inherited { get; set; }
}

[YamlSerializable]
public class Inherited : InheritedBase
{
    public string NotInherited { get; set; }


    [OnSerializing]
    public void Serializing()
    {
        Console.WriteLine("Serializing");
    }

    [OnSerialized]
    public void Serialized()
    {
        Console.WriteLine("Serialized");
    }

    [OnDeserialized]
    public void Deserialized()
    {
        Console.WriteLine("Deserialized");
    }

    [OnDeserializing]
    public void Deserializing()
    {
        Console.WriteLine("Deserializing");
    }

}

public enum MyTestEnum
{
    Y = 0,
    Z = 1,
}

[YamlSerializable]
public enum EnumMemberedEnum
{
    No = 0,

    [System.Runtime.Serialization.EnumMember(Value = "goodbye")]
    Hello = 1
}

[YamlSerializable]
public struct MyTestStruct
{
    public float X;
    public float Y;
    public MyTestNestedStruct Nested;
    
    [OnSerializing]
    public void Serializing()
    {
        Console.WriteLine("MyTestStruct: Serializing");
    }

    [OnSerialized]
    public void Serialized()
    {
        Console.WriteLine("MyTestStruct: Serialized");
    }

    [OnDeserialized]
    public void Deserialized()
    {
        Console.WriteLine("MyTestStruct: Deserialized");
    }

    [OnDeserializing]
    public void Deserializing()
    {
        Console.WriteLine("MyTestStruct: Deserializing");
    }
}

[YamlSerializable]
public struct MyTestNestedStruct
{
    public float X;
    public float Y;
}

#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8618 // Possible null reference argument.
#pragma warning restore CS8602 // Possible null reference argument.
