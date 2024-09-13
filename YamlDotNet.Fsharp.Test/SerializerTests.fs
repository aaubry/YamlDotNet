module SerializerTests

open System
open Xunit
open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions
open YamlDotNet.Core
open YamlDotNet.Fsharp.Test

[<CLIMutable>]
type Spec =
    {
        EngineType: string
        DriveType: string
    }

[<CLIMutable>]
type Car =
    {
        Name: string
        Year: int
        Spec: Spec option
        Nickname: string option
    }

[<CLIMutable>]
type Person =
    {
        Name: string
        MomentOfBirth: DateTime
        KidsSeat: int option
        Cars: Car array
    }

[<Fact>]
let Serialize_YamlWithScalarOptions () =
    let jackTheDriver =
        {
            Name = "Jack"
            MomentOfBirth = DateTime(1983, 4, 21, 20, 21, 03, 4)
            KidsSeat = Some 1
            Cars =
                [|
                    {
                        Name = "Mercedes"
                        Year = 2018
                        Nickname = Some "Jessy"
                        Spec = None
                    }
                    {
                        Name = "Honda"
                        Year = 2021
                        Nickname = None
                        Spec = None
                    }
                |]
        }

    let yaml =
        """name: Jack
momentOfBirth: 1983-04-21T20:21:03.0040000
kidsSeat: 1
cars:
- name: Mercedes
  year: 2018
  spec: 
  nickname: Jessy
- name: Honda
  year: 2021
  spec: 
  nickname: 
"""

    let sut =
        SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build()

    let person = sut.Serialize(jackTheDriver)
    Assert.Equal(yaml.Clean(), person.Clean())

[<Fact>]
let Serialize_YamlWithScalarOptions_OmitNull () =
    let jackTheDriver =
        {
            Name = "Jack"
            MomentOfBirth = DateTime(1983, 4, 21, 20, 21, 03, 4)
            KidsSeat = Some 1
            Cars =
                [|
                    {
                        Name = "Mercedes"
                        Year = 2018
                        Nickname = Some "Jessy"
                        Spec = None
                    }
                    {
                        Name = "Honda"
                        Year = 2021
                        Nickname = None
                        Spec = None
                    }
                |]
        }

    let yaml =
        """name: Jack
momentOfBirth: 1983-04-21T20:21:03.0040000
kidsSeat: 1
cars:
- name: Mercedes
  year: 2018
  nickname: Jessy
- name: Honda
  year: 2021
"""

    let sut =
        SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build()

    let person = sut.Serialize(jackTheDriver)
    Assert.Equal(yaml.Clean(), person.Clean())

[<Fact>]
let Serialize_YamlWithObjectOptions_OmitNull () =
    let jackTheDriver =
        {
            Name = "Jack"
            MomentOfBirth = DateTime(1983, 4, 21, 20, 21, 03, 4)
            KidsSeat = Some 1
            Cars =
                [|
                    {
                        Name = "Mercedes"
                        Year = 2018
                        Nickname = None
                        Spec = Some { EngineType = "V6"; DriveType = "AWD" }
                    }
                    {
                        Name = "Honda"
                        Year = 2021
                        Nickname = None
                        Spec = None
                    }
                |]
        }

    let yaml =
        """name: Jack
momentOfBirth: 1983-04-21T20:21:03.0040000
kidsSeat: 1
cars:
- name: Mercedes
  year: 2018
  spec:
    engineType: V6
    driveType: AWD
- name: Honda
  year: 2021
"""

    let sut =
        SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build()

    let person = sut.Serialize(jackTheDriver)
    Assert.Equal(yaml.Clean(), person.Clean())

[<CLIMutable>]
[<NoComparison>]
type TestSeq = { name: string; numbers: int seq }

[<Fact>]
let Serialize_YamlSeq () =
    let jackTheDriver =
        {
            name = "Jack"
            numbers = [ 12; 2; 2 ]
        }

    let yaml =
        """name: Jack
numbers:
- 12
- 2
- 2
"""

    let sut =
        SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build()

    let person = sut.Serialize(jackTheDriver)
    Assert.Equal(yaml.Clean(), person.Clean())

[<CLIMutable>]
type TestList = { name: string; numbers: int list }

[<Fact>]
let Serialize_YamlList () =
    let jackTheDriver =
        {
            name = "Jack"
            numbers = [ 12; 2; 2 ]
        }

    let yaml =
        """name: Jack
numbers:
- 12
- 2
- 2
"""

    let sut =
        SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build()

    let person = sut.Serialize(jackTheDriver)
    Assert.Equal(yaml.Clean(), person.Clean())

[<CLIMutable>]
type TestArray = { name: string; numbers: int array }

[<Fact>]
let Serialize_YamlArray () =
    let jackTheDriver =
        {
            name = "Jack"
            numbers = [| 12; 2; 2 |]
        }

    let yaml =
        """name: Jack
numbers:
- 12
- 2
- 2
"""

    let sut =
        SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build()

    let person = sut.Serialize(jackTheDriver)
    Assert.Equal(yaml.Clean(), person.Clean())
