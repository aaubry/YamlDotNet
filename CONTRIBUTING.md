# Contributing to YamlDotNet

**Welcome!**  
Thanks for you interest in contributing to this project. Any contribution will
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
  * Of course, there's no need to too verbose. Usually one or two lines will be enough.

## Project organization

The main project, YamlDotNet.csproj, is organized in three main namespaces: `Core`, `RepresentationModel` and `Serialization`. The `Core` namespace contains everything that is related to reading and writing YAML. The `RepresentationModel` has classes that represent a YAML stream, similar to XmlDocument for XML. The `Serialization` namespace contains classes to serialize and deserialize object graphs to / from YAML.

Unit tests are all contained in the project named YamlDotNet.Test.csproj.

The PerformanceTests folder contains various projects that contain performance tests that compare various versions of YamlDotNet to detect the impact of new features.

## Building / multiplatform

This project is available on different platforms. Solution configurations are used
to select the target platform. Building for Unity requires installing
[Visual Studio Tools for Unity](https://visualstudiogallery.msdn.microsoft.com/20b80b8c-659b-45ef-96c1-437828fe7cf2/file/92287/8/Visual%20Studio%202013%20Tools%20for%20Unity.msi).

|       Configuration       |      Target       |   Defines    |           Description           |
|---------------------------|-------------------|--------------|---------------------------------|
| Debug                     | .NET 3.5          | DEBUG        | Default debug build.            |
| Release-Unsigned          | .NET 3.5          |              | Release build, not signed.      |
| Release-Signed            | .NET 3.5          | SIGNED       | Release build, signed.          |
| Release-Portable-Unsigned | .NET 4.5 portable [Profile259](http://embed.plnkr.co/03ck2dCtnJogBKHJ9EjY/preview) | PORTABLE         | Portable class library, not signed. |
| Release-Portable-Signed   | .NET 4.5 portable [Profile259](http://embed.plnkr.co/03ck2dCtnJogBKHJ9EjY/preview) | PORTABLE; SIGNED | Portable class library, signed.     |
| Debug-UnitySubset-v35     | Unity Subset v3.5 | DEBUG; UNITY | Debug build for Unity target.   |
| Release-UnitySubset-v35   | Unity Subset v3.5 | UNITY        | Release build for Unity target. |

There are a few differences between the various target platforms,
mainly in the reflection API. In order to adapt the code to each platform,
`#if ... #endif` sections are used. When possible, such sections should be placed
in the `Helpers/Portability.cs` file. An effective technique is to define an extension
method that is used tourough the code, and has different implementations depending
on the build variables.

## Coding style

Attempt to follow the [SOLID](https://en.wikipedia.org/wiki/SOLID_%28object-oriented_design%29) principles. In particular, try to give each type a single responsibility, and favor composition to combine features.

As long as you keep the code readable, I don't care too much about any specific coding convention. There are only a few rules that should be honored:

* Use **tabs** instead of spaces. The entire code base is tab-indented, and there's no value in changing.
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
