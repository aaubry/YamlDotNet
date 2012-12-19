using Xunit;
using YamlDotNet.RepresentationModel;


namespace YamlDotNet.UnitTests.RepresentationModel
{
    class DynamicYamlTest
    {
        [Fact]
        public void TestMappingNode()
        {
            dynamic dynamicYaml = new DynamicYaml(MappingYaml);

            string receipt = (string)(dynamicYaml.Receipt);
            var firstPartNo = dynamicYaml.Items[0].part_no;
            Assert.Equal(receipt, "Oz-Ware Purchase Invoice");
        }

        [Fact]
        public void TestSequenceNode()
        {
            dynamic dynamicYaml = new DynamicYaml(SequenceYaml);

            string firstName = dynamicYaml[0].name;
            Assert.Equal(firstName, "Me");
        }

        [Fact]
        public void TestNestedSequenceNode()
        {
            dynamic dynamicYaml = new DynamicYaml(NestedSequenceYaml);

            var firstNumber = dynamicYaml[0, 0];
            Assert.Equal(firstNumber.ToString(), "1");
        }

        private const string SequenceYaml = @"---
            - name: Me
            - name: You";

        private const string NestedSequenceYaml = @"---
            - [1, 2, 3]
            - [4, 5, 6]";

        private const string MappingYaml = @"---
            receipt:    Oz-Ware Purchase Invoice
            date:        2007-08-06
            customer:
                given:   Dorothy
                family:  Gale

            items:
                - part_no:   A4786
                  descrip:   Water Bucket (Filled)
                  price:     1.47
                  quantity:  4

                - part_no:   E1628
                  descrip:   High Heeled ""Ruby"" Slippers
                  price:     100.27
                  quantity:  1

            bill-to:  &id001
                street: |
                        123 Tornado Alley
                        Suite 16
                city:   East Westville
                state:  KS

            ship-to:  *id001

            specialDelivery:  >
                Follow the Yellow Brick
                Road to the Emerald City.
                Pay no attention to the
                man behind the curtain.
...";
    }
}
