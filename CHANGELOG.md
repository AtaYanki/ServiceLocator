# Changelog

All notable changes to the Service Locator package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.0] - 2025-11-24

### 🎉 Initial Release

The first public release of Service Locator - a flexible and powerful dependency injection system for Unity.

### ✨ Features

#### Core System
- **Global & Scene Services** - Register services globally or per-scene with automatic lifecycle management
- **Hierarchical Resolution** - Services resolve through parent hierarchy, scene, and global locators automatically
- **Type-Safe Registration** - Strongly-typed service registration and retrieval with compile-time safety
- **Zero Configuration** - Works out of the box with minimal setup required

#### Service Management
- **Service Registration** - Register services by interface or concrete type
- **Service Resolution** - Automatic resolution through hierarchy (parent → scene → global)
- **Multiple Registration Methods** - Support for standard, updatable, fixed updatable, late updatable, and destroyable services

#### Update Integration
- **IUpdatable Interface** - Register services that need `Update()` callbacks
- **IFixedUpdatable Interface** - Register services that need `FixedUpdate()` callbacks
- **ILateUpdatable Interface** - Register services that need `LateUpdate()` callbacks
- **Automatic Update Management** - UpdateManager handles all update callbacks efficiently

#### Lifecycle Management
- **IDestroyable Interface** - Register services that need cleanup callbacks
- **Automatic Cleanup** - DestroyManager handles cleanup when ServiceLocator is destroyed
- **Scene Lifecycle** - Automatic cleanup when scene ServiceLocators are destroyed

#### Developer Experience
- **Fluent API** - Chain multiple registrations with a clean, readable syntax
- **Editor Integration** - Quick creation via Unity menu items (GameObject → ServiceLocator)
- **Bootstrapper Pattern** - Easy service setup with Global and Scene bootstrappers
- **Extension Methods** - Utility extensions for safer null handling

#### Components Added
- `ServiceLocator` - Main ServiceLocator component with global and scene support
- `ServiceLocatorGlobalBootstrapper` - Bootstrapper for global services
- `ServiceLocatorSceneBootstrapper` - Bootstrapper for scene-specific services
- `Bootstrapper` - Abstract base class for custom bootstrappers
- `ServiceManager` - Core service registry and retrieval system
- `UpdateManager` - Manages update callbacks for registered services
- `DestroyManager` - Handles lifecycle cleanup for registered services
- `ServiceLocatorExtensions` - Utility extension methods

#### Interfaces
- `IUpdatable` - Interface for services requiring Update() callbacks
- `IFixedUpdatable` - Interface for services requiring FixedUpdate() callbacks
- `ILateUpdatable` - Interface for services requiring LateUpdate() callbacks
- `IDestroyable` - Interface for services requiring cleanup callbacks

### 📚 Documentation
- Comprehensive README with installation guide
- Quick Start tutorial with complete code examples
- API reference for all public classes and methods
- Best practices guide for service registration and resolution
- Troubleshooting section for common issues
- Sample scripts demonstrating usage patterns

### 🧪 Samples
- **Sample Services** - Example service interfaces and implementations
- **Sample Bootstrappers** - Global and scene bootstrapper examples
- **Sample Consumers** - Example MonoBehaviour components consuming services
- Complete demonstration of all features

### 🔧 Technical Details
- **Minimum Unity Version:** Unity 6000.0
- **Dependencies:** None (pure C# implementation)
- **Platform Support:** All Unity-supported platforms
- **API Level:** .NET Standard 2.1

### 📦 Package Structure
```
Runtime/
  ├── ServiceLocator.cs              # Main ServiceLocator component
  ├── Bootstrapper.cs                # Base bootstrapper classes
  ├── ServiceManager.cs              # Core service registry
  ├── UpdateManager.cs               # Update callback management
  ├── DestroyManager.cs              # Lifecycle cleanup management
  └── ServiceLocatorExtensions.cs   # Utility extensions

Editor/
  └── ServiceLocatorMenuItems.cs    # Unity menu integration

Samples/
  ├── Scripts/
  │   ├── SampleServices.cs         # Example service interfaces/implementations
  │   ├── SampleBootstrapper.cs     # Example bootstrapper
  │   └── SampleConsumer.cs         # Example service consumer
```

### 🎯 Performance Characteristics
- **Memory:** Minimal overhead - services stored in dictionaries
- **CPU:** Efficient - O(1) service lookup
- **GC:** Minimal allocations during normal operations
- **Resolution:** Fast hierarchical resolution with caching

---

## Roadmap

### Future Considerations
- Service factory pattern support
- Dependency injection with constructor parameters
- Service lifetime management (singleton, transient, scoped)
- Service validation and dependency checking
- Performance profiling tools
- Editor debugging tools

---

## Support

For bug reports, feature requests, or questions:
- **Issues:** https://github.com/AtaYanki/ServiceLocator/issues
- **Discussions:** https://github.com/AtaYanki/ServiceLocator/discussions

---

**Note:** This changelog follows the [Keep a Changelog](https://keepachangelog.com/) format. Each version will document:
- `Added` for new features
- `Changed` for changes in existing functionality
- `Deprecated` for soon-to-be removed features
- `Removed` for removed features
- `Fixed` for bug fixes
- `Security` for vulnerability fixes
