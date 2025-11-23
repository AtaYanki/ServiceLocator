using UnityEngine;

namespace AtaYanki.ServiceLocator
{
    [AddComponentMenu("ServiceLocator/ServiceLocator Global")]
    public class ServiceLocatorGlobalBootstrapper : Bootstrapper
    {
        [SerializeField] private bool dontDestroyOnLoad = true;

        protected override void Bootstrap()
        {
            ServiceLocator.ConfigureAsGlobal(dontDestroyOnLoad);
        }
    }
}