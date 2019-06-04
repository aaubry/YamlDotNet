# Release notes
## Release 6.1.1

## New features

Tests from yaml-test-suite have been added to the project and an effort has been made to improve the conformance, thanks to [@am11](https://github.com/am11):

- [#395](https://github.com/aaubry/YamlDotNet/pull/395) Add spec test executor for yaml-test-suite
- [#400](https://github.com/aaubry/YamlDotNet/pull/400) Improve YAML spec conformance by three tests
- [#401](https://github.com/aaubry/YamlDotNet/pull/401) Allow scalar to have value without space (x:y)
- [#403](https://github.com/aaubry/YamlDotNet/pull/403) Relax anchor names allowed characters set
- [#404](https://github.com/aaubry/YamlDotNet/pull/404) Constrain DocumentEnd parsing to allowed tokens
- [#406](https://github.com/aaubry/YamlDotNet/pull/406) Improve omitted keys handling

### Other changes:

- Allow to save a `YamlStream` to an `IEmitter`
- Some infrastructural changes have been made to ensure that the project would build on Linux without issues.

## Bug fixes

- [#396](https://github.com/aaubry/YamlDotNet/pull/396) Fix missing string quotes around json serialized enums (fixes [#146](https://github.com/aaubry/YamlDotNet/issues/408))
- Increase the max simple key length to 1024 and allow to configure it
- Never emit key indicators in JSON

# Previous releases
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