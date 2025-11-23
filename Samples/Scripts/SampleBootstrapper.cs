using UnityEngine;
using AtaYanki.ServiceLocator;

namespace AtaYanki.ServiceLocator.Samples
{
    /// <summary>
    /// Example global bootstrapper that registers services that should persist across scenes.
    /// Attach this component to a GameObject with ServiceLocatorGlobalBootstrapper component.
    /// </summary>
    public class SampleGlobalBootstrapper : ServiceLocatorGlobalBootstrapper
    {
        [SerializeField] private bool enableAudioService = true;
        [SerializeField] private bool enableSaveService = true;
        [SerializeField] private bool enableGameTimer = true;
        [SerializeField] private bool enableNetworkService = true;

        protected override void Bootstrap()
        {
            base.Bootstrap(); // Important: Call base to configure as global

            Debug.Log("[SampleGlobalBootstrapper] Bootstrapping global services...");

            // Register standard services
            if (enableAudioService)
            {
                ServiceLocator.Register<IAudioService>(new AudioService());
                Debug.Log("[SampleGlobalBootstrapper] Registered IAudioService");
            }

            if (enableSaveService)
            {
                ServiceLocator.Register<ISaveService>(new SaveService());
                Debug.Log("[SampleGlobalBootstrapper] Registered ISaveService");
            }

            // Register updatable service (receives Update() callbacks)
            if (enableGameTimer)
            {
                GameTimer timer = new GameTimer();
                timer.Start();
                ServiceLocator.RegisterUpdatable<GameTimer>(timer);
                Debug.Log("[SampleGlobalBootstrapper] Registered GameTimer as updatable");
            }

            // Register destroyable service (receives cleanup callbacks)
            if (enableNetworkService)
            {
                NetworkService networkService = new NetworkService();
                networkService.Connect();
                ServiceLocator.RegisterDestroyable<NetworkService>(networkService);
                Debug.Log("[SampleGlobalBootstrapper] Registered NetworkService as destroyable");
            }

            // Example of chaining multiple registrations
            // ServiceLocator
            //     .Register<IAudioService>(new AudioService())
            //     .Register<ISaveService>(new SaveService())
            //     .RegisterUpdatable<GameTimer>(new GameTimer())
            //     .RegisterDestroyable<NetworkService>(new NetworkService());

            Debug.Log("[SampleGlobalBootstrapper] Global bootstrapping complete!");
        }
    }

    /// <summary>
    /// Example scene bootstrapper that registers services specific to a scene.
    /// Attach this component to a GameObject with ServiceLocatorSceneBootstrapper component.
    /// </summary>
    public class SampleSceneBootstrapper : ServiceLocatorSceneBootstrapper
    {
        [SerializeField] private bool enableGameplayService = true;
        [SerializeField] private bool enablePhysicsMonitor = true;
        [SerializeField] private bool enableCameraFollow = true;

        protected override void Bootstrap()
        {
            base.Bootstrap(); // Important: Call base to configure for scene

            Debug.Log("[SampleSceneBootstrapper] Bootstrapping scene services...");

            // Register scene-specific services
            if (enableGameplayService)
            {
                ServiceLocator.Register<IGameplayService>(new GameplayService());
                Debug.Log("[SampleSceneBootstrapper] Registered IGameplayService");
            }

            // Register fixed updatable service (receives FixedUpdate() callbacks)
            if (enablePhysicsMonitor)
            {
                ServiceLocator.RegisterFixedUpdatable<PhysicsMonitor>(new PhysicsMonitor());
                Debug.Log("[SampleSceneBootstrapper] Registered PhysicsMonitor as fixed updatable");
            }

            // Register late updatable service (receives LateUpdate() callbacks)
            if (enableCameraFollow)
            {
                CameraFollowService cameraService = new CameraFollowService();
                // In a real scenario, you might set a target here
                ServiceLocator.RegisterLateUpdatable<CameraFollowService>(cameraService);
                Debug.Log("[SampleSceneBootstrapper] Registered CameraFollowService as late updatable");
            }

            Debug.Log("[SampleSceneBootstrapper] Scene bootstrapping complete!");
        }
    }
}

