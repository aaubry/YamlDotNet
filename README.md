# YamlDotNet

| Travis | Appveyor | NuGet |
|--------|----------|-------|
|[![Travis CI](https://travis-ci.org/aaubry/YamlDotNet.svg?branch=master)](https://travis-ci.org/aaubry/YamlDotNet/builds#)|[![Build status](https://ci.appveyor.com/api/projects/status/github/aaubry/yamldotnet?svg=true)](https://ci.appveyor.com/project/aaubry/yamldotnet/branch/master)|  [![NuGet](https://img.shields.io/nuget/v/YamlDotNet.svg)](https://www.nuget.org/packages/YamlDotNet/)


YamlDotNet is a .NET library for YAML. YamlDotNet provides low level parsing and emitting of YAML as well as a high level object model similar to XmlDocument. A serialization library is also included that allows to read and write objects from and to YAML streams.

Currently, YamlDotNet supports [version 1.1 of the YAML specification](http://yaml.org/spec/1.1/).

## What is YAML?

YAML, which stands for "YAML Ain't Markup Language", is described as "a human friendly data serialization standard for all programming languages". Like XML, it allows to represent about any kind of data in a portable, platform-independent format. Unlike XML, it is "human friendly", which means that it is easy for a human to read or produce a valid YAML document.

## The YamlDotNet library

The library has now been successfully used in multiple projects and is considered fairly stable.

## More information

More information can be found in the [project's wiki](https://github.com/aaubry/YamlDotNet/wiki).

## Installing

Just install the [YamlDotNet NuGet package](http://www.nuget.org/packages/YamlDotNet/):

```
PM> Install-Package YamlDotNet
```

If you need signed assemblies, install the [YamlDotNet.Signed NuGet package](http://www.nuget.org/packages/YamlDotNet.Signed/) instead:

```
PM> Install-Package YamlDotNet.Signed
```

If you do not want to use NuGet, you can [download binaries here](https://ci.appveyor.com/project/aaubry/yamldotnet).

YamlDotNet is also available on the [Unity Asset Store](https://www.assetstore.unity3d.com/en/#!/content/36292).

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

# Changelog

## Version 4.2.1

Bug fixes:

* **Fix parser behavior when skipComments == false**  
  In most cases, the parser failed to parse after encountering a comment.

## Version 4.2.0

### New features

* **Support for .NET Core (`netstandard1.3`).**  
  The project files have been converted to the new format, which means that older versions of Visual Studio may be unable to load them.

### Development-related changes

* YamlDotNet now uses [Cake](http://cakebuild.net/) to define the build script. Previously, custom powershell scripts were used.

* [Docker](https://www.docker.com/) images are now available with everything that is required to build YamlDotNet, both [locally](https://hub.docker.com/r/aaubry/yamldotnet.local/), and on [Travis](https://hub.docker.com/r/aaubry/yamldotnet/). This is mostly useful for people wanting to develop on Linux, as it can be tricky to install the correct versions of the dependencies.

* **Code samples are now part of the solution**  
  They are specified as tests, and the [samples](https://github.com/aaubry/YamlDotNet/wiki/Samples) section of the wiki is generated from their source code and their output.

## Version 4.1.0

### New features

* 32bits Unicode code points in escape sequences and url-encoded tags [are now properly handled](https://github.com/aaubry/YamlDotNet/pull/219).
* [Anchors can now be redefined](https://github.com/aaubry/YamlDotNet/pull/222) in a document.  
  This is to conform to [the 1.1 spec](http://yaml.org/spec/1.1/#id863390) as well as [the 1.2 spec](http://www.yaml.org/spec/1.2/spec.html#id2765878):
  
  > #### 3.2.2.2. Anchors and Aliases
  > When composing a representation graph from serialized events, an alias node refers to the most recent node in the serialization having the specified anchor. Therefore, anchors need not be unique within a serialization.
* Added support for [tag mappings on the serializer](https://github.com/aaubry/YamlDotNet/pull/229).  
  Use `SerializerBuilder.WithTagMapping()` to register a new tag mapping on the serializer.
* Allow to [unregister components](https://github.com/aaubry/YamlDotNet/commit/43c18ecf482dd069784a2031d8d56c1fa3a81734) from the SerializerBuilder and DeserializerBuilder.  
  Use the `Without...` methods on `SerializerBuilder` and `DeserializerBuilder` for that.
* New [`DateTimeConverter`](https://github.com/aaubry/YamlDotNet/pull/234)  
  * It accepts [`DateTimeKind.Utc`](https://msdn.microsoft.com/en-us/library/shx7s921(v=vs.110).aspx) and [Standard Date and Time Format Strings](https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.110).aspx) of "G" as its default parameters, if they are omitted.
  * For deserialisation, it accepts as many number of formats as we want. If a value doesn't match against provided formats, it will return [`FormatException`](https://msdn.microsoft.com/en-us/library/system.formatexception(v=vs.110).aspx). Please refer to my whole test cases.
  * For serialisation, it only considers the first format in the format list.
* Improve the (de)serializer builders so that it is possible to [wrap existing component registrations](https://github.com/aaubry/YamlDotNet/commit/204ff4979dca7e2cfcbe86cd1387bf6b6f2398c3).
* Added the `ApplyNamingConventions` property to `YamlMemberAttribute`.  
  When this property is true, naming conventions are not applied to the associated member. This solves [issue 228](https://github.com/aaubry/YamlDotNet/issues/228).

### Bug fixes

* Fixed [issue 189](https://github.com/aaubry/YamlDotNet/pull/205): extra '\0' after indentation indicators.
* Fixed [some issues](https://github.com/aaubry/YamlDotNet/commit/a662fe53e9fd351eee7eef195e617023fd4e1cd3) related to parsing and emitting comments.
* Fixed [deserialization of ulongs](https://github.com/aaubry/YamlDotNet/commit/62bcaacf5873f9fd94385a20200028595a1381a7) greater than long.MaxValue.
* Fixed [issue 218](https://github.com/aaubry/YamlDotNet/issues/218): Objects with custom type converters are traversed.
* [Avoid crashing with a StackOverflowException](https://github.com/aaubry/YamlDotNet/pull/223) when iterating over the AllNodes property when it's infinitely recursive.

### Other

* The samples have been added to the project as a new unit test project, to ensure that they stay up-to-date with the code.
  In the future, a documentation page will be generated from the samples, that will show the sample, its documentation and respective output.

## Version 4.0.0

This a major release that introduces a few breaking changes.

### Breaking changes

* **The constructors of `Serializer` and `Deserializer` are now obsolete**  
  Except for the parameterless versions. The `SerializerBuilder` and `DeserializerBuilder`
  classes should now be used to configure and create instances of the (de)serializer.

* **Replaced the `IYamlSerializable` interface with `IYamlConvertible`**  
  The `IYamlSerializable` is now obsolete, but will be kept until the next major release.

* **[Removed](https://github.com/aaubry/YamlDotNet/pull/203) `EventReader`**  
  `EventReader` was a wrapper over `IParser` that offered some abstractions for parsing,
  but also had some design flaws. It has been replaced by extension methods for `IParser`.
  The extension methods provide the same functionality,
  and allow to always use the same type to represent the parser.

* **Dropped support for `YamlAliasAttribute`**  
  This class has been obsolete for many releases, and it was time to let it go.

### New features

* [**`SerializerBuilder` and `DeserializerBuilder`**](https://github.com/aaubry/YamlDotNet/pull/204)  
  This is an important change that adds "builders" that can be used
  to configure the Serializer and Deserializer through a fluent syntax.
  The main objective of this is to allow more control over
  the composition of services performed by these two classes.
  This means that every aspect of the composition should be
  extensible / overridable. Things like injecting a custom TypeInspector
  or replacing the the default ArrayNodeDeserializer with
  an alternative implementation become possible and easy.  
  In order to avoid breaking existing code,
  the constructors of Serializer and Deserializer have been kept
  but marked as obsolete. In a future release they will be discarded.

* **Added the `IYamlConvertible` interface**  
  This new interface differs in that its methods receive a delegate that can be used
  to reuse the current serializer or deserializer.

* **Improved the usability of `YamlDocument`**  
  and other RepresentationModel classes:
  * Added conversion operators and indexers for easier parsing and construction of YamlNodes.
  * `YamlMappingNode`, `YamlSequenceNode` and `YamlScalarNode` now implement `IYamlConvertible`,
    which means that these types can appear in the middle of an object that is being serialized or
    deserialized, and produce the expected result.

* [**Added support for alternative Boolean values**](https://github.com/aaubry/YamlDotNet/pull/183)
  * True: `true`, `y`, `yes`, `on`
  * False: `false`, `n`, `no`, `off`.


### Bug fixes

* [Serialization Error when string starts with quote](https://github.com/aaubry/YamlDotNet/issues/135)
* [YamlVisitor is marked as obsolete, but no YamlVisitorBase class exists](https://github.com/aaubry/YamlDotNet/issues/200)
* Do not assign anchors to scalars during serialization.

## Version 3.9.0

New features:

* Add YamlVisitorBase as an improved replacement for YamlVisitor
  * **YamlVisitor is now obsolete**, and will be removed in a future release.
* Ensure compatibility with AOT compilation, for platforms that do not allow dynamic code generation, such as IOS or PS4.
* Add Yaml attribute overrides feature, similar to XML Serializer attribute overrides behavior.
* Add a YamlNodeType enumeration property to nodes.

Bug fixes:

* Fix #166 - Guid conversion to JSON is unquoted.
* Ignore enum value case during deserialization.
* Improve newline handling
  * In some cases, consecutive newlines were incorrectly parsed or emitted.
* Fix #177 - double.MaxValue serialization.
* Register custom type converters with higher precedence than the built-in converters.

## Version 3.8.0

New features:

* **Add support for different scalar integer bases.**  
  Addresses issue [#113](https://github.com/aaubry/YamlDotNet/issues/113). Adds basic support for deserializing scalar integers
  written in binary, octal, decimal, hex, and base 60, as allowed in the YAML
  specification; see http://yaml.org/type/int.html. Adds unit tests for each
  of these bases as well.
* **Add dnx compatibility to the NuGet packages.**
* Do not throw exception if a tag does not contain a valid type name.

Fixes and improvements:

* Cache type metadata.
* Fix wrong type when deserializing UInt16.
* Fix handling of special float values, such as NaN, PositiveInfinity and NegativeInfinity.
* Properly quote empty strings.
* Properly handle non-Unicode encodings when emitting scalars.

## Version 3.7.0

This is a minor update that simply adds an overload of YamlStream.Load to be able to specify the EventReader.

## Version 3.6.1

Bug fixes:

  * Bug in the GetPublicMethods implementation for portable.

## Version 3.6.0

New features:

  * Ability to opt out of anchor assignment during `YamlStream.Save()`.
  * Allow the style of scalar properties to be specified through the `YamlMember` attribute.
  * Add solution configuration to target "Unity 3.5 .net Subset Base Class Libraries".

Bug fixes:

  * Do not compare nodes by value while assigning anchors. It is the responsibility of the user to use the same reference if they want an alias.
  * Fixed #121: Finding properties in parent interfaces

## Version 3.5.1

Fix bug:

* Scalars returned by the scanner do not have their Start and End properties set.

## Version 3.5.0

* Add native support of System.Guid serialization.
* Add properties to YamlMemberAttribute:
    * Order: specifies the order of the members when they are serialized.
    * Alias: instructs the deserializer to use a different field name for serialization.
* The YamlAliasAttribute is now obsolete. New code should use YamlMemberAttribute instead.
* Throw proper exceptions, with correct marks, when deserialization of a node fails.

## Version 3.4.0

Changes and fixes on the Scanner to make it more usable:

* Report the location of comments correctly, when the scanner is created with "skipComments = false"
* In case of syntax error, do not report an empty range and skip to the next token.
* Make the scanner and related types serializable, so that the state of the scanner can be captured and then restored later (assuming that the TextReader is also serializable).

## Version 3.3.1

This release adds a signed package and portable versions of the library.

## Version 3.3.0

* Make types in YamlDotNet.RepresentationModel serializable.

## Version 3.2.1

* Fix AnchorNotFoundException when another exception occurs during deserialization.

## Version 3.2.0

This release adds merge key support: http://yaml.org/type/merge.html

Example from BackreferencesAreMergedWithMappings unit test:

```C#
var reader = new EventReader(new MergingParser(new Parser(stream)));
var result = Deserializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(parser);
```

## Version 3.1.1

This is a bugfix release that fixes issue #90.

## Version 3.1.0

* Add a parameter to the deserializer to ignore unmapped properties in YAML.

## Version 3.0.0

* Fix issue #26: Use the actual type of the objects instead of the statically detected one.
* Merged the Core, Converters and RepresentationModel assemblies. **The NuGet packages YamlDotNet.Core and YamlDotNet.RepresentationModel are now a single package, named YamlDotNet**.
* Removed YamlDotNet.Configuration and YamlDotNet.Converters.
* Line numbers in error messages now start at one.
* TypeConverter is now used to cast list items.
* Various code improvements.
* More and better unit tests.

## Version 2.2.0

TODO

## Version 2.1.0

TODO

## Version 2.0.0

* YamlSerializer has been replaced by the Deserializer class. It offer the same functionality of YamlSerializer but is easier to maintain and extend.
  * **Breaking change:** DeserializationOverrides is no longer supported. If you need this, please file a bug and we will analyze it.
  * **Breaking change:** IDeserializationContext is no longer supported. If you need this, please file a bug and we will analyze it.
  * Tag mappings are registered directly on the Deserializer using RegisterTagMapping()
  * ObjectFactory is specified in the constructor, if required.

* Bug fixes to the Serializer:
  * Fix bug when serializing lists with nulls inside. e9019d5f224f266e88d9882502f83f0c6865ec24

* Adds a YAML editor add-in for Visual Studio 2012. Available on the [Visual Studio Gallery](http://visualstudiogallery.msdn.microsoft.com/34423c06-f756-4721-8394-bc3d23b91ca7).
