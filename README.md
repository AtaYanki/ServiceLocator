# Service Locator

A flexible and powerful Service Locator pattern implementation for Unity. This package provides a robust dependency injection system with support for global, scene-specific, and hierarchical service resolution, making it easy to manage dependencies across your Unity project.

---

## 🚀 Features

* **Global & Scene Services** – Register services globally or per-scene with automatic lifecycle management
* **Hierarchical Resolution** – Services resolve through parent hierarchy, scene, and global locators automatically
* **Update Integration** – Register services that need `Update()`, `FixedUpdate()`, or `LateUpdate()` callbacks
* **Lifecycle Management** – Automatic cleanup with `IDestroyable` interface support
* **Type-Safe** – Strongly-typed service registration and retrieval
* **Zero Configuration** – Works out of the box with minimal setup
* **Editor Integration** – Quick creation via Unity menu items

---

## 📦 Installation

### Unity Package Manager (Git URL)

1. Open **Window → Package Manager**
2. Click the **+** button → **Add package from git URL**
3. Enter the repository URL:

```
https://github.com/AtaYanki/ServiceLocator.git
```

4. Wait for Unity to download and import the package

**Requirements:**
- Unity 6000.0 or newer

---

## 🛠️ How It Works

The Service Locator pattern provides a centralized registry for dependencies, allowing components to request services without knowing their concrete implementations. This package implements three levels of service resolution:

1. **Parent Hierarchy** – Checks parent GameObjects for ServiceLocator components
2. **Scene Level** – Falls back to scene-specific ServiceLocator
3. **Global Level** – Finally checks the global ServiceLocator

This hierarchy ensures services are resolved from the most specific to the most general context.

---

## 📘 Quick Start

### 1. Create a Service Interface and Implementation

```csharp
public interface IAudioService
{
    void PlaySound(string soundName);
}

public class AudioService : IAudioService
{
    public void PlaySound(string soundName)
    {
        Debug.Log($"Playing sound: {soundName}");
    }
}
```

### 2. Register Services

Create a bootstrapper to register your services. You can use either a Global or Scene ServiceLocator:

**Global ServiceLocator** (persists across scenes):

```csharp
using AtaYanki.ServiceLocator;
using UnityEngine;

public class GlobalBootstrapper : ServiceLocatorGlobalBootstrapper
{
    protected override void Bootstrap()
    {
        base.Bootstrap(); // Important: Call base to configure as global
        
        // Register services
        ServiceLocator
            .Register<IAudioService>(new AudioService())
            .Register<ISaveService>(new SaveService());
    }
}
```

**Scene ServiceLocator** (per-scene):

```csharp
using AtaYanki.ServiceLocator;
using UnityEngine;

public class SceneBootstrapper : ServiceLocatorSceneBootstrapper
{
    protected override void Bootstrap()
    {
        base.Bootstrap(); // Important: Call base to configure for scene
        
        // Register scene-specific services
        ServiceLocator.Register<IGameplayService>(new GameplayService());
    }
}
```

### 3. Retrieve Services

```csharp
using AtaYanki.ServiceLocator;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private IAudioService _audioService;
    
    void Start()
    {
        // Automatically resolves from hierarchy → scene → global
        ServiceLocator.For(this).Get(out _audioService);
    }
    
    void OnJump()
    {
        _audioService?.PlaySound("jump");
    }
}
```

---

## 🎯 Advanced Usage

### Registering Updatable Services

Services can implement `IUpdatable`, `IFixedUpdatable`, or `ILateUpdatable` to receive Unity update callbacks:

```csharp
public class GameTimer : IUpdatable
{
    private float _time;
    
    public void Update()
    {
        _time += Time.deltaTime;
    }
    
    public float GetTime() => _time;
}

// Register as updatable
ServiceLocator.Global.RegisterUpdatable<GameTimer>(new GameTimer());
```

### Registering Destroyable Services

Services can implement `IDestroyable` to receive cleanup callbacks when the ServiceLocator is destroyed:

```csharp
public class NetworkManager : IDestroyable
{
    public void Destroy()
    {
        // Cleanup network connections
        Disconnect();
    }
}

// Register as destroyable
ServiceLocator.Global.RegisterDestroyable<NetworkManager>(new NetworkManager());
```

### Direct Global Access

For services that should always be global:

```csharp
// Register globally
ServiceLocator.Global.Register<IAudioService>(new AudioService());

// Retrieve globally
ServiceLocator.Global.Get<IAudioService>(out IAudioService audioService);
```

### Scene-Specific Services

```csharp
// Get the ServiceLocator for a specific scene
ServiceLocator sceneLocator = ServiceLocator.ForSceneOf(this);

// Register scene-specific service
sceneLocator.Register<IGameplayService>(new GameplayService());
```

### Chaining Registration

The fluent API allows chaining multiple registrations:

```csharp
ServiceLocator.Global
    .Register<IAudioService>(new AudioService())
    .Register<ISaveService>(new SaveService())
    .RegisterUpdatable<GameTimer>(new GameTimer())
    .RegisterDestroyable<NetworkManager>(new NetworkManager());
```

---

## 📂 Package Structure

```
Runtime/
  ├── ServiceLocator.cs              # Main ServiceLocator component
  ├── Bootstrapper.cs                # Base bootstrapper classes
  ├── ServiceManager.cs              # Core service registry
  ├── UpdateManager.cs               # Update callback management
  ├── DestroyManager.cs              # Lifecycle cleanup management
  └── ServiceLocatorExtensions.cs   # Utility extensions

Samples/
  ├── Scripts/
  │   ├── SampleServices.cs         # Example service interfaces/implementations
  │   ├── SampleBootstrapper.cs     # Example bootstrapper
  │   └── SampleConsumer.cs         # Example service consumer
```

---

## ⚙️ API Reference

### ServiceLocator

**Static Properties:**
- `static ServiceLocator Global { get; }` – Access the global ServiceLocator instance

**Static Methods:**
- `static ServiceLocator For(MonoBehaviour monoBehaviour)` – Get ServiceLocator for a MonoBehaviour (hierarchy → scene → global)
- `static ServiceLocator ForSceneOf(MonoBehaviour monoBehaviour)` – Get ServiceLocator for a specific scene

**Instance Methods:**
- `ServiceLocator Register<T>(T service)` – Register a service by type
- `ServiceLocator Register(Type type, object service)` – Register a service with explicit type
- `ServiceLocator RegisterUpdatable<T>(T service)` – Register a service that implements `IUpdatable`
- `ServiceLocator RegisterFixedUpdatable<T>(T service)` – Register a service that implements `IFixedUpdatable`
- `ServiceLocator RegisterLateUpdatable<T>(T service)` – Register a service that implements `ILateUpdatable`
- `ServiceLocator RegisterDestroyable<T>(T service)` – Register a service that implements `IDestroyable`
- `ServiceLocator Get<T>(out T service)` – Retrieve a service (throws if not found)

### Interfaces

- `IUpdatable` – Implement to receive `Update()` callbacks
- `IFixedUpdatable` – Implement to receive `FixedUpdate()` callbacks
- `ILateUpdatable` – Implement to receive `LateUpdate()` callbacks
- `IDestroyable` – Implement to receive cleanup callbacks on ServiceLocator destruction

### Bootstrappers

- `ServiceLocatorGlobalBootstrapper` – Base class for global service registration
- `ServiceLocatorSceneBootstrapper` – Base class for scene-specific service registration

---

## 🎨 Best Practices

### Service Registration

1. **Register in Bootstrappers** – Always register services in bootstrapper classes, not in individual components
2. **Use Interfaces** – Register services by interface, not concrete types, for better decoupling
3. **Global vs Scene** – Use global services for cross-scene dependencies (audio, save system), scene services for gameplay-specific logic

### Service Resolution

1. **Use `ServiceLocator.For(this)`** – This automatically resolves through the hierarchy
2. **Cache References** – Store service references in fields rather than retrieving repeatedly
3. **Null Checks** – Always check if services are null before use

### Lifecycle Management

1. **Implement IDestroyable** – For services that need cleanup (network connections, file handles)
2. **Use Update Interfaces** – Only implement update interfaces if your service truly needs Unity callbacks
3. **Avoid Circular Dependencies** – Design services to avoid depending on each other in cycles

---

## 🧪 Included Samples

The package includes sample scripts demonstrating:

- Service interface and implementation patterns
- Global and scene bootstrapper setup
- Service consumption in MonoBehaviour components
- Update callback integration
- Lifecycle management

**To use samples:**
1. Check the `Samples/` folder in the package
2. Copy sample scripts to your project
3. Create GameObjects with bootstrapper components
4. Test service registration and retrieval

---

## 📝 Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history.

---

## 🔧 Troubleshooting

**Issue:** Service not found exception
- Ensure the service is registered before it's requested
- Check that you're using the correct ServiceLocator (global vs scene)
- Verify the service type matches exactly (including generic parameters)

**Issue:** Services not persisting across scenes
- Use `ServiceLocatorGlobalBootstrapper` for cross-scene services
- Ensure `dontDestroyOnLoad` is set to `true` on the global bootstrapper

**Issue:** Update callbacks not firing
- Verify the service implements `IUpdatable`, `IFixedUpdatable`, or `ILateUpdatable`
- Ensure the service is registered using `RegisterUpdatable`, `RegisterFixedUpdatable`, or `RegisterLateUpdatable`

**Issue:** Multiple ServiceLocators in scene
- Only one ServiceLocator per scene is recommended
- Use the hierarchy system if you need nested service resolution

---

## 📄 License

MIT License - See [LICENSE](LICENSE) for details.

---

## 🙌 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Make your changes with clear commit messages
4. Submit a pull request

**Areas for contribution:**
- Additional sample scenes
- Performance improvements
- Bug fixes
- Documentation improvements

---

## 💬 Support

**Found a bug or have a feature request?**
- Open an issue on [GitHub](https://github.com/AtaYanki/ServiceLocator/issues)
- Provide Unity version, error logs, and reproduction steps

**Need help integrating?**
- Check the included sample scripts
- Review the Quick Start guide above
- Open a discussion on GitHub

---

## 🏷️ Keywords

Unity, Dependency Injection, Service Locator, IoC, Design Patterns, Architecture, Services, Global Services, Scene Services
