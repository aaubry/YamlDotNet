# YamlDotNet

YamlDotNet is a .NET library for YAML. YamlDotNet provides low level parsing and emitting of YAML as well as a high level object model similar to XmlDocument. A serialization library is also included that allows to read and write objects from and to YAML streams.

## What is YAML?

YAML, which stands for "YAML Ain't Markup Language", is described as "a human friendly data serialization standard for all programming languages". Like XML, it allows to represent about any kind of data in a portable, platform-independent format. Unlike XML, it is "human friendly", which means that it is easy for a human to read or produce a valid YAML document.

## The YamlDotNet library

The library has now been successfully used in multiple projects and is considered fairly stable.

## Where to get it?

The most up-to-date version can always be found in the following NuGet packages:

* [http://www.nuget.org/packages/YamlDotNet](http://www.nuget.org/packages/YamlDotNet)

# Changelog

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
