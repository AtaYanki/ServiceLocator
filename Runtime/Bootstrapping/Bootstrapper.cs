using UnityEngine;
using UnityEngine.SceneManagement;

namespace AtaYanki.OmniServio
{
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(OmniServio))]
    public abstract class Bootstrapper : MonoBehaviour
    {
        private OmniServio _omniServio;
        internal OmniServio OmniServio => _omniServio.OrNull() ?? (_omniServio = GetComponent<OmniServio>());

        private bool _hasBeenBootstrapped;
        private bool _isBootstrapComplete = false;

        public bool IsBootstrapComplete => _isBootstrapComplete;

        private void Awake() => BootstrapOnDemand();

        public void BootstrapOnDemand()
        {
            if (_hasBeenBootstrapped) return;

            _hasBeenBootstrapped = true;
            _isBootstrapComplete = false;

            Bootstrap();

            if (_isBootstrapComplete == false)
            {
                MarkBootstrapComplete();
            }

            EnsureDependencyInjectionManager();
        }

        protected abstract void Bootstrap();

        protected void MarkBootstrapComplete()
        {
            _isBootstrapComplete = true;
        }

        private void EnsureDependencyInjectionManager()
        {
            if (this is OmniServioGlobalBootstrapper)
            {
                return;
            }

            Scene scene = gameObject.scene;
            DependencyInjectionManager existingManager = FindFirstObjectByType<DependencyInjectionManager>();

            if (existingManager != null && existingManager.gameObject.scene == scene)
            {
                return;
            }

            GameObject injectionManagerGO = new GameObject("DependencyInjectionManager");
            SceneManager.MoveGameObjectToScene(injectionManagerGO, scene);
            injectionManagerGO.AddComponent<DependencyInjectionManager>();

            Debug.Log($"[Bootstrapper] Created DependencyInjectionManager for scene '{scene.name}'");
        }
    }
}