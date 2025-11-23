using UnityEngine;

namespace AtaYanki.ServiceLocator
{
    [AddComponentMenu("ServiceLocator/ServiceLocator Scene")]
    public class ServiceLocatorSceneBootstrapper : Bootstrapper
    {
        protected override void Bootstrap()
        {
            ServiceLocator.ConfigureForScene();
        }
    }
}