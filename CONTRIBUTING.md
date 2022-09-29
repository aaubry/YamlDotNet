# Contributing to YamlDotNet

**Welcome!**  
Thanks for your interest in contributing to this project. Any contribution will
be gladly accepted, provided that they are generally useful and follow the
conventions of the project.

If you are considering a contribution, please read and follow these guidelines.

## Pull requests

All contributions should be submitted as pull requests.

1. Please create **one pull request for each feature**. This results in smaller pull requests that are easier to review and validate.

1. **Avoid reformatting existing code** unless you are making other changes to it.
   * Cleaning-up of `using`s is acceptable, if you made other changes to that file.
   * If you believe that some code is badly formatted and needs fixing, isolate that change in a separate pull request.

1. Always add one or more **unit tests** that prove that the feature / fix you are submitting is working correctly.

1. Please **describe the motivation** behind the pull request. Explain what was the problem / requirement. Unless the implementation is self-explanatory, also describe the solution.
   * Of course, there's no need to be too verbose. Usually one or two lines will be enough.

## Project organization

The main project, YamlDotNet.csproj, is organized in three main namespaces: `Core`, `RepresentationModel` and `Serialization`. The `Core` namespace contains everything that is related to reading and writing YAML. The `RepresentationModel` has classes that represent a YAML stream, similar to XmlDocument for XML. The `Serialization` namespace contains classes to serialize and deserialize object graphs to / from YAML.

Unit tests are all contained in the project named YamlDotNet.Test.csproj.

The PerformanceTests folder contains various projects that contain performance tests that compare various versions of YamlDotNet to detect the impact of new features.

## Building / multiplatform

This repository uses submodules. **Before building, make sure that you update them** using the following command:
```
git submodule update --init
```

In order to build locally, you need at least to install the [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download). Alternatively, you may install [Visual Studio 2019 Community](https://visualstudio.microsoft.com/vs/), which will install it for you and allow you to edit the code as well.

Building for Unity requires installing
[Visual Studio Tools for Unity](https://visualstudiogallery.msdn.microsoft.com/20b80b8c-659b-45ef-96c1-437828fe7cf2/file/92287/8/Visual%20Studio%202013%20Tools%20for%20Unity.msi).

**TODO**: Other .NET SDKs are probably needed. Check the correct set of requirements and update this page.

This project is compatible with the standard `dotnet` tool, so building the project is as simple as running the following command:
```
dotnet build
```

You can also run the unit tests in a similar fashion:
```
dotnet test
```

### Build tool

There is a program inside `/tools/build` that orchestrates other automation tasks. This tool can be executed by calling `build.cmd <target>`, where `<target>` is one of the following:

#### `Pack`

Builds the project and produces a NuGet package.

#### `Publish`

Builds the project and publishes a NuGet package to nuget.org. In order to do so, an environment variable named `NUGET_API_KEY` must be set.

#### `Release`

If there are no release notes for the current version, generates those release notes from the git log and exits.  
Otherwise, creates a release from the current commit by performing the following:
1. Update the `RELEASE_NOTES.md` file.
2. Commit the `RELEASE_NOTES.md` and the release notes for the current version.
3. Tags the commit with the current version.

Once this is done, the tag must be pushed manually to the repository.

#### `Document`

Generates the samples documentation.

### Continuous integration

Every commit and pull request is built on [AppVeyor](https://ci.appveyor.com/project/aaubry/yamldotnet). The build definition is kept intentionally simple. All the logic is delegated to the build tool. This makes it easy to use a different CI provider, or even to run the builds manually.

### Target platforms

The project targets the following platforms:

* .NET Framework 4.7
* .NET Framework 4.5
* .NET Framework 3.5
* .NET Standard 2.1
* Unity Subset v3.5
* .NET 6.0

In the csproj, the `TargetFrameworks` element also targets the following platforms for technical reasons:

* net40: this is a hack used to target Unity. That target is overriden and in reality it targets Unity Subset v3.5.
* .NET Core 3.0: this is to benefit from nullable annotations in the BCL.

### Build configurations

There are a few differences between the various target platforms,
mainly in the reflection API. In order to adapt the code to each platform,
`#if ... #endif` sections are used. When possible, such sections should be placed
in the `Helpers/Portability.cs` file. An effective technique is to define an extension
method that is used through the code, and has different implementations depending
on the build variables.

## AOT compatibility

Some platforms - such as IOS - forbid dynamic code generation. This prevents Just-in-Time compilation (JIT) from being used. In those cases, one can use Mono's Ahead-of-Time compilation (AOT). This results on a precompiled assembly that does not rely on JIT. There are [some limitations](http://www.mono-project.com/docs/advanced/aot/#limitation-generic-interface-instantiation) however, most of them are related to usage of generics.

In order to ensure that YamlDotNet is compatible with AOT compilation, an automatic test has been created that runs on every commit. That test exercises the serializer and deserializer to help identify AOT-related problems.

## Coding style

Attempt to follow the [SOLID](https://en.wikipedia.org/wiki/SOLID_%28object-oriented_design%29) principles. In particular, try to give each type a single responsibility, and favour composition to combine features.

As long as you keep the code readable, I don't care too much about any specific coding convention. There are only a few rules that should be honoured:

* Use **4 spaces** to indent.
* Each class / interface / struct / delegate **goes to its own file**.
  * The only acceptable exception is for small and closely related types.
* Use sane indentation rules. Break long lines when needed, but don't be obsessive:
  * This is **OK**:

    ```C#
    Traverse(
        new ObjectDescriptor(
            value.Value,
            underlyingType,
            value.Type,
            value.ScalarStyle
        ),
        visitor,
        currentDepth
    );
    ```
  * This is **OK too**:

    ```C#
    Traverse(
        new ObjectDescriptor(value.Value, underlyingType, value.Type, value.ScalarStyle),
        visitor,
        currentDepth
    );
    ```
  * This is **not very good**:

    ```C#
    Traverse(new ObjectDescriptor(value.Value, underlyingType, value.Type, value.ScalarStyle), visitor, currentDepth);
    ```
  * This is **awful**:

    ```C#
    Traverse(new ObjectDescriptor(value.Value,
                                  underlyingType,
                                  value.Type,
                                  value.ScalarStyle),
             visitor,
             currentDepth);
    ```
