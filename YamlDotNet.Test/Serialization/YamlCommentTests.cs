using Xunit;
using Xunit.Abstractions;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test.Serialization
{
    public class YamlCommentTests
    {
        protected readonly ITestOutputHelper Output;
        public YamlCommentTests(ITestOutputHelper helper)
        {
            Output = helper;
        }

        [Fact]
        public void SerializationWithComment()
        {
            var person = new Person();
            person.Name = "PandaTea";
            person.Age = 100;
            person.Sex = "male";

            Serializer serializer = new Serializer();
            string result = serializer.Serialize(person);

            Output.WriteLine(result);
        }

        class Person
        {
            [YamlMember(Description = "this is a yaml comment about name property")]
            public string Name { get; set; }
            [YamlMember(Description = "this is age")]
            public int Age { get; set; }
            [YamlMember(Description = "male or female")]
            public string Sex { get; set; }
        }
    }
}
