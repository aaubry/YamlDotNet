using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.RepresentationModel;

namespace YamlDotNet.Samples
{
    public class DynamicYamlSamples
    {
        public void MappingNodeSample()
        {
            dynamic dynamicYaml = new DynamicYaml(MappingYaml);

            string receipt = (string)(dynamicYaml.Receipt);
            var firstPartNo = dynamicYaml.Items[0].part_no;
        }

        public void SequenceNodeSample()
        {
            dynamic dynamicYaml = new DynamicYaml(SequenceYaml);

            string firstName = dynamicYaml[0].name;
        }

        public void NestedSequenceNodeSample()
        {
            dynamic dynamicYaml = new DynamicYaml(NestedSequenceYaml);

            string firstNumberAsString = dynamicYaml[0, 0];
            int firstNumberAsInt = dynamicYaml[0, 0];
        }

        public void TestEnumConvert()
        {
            dynamic dynamicYaml = new DynamicYaml(EnumConvertYaml);
            
            StringComparison stringComparisonMode = dynamicYaml[0].stringComparisonMode;
        }

        private const string EnumConvertYaml = @"---
            - stringComparisonMode: CurrentCultureIgnoreCase
            - stringComparisonMode: Ordinal";

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
