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

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using FluentAssertions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test.Serialization
{
    public class SerializationTestHelper
    {
        private SerializerBuilder serializerBuilder;
        private DeserializerBuilder deserializerBuilder;

        protected T DoRoundtripFromObjectTo<T>(object obj)
        {
            return DoRoundtripFromObjectTo<T>(obj, Serializer);
        }

        protected T DoRoundtripFromObjectTo<T>(object obj, ISerializer serializer)
        {
            return DoRoundtripFromObjectTo<T>(obj, serializer, Deserializer);
        }

        protected T DoRoundtripFromObjectTo<T>(object obj, ISerializer serializer, IDeserializer deserializer)
        {
            var writer = new StringWriter();
            serializer.Serialize(writer, obj);
            return deserializer.Deserialize<T>(UsingReaderFor(writer));
        }

        protected T DoRoundtripOn<T>(object obj)
        {
            return DoRoundtripOn<T>(obj, Serializer);
        }

        protected T DoRoundtripOn<T>(object obj, ISerializer serializer)
        {
            var writer = new StringWriter();
            serializer.Serialize(writer, obj, typeof(T));
            return new Deserializer().Deserialize<T>(UsingReaderFor(writer));
        }

        protected SerializerBuilder SerializerBuilder
        {
            get
            {
                return serializerBuilder ??= new SerializerBuilder();
            }
        }

        protected ISerializer Serializer
        {
            get
            {
                return SerializerBuilder.Build();
            }
        }

        protected DeserializerBuilder DeserializerBuilder
        {
            get
            {
                return deserializerBuilder ??= new DeserializerBuilder();
            }
        }

        protected IDeserializer Deserializer
        {
            get
            {
                return DeserializerBuilder.Build();
            }
        }

        protected void AssumingDeserializerWith(IObjectFactory factory)
        {
            deserializerBuilder = new DeserializerBuilder()
                .WithObjectFactory(factory);
        }

        protected TextReader UsingReaderFor(TextWriter buffer)
        {
            return UsingReaderFor(buffer.ToString());
        }

        protected TextReader UsingReaderFor(string text)
        {
            return new StringReader(text);
        }

        protected static IParser ParserFor(string yaml)
        {
            return new Parser(new StringReader(yaml));
        }

        protected string Lines(params string[] lines)
        {
            return string.Join("\r\n", lines);
        }

        protected object Entry(string key, string value)
        {
            return new DictionaryEntry(key, value);
        }
    }

    // ReSharper disable InconsistentNaming

    [Flags]
    public enum EnumExample
    {
        None,
        One,
        Two
    }

    public struct StructExample
    {
        public int Value { get; set; }
    }

    public enum SByteEnum : sbyte { Default, Sbyte }
    public enum ByteEnum : byte { Default, Byte }
    public enum Int16Enum : short { Default, Short }
    public enum UInt16Enum : ushort { Default, Ushort }
    public enum Int32Enum : int { Default, Int }
    public enum UInt32Enum : uint { Default, Uint }
    public enum Int64Enum : long { Default, Long }
    public enum UInt64Enum : ulong { Default, Ulong }

    public class CircularReference
    {
        public CircularReference Child1 { get; set; }
        public CircularReference Child2 { get; set; }
    }

    [TypeConverter(typeof(ConvertibleConverter))]
    public class Convertible : IConvertible
    {
        public string Left { get; set; }
        public string Right { get; set; }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            conversionType.Should().Be<string>();
            return ToString(provider);
        }

        public string ToString(IFormatProvider provider)
        {
            provider.Should().Be(CultureInfo.InvariantCulture);
            return string.Format(provider, "[{0}, {1}]", Left, Right);
        }

        #region Unsupported Members

        public System.TypeCode GetTypeCode()
        {
            throw new NotSupportedException();
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    public class ConvertibleConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return false;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (!(value is string))
            {
                throw new InvalidOperationException();
            }

            var parts = (value as string).Split(' ');
            return new Convertible
            {
                Left = parts[0],
                Right = parts[1]
            };
        }
    }

    public class MissingDefaultCtor
    {
        public string Value;

        public MissingDefaultCtor(string value)
        {
            Value = value;
        }
    }

    public class MissingDefaultCtorConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(MissingDefaultCtor);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            var value = ((Scalar)parser.Current).Value;
            parser.MoveNext();
            return new MissingDefaultCtor(value);
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            emitter.Emit(new Scalar(((MissingDefaultCtor)value).Value));
        }
    }

    public class InheritanceExample
    {
        public object SomeScalar { get; set; }
        public Base RegularBase { get; set; }

        [YamlMember(serializeAs: typeof(Base))]
        public Base BaseWithSerializeAs { get; set; }
    }

    public class InterfaceExample
    {
        public IDerived Derived { get; set; }
    }

    public interface IBase
    {
        string BaseProperty { get; set; }
    }

    public interface IDerived : IBase
    {
        string DerivedProperty { get; set; }
    }

    public class Base : IBase
    {
        public string BaseProperty { get; set; }
    }

    public class Derived : Base, IDerived
    {
        public string DerivedProperty { get; set; }
    }

    public class EmptyBase
    {
    }

    public class EmptyDerived : EmptyBase
    {
    }

    public class Simple
    {
        public string aaa { get; set; }
    }

    public class SimpleScratch
    {
        public string Scratch { get; set; }
        public bool DeleteScratch { get; set; }
        public IEnumerable<string> MappedScratch { get; set; }
    }

    public class Example
    {
        public bool MyFlag { get; set; }
        public string Nothing { get; set; }
        public int MyInt { get; set; }
        public double MyDouble { get; set; }
        public string MyString { get; set; }
        public DateTime MyDate { get; set; }
        public TimeSpan MyTimeSpan { get; set; }
        public Point MyPoint { get; set; }
        public int? MyNullableWithValue { get; set; }
        public int? MyNullableWithoutValue { get; set; }

        public Example()
        {
            MyInt = 1234;
            MyDouble = 6789.1011;
            MyString = "Hello world";
            MyDate = DateTime.Now;
            MyTimeSpan = TimeSpan.FromHours(1);
            MyPoint = new Point(100, 200);
            MyNullableWithValue = 8;
        }
    }

    public class OrderExample
    {
        public OrderExample()
        {
            this.Order1 = "Order1 value";
            this.Order2 = "Order2 value";
        }

        [YamlMember(Order = 2)]
        public string Order2 { get; set; }

        [YamlMember(Order = 1)]
        public string Order1 { get; set; }
    }

    public class IgnoreExample
    {
        [YamlIgnore]
        public string IgnoreMe
        {
            get { throw new InvalidOperationException("Accessing a [YamlIgnore] property"); }
            set { throw new InvalidOperationException("Accessing a [YamlIgnore] property"); }
        }
    }

    public class IgnoreExampleBase
    {
        [YamlIgnore]
        public virtual string IgnoreMe
        {
            get { throw new InvalidOperationException("Accessing a [YamlIgnore] property"); }
            set { throw new InvalidOperationException("Accessing a [YamlIgnore] property"); }
        }
    }

    public class IgnoreExampleDerived : IgnoreExampleBase
    {
        public override string IgnoreMe
        {
            get { throw new InvalidOperationException("Accessing a [YamlIgnore] property"); }
            set { throw new InvalidOperationException("Accessing a [YamlIgnore] property"); }
        }
    }

    public class ScalarStyleExample
    {
        public ScalarStyleExample()
        {
            var content = "Test";
            LiteralString = content;
            DoubleQuotedString = content;
        }

        [YamlMember(ScalarStyle = ScalarStyle.Literal)]
        public string LiteralString { get; set; }

        [YamlMember(ScalarStyle = ScalarStyle.DoubleQuoted)]
        public string DoubleQuotedString { get; set; }
    }

    /// <summary>
    /// This class demonstrates an object with serializable data that will be
    /// written to YAML with strings that are formatted as valid numbers.
    /// </summary>
    public class MixedFormatScalarStyleExample
    {
        public string[] Data { get; }

        public MixedFormatScalarStyleExample(string[] data)
        {
            Data = data;
        }
    }

    public class DefaultsExample
    {
        public const string DefaultValue = "myDefault";

        [DefaultValue(DefaultValue)]
        public string Value { get; set; }
    }

    public class ThrowingPropertyExample
    {
        public string Value => throw new InvalidOperationException();
    }

    public class CustomGenericDictionary : IDictionary<string, string>
    {
        private readonly Dictionary<string, string> dictionary = new Dictionary<string, string>();

        public void Add(string key, string value)
        {
            dictionary.Add(key, value);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region Unsupported Members

        public bool ContainsKey(string key)
        {
            throw new NotSupportedException();
        }

        public ICollection<string> Keys
        {
            get { throw new NotSupportedException(); }
        }

        public bool Remove(string key)
        {
            throw new NotSupportedException();
        }

        public bool TryGetValue(string key, out string value)
        {
            throw new NotSupportedException();
        }

        public ICollection<string> Values
        {
            get { throw new NotSupportedException(); }
        }

        public string this[string key]
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public void Add(KeyValuePair<string, string> item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public int Count
        {
            get { throw new NotSupportedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotSupportedException(); }
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    public class NameConvention
    {
        public string FirstTest { get; set; }
        public string SecondTest { get; set; }
        public string ThirdTest { get; set; }

        [YamlMember(Alias = "fourthTest")]
        public string AliasTest { get; set; }

        [YamlIgnore]
        public string fourthTest { get; set; }
    }

    public class NonPublicPropertiesExample
    {
        public string Public { get; set; } = "public";

        internal string Internal { get; set; } = "internal";

        protected string Protected { get; set; } = "protected";

        private string Private { get; set; } = "private";

        /// <inheritdoc />
        public override string ToString() => $"{Public},{Internal},{Protected},{Private}";
    }

#pragma warning disable IDE0044 // Add readonly modifier
    public class NonPublicFieldsExample
    {
        public string Public = "public";

        internal string Internal = "internal";

        protected string Protected = "protected";

        private string Private = "private";

        /// <inheritdoc />
        public override string ToString() => $"{Public},{Internal},{Protected},{Private}";
    }
#pragma warning restore IDE0044 // Add readonly modifier
}
