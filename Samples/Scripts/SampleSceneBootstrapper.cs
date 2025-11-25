using UnityEngine;
using AtaYanki.OmniServio;
using UnityEngine.SceneManagement;

namespace AtaYanki.OmniServio.Samples
{
    /// <summary>
    /// Example scene bootstrapper that registers services specific to a scene.
    /// Attach this component to a GameObject with OmniServioSceneBootstrapper component.
    /// </summary>
    public class SampleSceneBootstrapper : OmniServioSceneBootstrapper
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
                OmniServio.Register<IGameplayService>(new GameplayService());
                Debug.Log("[SampleSceneBootstrapper] Registered IGameplayService");
            }

            // Register fixed updatable service (receives FixedUpdate() callbacks)
            if (enablePhysicsMonitor)
            {
                OmniServio.RegisterFixedUpdatable<PhysicsMonitor>(new PhysicsMonitor());
                Debug.Log("[SampleSceneBootstrapper] Registered PhysicsMonitor as fixed updatable");
            }

            // Register late updatable service (receives LateUpdate() callbacks)
            if (enableCameraFollow)
            {
                CameraFollowService cameraService = new CameraFollowService();
                // In a real scenario, you might set a target here
                OmniServio.RegisterLateUpdatable<CameraFollowService>(cameraService);
                Debug.Log("[SampleSceneBootstrapper] Registered CameraFollowService as late updatable");
            }

            Debug.Log("[SampleSceneBootstrapper] Scene bootstrapping complete!");
        }
    }
}

