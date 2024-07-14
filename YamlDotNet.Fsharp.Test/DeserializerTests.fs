module DeserializerTests

open System
open Xunit
open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions
open System.ComponentModel

[<CLIMutable>]
type Spec = {
    EngineType: string
    DriveType: string
}

[<CLIMutable>]
type Car = {
    Name: string
    Year: int
    Spec: Spec option
    Nickname: string option
}

[<CLIMutable>]
type Person = {
    Name: string
    MomentOfBirth: DateTime
    Cars: Car array
}

[<Fact>]
let Deserialize_YamlWithScalarOptions() =
    let yaml = """
name: Jack
momentOfBirth: 1983-04-21T20:21:03.0041599Z
cars:
- name: Mercedes
  year: 2018
  nickname: Jessy
- name: Honda
  year: 2021
"""
    let sut = DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()

    let person = sut.Deserialize<Person>(yaml)
    Assert.Equal("Jack", person.Name)
    Assert.Equal(2, person.Cars.Length)
    Assert.Equal("Mercedes", person.Cars[0].Name)
    Assert.Equal(Some "Jessy", person.Cars[0].Nickname)// |> should equal (Some "Jessy")
    Assert.Equal("Honda", person.Cars[1].Name)
    Assert.Equal(None, person.Cars[1].Nickname)

[<Fact>]
let Deserialize_YamlWithObjectOptions() =
    let yaml = """
name: Jack
momentOfBirth: 1983-04-21T20:21:03.0041599Z
cars:
- name: Mercedes
  year: 2018
  spec:
    engineType: V6
    driveType: AWD
- name: Honda
  year: 2021
"""
    let sut = DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()

    let person = sut.Deserialize<Person>(yaml)
    Assert.Equal("Jack", person.Name)
    Assert.Equal(2, person.Cars.Length)
    
    Assert.Equal("Mercedes", person.Cars[0].Name)
    Assert.NotNull(person.Cars[0].Spec)
    Assert.True(person.Cars[0].Spec |> Option.isSome)
    Assert.Equal("V6", person.Cars[0].Spec.Value.EngineType)
    Assert.Equal("AWD", person.Cars[0].Spec.Value.DriveType)
    
    Assert.Equal("Honda", person.Cars[1].Name)
    Assert.Null(person.Cars[1].Spec)
    Assert.Equal(None, person.Cars[1].Spec)
    Assert.Equal(None, person.Cars[1].Nickname)
