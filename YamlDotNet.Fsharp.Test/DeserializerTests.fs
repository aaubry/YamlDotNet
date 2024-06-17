module DeserializerTests

open System
open Xunit
open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions
open FsUnit.Xunit
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
    person.Name |> should equal "Jack"
    person.Cars |> should haveLength 2
    person.Cars[0].Name |> should equal "Mercedes"
    person.Cars[0].Nickname |> should equal (Some "Jessy")
    person.Cars[1].Name |> should equal "Honda"
    person.Cars[1].Nickname |> should equal None


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
    person.Name |> should equal "Jack"
    person.Cars |> should haveLength 2
    
    person.Cars[0].Name |> should equal "Mercedes"
    person.Cars[0].Spec |> should not' (be null)
    person.Cars[0].Spec |> Option.isSome |> should equal true
    person.Cars[0].Spec.Value.EngineType |> should equal "V6"
    person.Cars[0].Spec.Value.DriveType |> should equal "AWD"
    
    person.Cars[1].Name |> should equal "Honda"
    person.Cars[1].Spec |> should be null
    person.Cars[1].Spec |> should equal None
    person.Cars[1].Nickname |> should equal None


[<CLIMutable>]
type TestSeq = {
  name: string
  numbers: int seq
}

[<Fact>]
let Deserialize_YamlSeq() =
    let jackTheDriver = {
        name = "Jack"
        numbers = seq { 12; 2; 2 }
    }

    let yaml = """name: Jack
numbers:
- 12
- 2
- 2
"""
    let sut = DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()

    let person = sut.Deserialize<TestSeq>(yaml)
    person.name |> should equal jackTheDriver.name
    person.numbers |> should equalSeq jackTheDriver.numbers

[<CLIMutable>]
type TestList = {
  name: string
  numbers: int list
}

[<Fact>]
let Deserialize_YamlList() =
    let jackTheDriver = {
        name = "Jack"
        numbers = [ 12; 2; 2 ]
    }

    let yaml = """name: Jack
numbers:
- 12
- 2
- 2
"""
    let sut = DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()

    let person = sut.Deserialize<TestList>(yaml)
    person |> should equal jackTheDriver


[<CLIMutable>]
type TestArray = {
  name: string
  numbers: int array
}

[<Fact>]
let Deserialize_YamlArray() =
    let jackTheDriver = {
        name = "Jack"
        numbers = [| 12; 2; 2 |]
    }

    let yaml = """name: Jack
numbers:
- 12
- 2
- 2
"""
    let sut = DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()

    let person = sut.Deserialize<TestArray>(yaml)
    person |> should equal jackTheDriver
