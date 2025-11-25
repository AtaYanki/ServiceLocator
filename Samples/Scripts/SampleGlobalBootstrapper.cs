using UnityEngine;

namespace AtaYanki.OmniServio.Samples
{
    public class SampleGlobalBootstrapper : OmniServioGlobalBootstrapper
    {
        [SerializeField] private bool enableAudioService = true;
        [SerializeField] private bool enableSaveService = true;
        [SerializeField] private bool enableGameTimer = true;
        [SerializeField] private bool enableNetworkService = true;

        protected override void Bootstrap()
        {
            base.Bootstrap();

            Debug.Log("[SampleGlobalBootstrapper] Bootstrapping global services...");

            if (enableAudioService)
            {
                OmniServio.Register<IAudioService>(new AudioService());
                Debug.Log("[SampleGlobalBootstrapper] Registered IAudioService");
            }

            if (enableSaveService)
            {
                OmniServio.Register<ISaveService>(new SaveService());
                Debug.Log("[SampleGlobalBootstrapper] Registered ISaveService");
            }

            if (enableGameTimer)
            {
                GameTimer timer = new GameTimer();
                OmniServio.RegisterUpdatable<GameTimer>(timer);
                Debug.Log("[SampleGlobalBootstrapper] Registered GameTimer as updatable");
            }

            if (enableNetworkService)
            {
                NetworkService networkService = new NetworkService();
                OmniServio.RegisterDestroyable<NetworkService>(networkService);
                Debug.Log("[SampleGlobalBootstrapper] Registered NetworkService as destroyable");
            }

            Debug.Log("[SampleGlobalBootstrapper] Global bootstrapping complete!");
        }
    }
}

