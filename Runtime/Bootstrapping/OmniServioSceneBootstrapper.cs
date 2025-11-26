using UnityEngine;

namespace AtaYanki.OmniServio
{
    [DefaultExecutionOrder(-100)]
    [AddComponentMenu("OmniServio/OmniServio Scene")]
    public class OmniServioSceneBootstrapper : Bootstrapper
    {
        protected override void Bootstrap()
        {
            OmniServio.ConfigureForScene();
        }
    }
}