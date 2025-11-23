# Service Locator Samples

This folder contains sample scripts demonstrating how to use the Service Locator package.

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
- **SampleConsumer**: Demonstrates using `ServiceLocator.For(this)` for automatic resolution
- **SampleGlobalConsumer**: Demonstrates direct global ServiceLocator access
- **SampleSceneConsumer**: Demonstrates scene-specific ServiceLocator access

## 🚀 Quick Setup

### 1. Create a Global ServiceLocator

1. In Unity, go to **GameObject → ServiceLocator → Add Global**
2. Add the `SampleGlobalBootstrapper` component to the created GameObject
3. Configure which services to enable in the Inspector

### 2. Create a Scene ServiceLocator

1. In Unity, go to **GameObject → ServiceLocator → Add Scene**
2. Add the `SampleSceneBootstrapper` component to the created GameObject
3. Configure which services to enable in the Inspector

### 3. Create Service Consumers

1. Create a new GameObject in your scene
2. Add one of the consumer components (`SampleConsumer`, `SampleGlobalConsumer`, or `SampleSceneConsumer`)
3. The component will automatically retrieve and use services when the scene starts

## 🎮 Testing the Samples

Once set up, you can test the samples:

- **Space Key**: Plays a sound and starts the game
- **S Key**: Saves the game
- **A Key**: Plays a global sound (if using SampleGlobalConsumer)
- **G Key**: Starts the game (if using SampleSceneConsumer)
- **GUI Buttons**: Interactive buttons appear in the top-left corner when using SampleConsumer

## 📝 Usage Patterns Demonstrated

1. **Global Services**: Services that persist across scenes (Audio, Save System)
2. **Scene Services**: Services specific to a scene (Gameplay Logic)
3. **Updatable Services**: Services that receive Unity update callbacks
4. **Destroyable Services**: Services that clean up when ServiceLocator is destroyed
5. **Service Resolution**: Different methods of retrieving services (hierarchy, global, scene)

## 💡 Customization

Feel free to modify these samples to fit your project's needs:
- Create your own service interfaces and implementations
- Implement custom bootstrappers for your specific services
- Use the patterns shown here as a starting point for your own code

