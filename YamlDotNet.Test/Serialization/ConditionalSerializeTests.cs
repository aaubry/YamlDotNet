using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;

namespace YamlDotNet.Test.Serialization
{
    public class ConditionalSerializeTests
    {
        class TestConditionalSerializeClass
        {
            public string ID1 { get { return "1"; } } 
            public string ID2 { get { return "2"; } } 
            public string ID3 { get { return "3"; } } 
            public string ID4 { get { return "4"; } } 
            public string ID5 { get { return "5"; } }
            public string ID6 { get { return "6"; } }

            public class CustomClassReturn
            {
                public override bool Equals(object obj)
                {
                    return false;
                }

                public override int GetHashCode()
                {
                    return base.GetHashCode();
                }
            }

            public bool ShouldSerializeID1()
            {
                return true;
            }

            public bool ShouldSerializeID2()
            {
                return false;
            }

            public void ShouldSerializeID3()
            {
            }

            
            private void ShouldSerializeID4()
            {
                throw new Exception();
            }

            public bool CustomPrefixID5()
            {
                return false;
            }

            public CustomClassReturn ShouldSerializeID6()
            {
                return new CustomClassReturn();
            }
        }

        [Fact]
        public void TestConditionSerializer()
        {
            var sut = new SerializerBuilder()
                .WithTypeInspector(a => new ConditionalSerializeTypeInspector(a))
                .Build();
            //ID1 should serialize true
            //ID2 shouldn't serialize false
            //ID3 should serilize on error
            //ID4 should serialize since its not public and can't find function 
            //ID5 shouldn't serialize but with custom prefix
            //ID6 shouldn't serialize because the class equals false
            var expected = "ID1: 1\r\n" +
                           "ID3: 3\r\n" +
                           "ID4: 4\r\n" +
                           "ID5: 5\r\n";

            var result = sut.Serialize(new TestConditionalSerializeClass());

            Assert.Equal(expected, result);
        }


        [Fact]
        public void TestConditionSerializerPrefix()
        {
            var sut = new SerializerBuilder()
                .WithTypeInspector(a => new ConditionalSerializeTypeInspector(a, "CustomPrefix"))
                .Build();
            //Everything except ID5 should serialize because the custom prefix Only ID5 should serialize because of the custom prefix

            var expected = "ID1: 1\r\n" +
                           "ID2: 2\r\n" +
                           "ID3: 3\r\n" +
                           "ID4: 4\r\n" +
                           "ID6: 6\r\n";;


            var result = sut.Serialize(new TestConditionalSerializeClass());

            Assert.Equal(expected, result);
        }
    }
}
