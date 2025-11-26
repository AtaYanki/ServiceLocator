using UnityEngine;

namespace AtaYanki.OmniServio
{
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