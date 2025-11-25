using UnityEngine;
using UnityEngine.SceneManagement;

namespace AtaYanki.OmniServio
{
    /// <summary>
    /// Base class for bootstrappers that register services.
    /// Runs early (execution order -100) to ensure services are registered before dependency injection.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(OmniServio))]
    public abstract class Bootstrapper : MonoBehaviour
    {
        private OmniServio _omniServio;
        internal OmniServio OmniServio => _omniServio.OrNull() ?? (_omniServio = GetComponent<OmniServio>());

        private bool _hasBeenBootstrapped;
        private bool _isBootstrapComplete = false;

        /// <summary>
        /// Indicates whether bootstrap has completed.
        /// Set to true when all bootstrap operations (including async) are done.
        /// </summary>
        public bool IsBootstrapComplete => _isBootstrapComplete;

        private void Awake() => BootstrapOnDemand();

        public void BootstrapOnDemand()
        {
            if (_hasBeenBootstrapped) return;

            _hasBeenBootstrapped = true;
            _isBootstrapComplete = false;
            
            Bootstrap();
            
            // If Bootstrap() is synchronous, mark as complete immediately
            // For async operations, call MarkBootstrapComplete() when done
            if (_isBootstrapComplete == false)
            {
                // Assume synchronous by default - mark complete after Bootstrap() returns
                MarkBootstrapComplete();
            }
            
            // Automatically add DependencyInjectionManager after bootstrap completes
            EnsureDependencyInjectionManager();
        }

        protected abstract void Bootstrap();

        /// <summary>
        /// Call this method when all bootstrap operations (including async) are complete.
        /// Use this if your Bootstrap() method has long-running or async tasks.
        /// </summary>
        protected void MarkBootstrapComplete()
        {
            _isBootstrapComplete = true;
        }

        /// <summary>
        /// Ensures DependencyInjectionManager exists in the scene to handle automatic injection.
        /// Only scene bootstrappers create DependencyInjectionManager (global bootstrappers don't need it).
        /// </summary>
        private void EnsureDependencyInjectionManager()
        {
            // Only scene bootstrappers should create DependencyInjectionManager
            // Global bootstrappers don't need it since they're not scene-specific
            if (this is OmniServioGlobalBootstrapper)
            {
                return; // Global bootstrapper doesn't create injection manager
            }

            // Check if DependencyInjectionManager already exists in this scene
            Scene scene = gameObject.scene;
            DependencyInjectionManager existingManager = FindFirstObjectByType<DependencyInjectionManager>();
            
            // Verify it's in the same scene
            if (existingManager != null && existingManager.gameObject.scene == scene)
            {
                return; // Already exists in this scene, no need to create another
            }

            // Create DependencyInjectionManager GameObject in the same scene
            GameObject injectionManagerGO = new GameObject("DependencyInjectionManager");
            SceneManager.MoveGameObjectToScene(injectionManagerGO, scene);
            injectionManagerGO.AddComponent<DependencyInjectionManager>();
            
            Debug.Log($"[Bootstrapper] Created DependencyInjectionManager for scene '{scene.name}'");
        }
    }
}