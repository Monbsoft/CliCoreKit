# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2025-01-06

### Added

#### Generic Type-Safe API
- **Generic Methods**: Added `AddOption<T>()` and `AddArgument<T>()` for type-safe command configuration
- **Typed Getters**: Added `GetOption<T>()`, `GetArgument<T>()`, and `GetOptionValues<T>()` to `ParsedArguments`
- **Automatic Type Conversion**: Support for `string`, `int`, `double`, `bool`, and other primitive types
- **Type Display**: Help system now shows types (`<int>`, `<string>`, etc.) for options and arguments

#### Automatic Help System
- **Auto-Generated Help**: All commands now automatically support `--help` and `-h` without manual implementation
- **Metadata-Driven**: Help is generated from `Arguments` and `Options` definitions
- **Hierarchical Help**: Parent commands show child commands automatically
- **Opt-Out**: Added `DisableHelp` property and `WithoutHelp()` method for custom help handling

#### New Classes and APIs
- **`ArgumentDefinition`**: Define positional arguments with type, description, and default values
- **`CommandBuilder`**: Fluent API for configuring commands with chainable methods
- **Enhanced `CommandDefinition`**: Added `Arguments` list and `DisableHelp` property

### Changed

#### Breaking Changes (Backward Compatible)
- `AddCommand<T>()` now returns `CommandBuilder` instead of `CliHostBuilder` for fluent configuration
- Commands no longer need to manually handle `--help` unless `DisableHelp` is true

#### Improvements
- **`ParsedArguments`**: Added robust type conversion with nullable type support
- **Help Display**: Improved formatting with proper alignment and type information
- **Recursive Command Display**: Better hierarchy visualization in global help

### Enhanced
- **Boolean Options**: Automatically set `HasValue = false` for `bool` type options (flag behavior)
- **Default Values**: Display default values in help output
- **Required Indicators**: Show `(required)` for mandatory arguments and options

### Documentation
- Added comprehensive documentation for automatic help system
- Added guide for typed arguments and options with generics
- Updated README with new API examples

## [1.0.0] - 2025-01-XX

### Added
- Initial release
- POSIX, GNU, and Windows-style argument parsing
- Command registry and routing
- Middleware pipeline support
- IHostBuilder integration
- Dependency injection support
- Validation middleware
- Hierarchical command support
- Command aliases

[1.1.0]: https://github.com/monbsoft/CliCoreKit/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/monbsoft/CliCoreKit/releases/tag/v1.0.0
