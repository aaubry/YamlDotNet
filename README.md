# YamlDotNet

| Travis | Appveyor | NuGet |
|--------|----------|-------|
|[![Travis CI](https://travis-ci.org/aaubry/YamlDotNet.svg?branch=master)](https://travis-ci.org/aaubry/YamlDotNet/builds#)|[![Build status](https://ci.appveyor.com/api/projects/status/github/aaubry/yamldotnet?svg=true)](https://ci.appveyor.com/project/aaubry/yamldotnet/branch/master)|  [![NuGet](https://img.shields.io/nuget/v/YamlDotNet.svg)](https://www.nuget.org/packages/YamlDotNet/)


YamlDotNet is a YAML library for [netstandard and other .NET runtimes](#the-yamldotnet-library).

YamlDotNet provides low level parsing and emitting of YAML as well as a high level object model similar to XmlDocument. A serialization library is also included that allows to read and write objects from and to YAML streams.

YamlDotNet's conformance with YAML specifications:

|            YAML Spec                | YDN Parser | YDN Emitter |
|:-----------------------------------:|:----------:|:-----------:|
|  [v1.1](https://yaml.org/spec/1.1/)  |     ✓      |      ✓      |
|  [v1.2](https://yaml.org/spec/1.2/spec.html)  |     ✓      |             |


## What is YAML?

YAML, which stands for "YAML Ain't Markup Language", is described as "a human friendly data serialization standard for all programming languages". Like XML, it allows to represent about any kind of data in a portable, platform-independent format. Unlike XML, it is "human friendly", which means that it is easy for a human to read or produce a valid YAML document.

## The YamlDotNet library

The library has now been successfully used in multiple projects and is considered fairly stable. It is compatible with the following runtimes:

* netstandard 2.1
* netstandard 1.3
* .NET Framework 4.5
* Unity Subset v3.5

The following runtimes are also supported, with a few features missing:

* .NET Framework 3.5
* .NET Framework 2.0

The library is compatible with mono's [Ahead-of-Time compilation](https://www.mono-project.com/docs/advanced/aot/) (AOT), and should work correctly on platforms that depend on it, such as Unity.

## Quick start

Here are some quick samples to get you started.

### Deserialization from a string

```c#
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
...

string yaml;
//
var deserializer = new DeserializerBuilder()
    .WithNamingConvention(CamelCaseNamingConvention.Instance)
    .Build();

var order = deserializer.Deserialize<Order>(yaml);
```

### Serialisation to a string

```c#
using YamlDotNet.RepresentationModel;
...

var objectToSerialize;
...
var serializer = new SerializerBuilder().Build();
//yaml contains a string containing your YAML
var yaml = serializer.Serialize(receipt);
```

## More information

More information can be found in the [project's wiki](https://github.com/aaubry/YamlDotNet/wiki).

## Installing

Just install the [YamlDotNet NuGet package](http://www.nuget.org/packages/YamlDotNet/):

```
PM> Install-Package YamlDotNet
```

If you do not want to use NuGet, you can [download binaries here](https://ci.appveyor.com/project/aaubry/yamldotnet).

YamlDotNet is also available on the [Unity Asset Store](https://www.assetstore.unity3d.com/en/#!/content/36292).

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## Release notes

[Release notes for the latest version](RELEASE_NOTES.md)
