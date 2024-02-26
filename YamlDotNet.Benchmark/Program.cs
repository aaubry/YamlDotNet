using YamlDotNet.Serialization;

var serializer = new SerializerBuilder()
    .WithQuotingNecessaryStrings()
    .Build();

var s = "\t, something";
var yaml = serializer.Serialize(s);
Console.WriteLine(yaml);
var deserializer = new DeserializerBuilder().Build();
var value = deserializer.Deserialize(yaml);
