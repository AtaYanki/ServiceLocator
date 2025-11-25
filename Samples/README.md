# OmniServio Samples

This folder contains sample scripts demonstrating how to use the OmniServio package.

## 📁 Sample Files

### `Scripts/SampleServices.cs`
Contains example service interfaces and implementations:
- **Service Interfaces**: `IAudioService`, `ISaveService`, `IGameplayService`
- **Service Implementations**: `AudioService`, `SaveService`, `GameplayService`
- **Updatable Services**: `GameTimer` (IUpdatable), `PhysicsMonitor` (IFixedUpdatable), `CameraFollowService` (ILateUpdatable)
- **Destroyable Services**: `NetworkService` (IDestroyable)

### `Scripts/SampleBootstrapper.cs`
Contains example bootstrappers for registering services:
- **SampleGlobalBootstrapper**: Registers global services that persist across scenes
- **SampleSceneBootstrapper**: Registers scene-specific services

### `Scripts/SampleConsumer.cs`
Contains example MonoBehaviour components that consume services:
- **SampleConsumer**: Demonstrates using `OmniServio.For(this)` for automatic resolution
- **SampleGlobalConsumer**: Demonstrates direct global OmniServio access
- **SampleSceneConsumer**: Demonstrates scene-specific OmniServio access
- **SampleDependencyInjectionConsumer**: Basic example of automatic dependency injection using `[Inject]` attribute

### `Scripts/SampleDependencyInjection.cs`
Comprehensive dependency injection examples with detailed usage patterns:
- **SampleDependencyInjection**: Complete example showing field injection, property injection, global injection, and manual injection
- **SampleBaseComponent** / **SampleDerivedComponent**: Shows inheritance of injected dependencies
- **SampleConditionalInjection**: Demonstrates conditional injection patterns
- **SampleManualInjection**: Shows manual injection without AutoInjectComponent
- **SampleSpecificInjection**: Demonstrates injection from a specific OmniServio instance

## 🚀 Quick Setup

### 1. Create a Global OmniServio

1. In Unity, go to **GameObject → OmniServio → Add Global**
2. Add the `SampleGlobalBootstrapper` component to the created GameObject
3. Configure which services to enable in the Inspector

### 2. Create a Scene OmniServio

1. In Unity, go to **GameObject → OmniServio → Add Scene**
2. Add the `SampleSceneBootstrapper` component to the created GameObject
3. Configure which services to enable in the Inspector

### 3. Create Service Consumers

**Method 1: Manual Service Retrieval**

1. Create a new GameObject in your scene
2. Add one of the consumer components (`SampleConsumer`, `SampleGlobalConsumer`, or `SampleSceneConsumer`)
3. The component will automatically retrieve and use services when the scene starts

**Method 2: Automatic Dependency Injection (Recommended)**

1. Create a new GameObject in your scene
2. Add the `AutoInjectComponent` component (from OmniServio package)
3. Add the `SampleDependencyInjectionConsumer` component
4. Mark fields/properties with `[Inject]` attribute - they will be automatically populated!

## 🎮 Testing the Samples

Once set up, you can test the samples:

- **Space Key**: Plays a sound and starts the game
- **S Key**: Saves the game
- **A Key**: Plays a global sound (if using SampleGlobalConsumer)
- **G Key**: Starts the game (if using SampleSceneConsumer)
- **I Key**: Uses injected services (if using SampleDependencyInjectionConsumer)
- **J Key**: Manually triggers injection (if using SampleDependencyInjectionConsumer)
- **GUI Buttons**: Interactive buttons appear in the top-left corner when using SampleConsumer or SampleDependencyInjectionConsumer

## 📝 Usage Patterns Demonstrated

1. **Global Services**: Services that persist across scenes (Audio, Save System)
2. **Scene Services**: Services specific to a scene (Gameplay Logic)
3. **Updatable Services**: Services that receive Unity update callbacks
4. **Destroyable Services**: Services that clean up when OmniServio is destroyed
5. **Service Resolution**: Different methods of retrieving services (hierarchy, global, scene)
6. **Dependency Injection**: Automatic injection using `[Inject]` attribute with `AutoInjectComponent`

## 💡 Dependency Injection Examples

### Basic Usage

The `SampleDependencyInjectionConsumer` demonstrates basic dependency injection:

```csharp
public class SampleDependencyInjectionConsumer : MonoBehaviour
{
    // Fields are automatically injected
    [Inject] private IAudioService _audioService;
    [Inject] private ISaveService _saveService;
    
    // Properties can also be injected
    [Inject] public GameTimer GameTimer { get; private set; }
    
    // Use global services with UseGlobal = true
    [Inject(UseGlobal = true)] private IAudioService _globalAudioService;
    
    void Start()
    {
        // Dependencies are already injected!
        _audioService?.PlaySound("ready");
    }
}
```

### Comprehensive Examples

See `SampleDependencyInjection.cs` for comprehensive examples including:
- Field and property injection
- Global vs local service injection
- Manual injection patterns
- Inheritance with injected dependencies
- Conditional injection
- Injection from specific OmniServio instances

**To enable automatic injection:**
1. Add `AutoInjectComponent` to your GameObject (or a parent)
2. Mark fields/properties with `[Inject]` attribute
3. Ensure services are registered in a bootstrapper
4. That's it! Dependencies are injected automatically on Awake

**Manual injection (without AutoInjectComponent):**
```csharp
void Awake()
{
    DependencyInjector.Inject(this);
}
```

## 🔧 Customization

Feel free to modify these samples to fit your project's needs:
- Create your own service interfaces and implementations
- Implement custom bootstrappers for your specific services
- Use the patterns shown here as a starting point for your own code
- Experiment with dependency injection for cleaner, more maintainable code
