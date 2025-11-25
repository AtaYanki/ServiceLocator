using UnityEngine;

namespace AtaYanki.OmniServio
{
    /// <summary>
    /// Bootstrapper for scene-specific services.
    /// Runs early (execution order -100) to register services before dependency injection.
    /// </summary>
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