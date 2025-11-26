using UnityEngine;
using AtaYanki.OmniServio;
using UnityEngine.SceneManagement;

namespace AtaYanki.OmniServio.Samples
{
    public class SampleSceneBootstrapper : OmniServioSceneBootstrapper
    {
        [SerializeField] private bool enableGameplayService = true;
        [SerializeField] private bool enablePhysicsMonitor = true;
        [SerializeField] private bool enableCameraFollow = true;

        protected override void Bootstrap()
        {
            base.Bootstrap();

            Debug.Log("[SampleSceneBootstrapper] Bootstrapping scene services...");

            if (enableGameplayService)
            {
                OmniServio.Register<IGameplayService>(new GameplayService());
                Debug.Log("[SampleSceneBootstrapper] Registered IGameplayService");
            }

            if (enablePhysicsMonitor)
            {
                OmniServio.RegisterFixedUpdatable<PhysicsMonitor>(new PhysicsMonitor());
                Debug.Log("[SampleSceneBootstrapper] Registered PhysicsMonitor as fixed updatable");
            }

            if (enableCameraFollow)
            {
                CameraFollowService cameraService = new CameraFollowService();
                OmniServio.RegisterLateUpdatable<CameraFollowService>(cameraService);
                Debug.Log("[SampleSceneBootstrapper] Registered CameraFollowService as late updatable");
            }

            Debug.Log("[SampleSceneBootstrapper] Scene bootstrapping complete!");
        }
    }
}

