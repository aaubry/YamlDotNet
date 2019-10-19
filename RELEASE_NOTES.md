# Release notes
## Release 8.0.0

## New features and improvements

- Change the default value handling behavior. Fixes #427  
  This is a **breaking change** to the default behaviour of the serializer, which will now **always emit null and default values**.  
  It is possible to configure this behaviour by using the `ConfigureDefaultValuesHandling` method on `SerializerBuilder`.

  [More details are available in the documentation.](https://github.com/aaubry/YamlDotNet/wiki/Serialization.Serializer#configuredefaultvalueshandlingdefaultvalueshandling)

- Add default implementations for the following non-generic collections to `DefaultObjectFactory`:  
  - IEnumerable  
  - ICollection  
  - IList  
  - IDictionary

- Remove obsolete and unused `SerializationOptions` enum. Fixes #438
- Throw descriptive exceptions when using the "linq" methods of `YamlNode`. Relates to #437

## Bug fixes

- Never emit document end indicator on stream end. Fixes #436
- Fix exception when deserializing an interface. Fixes #439  

# Previous releases
- [7.0.0](releases/7.0.0.md)
- [6.1.2](releases/6.1.2.md)
- [6.1.1](releases/6.1.1.md)
- [6.0.0](releases/6.0.0.md)
- [5.4.0](releases/5.4.0.md)
- [5.3.1](releases/5.3.1.md)
- [5.3.0](releases/5.3.0.md)
- [5.2.1](releases/5.2.1.md)
- [5.2.0](releases/5.2.0.md)
- [5.1.0](releases/5.1.0.md)
- [5.0.0](releases/5.0.0.md)
- [4.3.2](releases/4.3.2.md)
- [4.3.1](releases/4.3.1.md)
- [4.3.0](releases/4.3.0.md)
- [4.2.4](releases/4.2.4.md)
- [4.2.3](releases/4.2.3.md)
- [4.2.2](releases/4.2.2.md)
- [4.2.1](releases/4.2.1.md)
- [4.2.0](releases/4.2.0.md)
- [4.1.0](releases/4.1.0.md)
- [4.0.0](releases/4.0.0.md)
- [3.9.0](releases/3.9.0.md)
- [3.8.0](releases/3.8.0.md)
- [3.7.0](releases/3.7.0.md)
- [3.6.1](releases/3.6.1.md)
- [3.6.0](releases/3.6.0.md)
- [3.5.1](releases/3.5.1.md)
- [3.5.0](releases/3.5.0.md)
- [3.4.0](releases/3.4.0.md)
- [3.3.1](releases/3.3.1.md)
- [3.3.0](releases/3.3.0.md)
- [3.2.1](releases/3.2.1.md)
- [3.2.0](releases/3.2.0.md)
- [3.1.1](releases/3.1.1.md)
- [3.1.0](releases/3.1.0.md)
- [3.0.0](releases/3.0.0.md)
- [2.2.0](releases/2.2.0.md)
- [2.1.0](releases/2.1.0.md)
- [2.0.0](releases/2.0.0.md)