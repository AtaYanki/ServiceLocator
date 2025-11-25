using UnityEngine;

namespace AtaYanki.OmniServio
{
    /// <summary>
    /// Bootstrapper for global services that persist across scenes.
    /// Runs early (execution order -100) to register services before dependency injection.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [AddComponentMenu("OmniServio/OmniServio Global")]
    public class OmniServioGlobalBootstrapper : Bootstrapper
    {
        [SerializeField] private bool dontDestroyOnLoad = true;

        protected override void Bootstrap()
        {
            OmniServio.ConfigureAsGlobal(dontDestroyOnLoad);
        }
    }
}