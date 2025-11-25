using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace AtaYanki.OmniServio
{
    /// <summary>
    /// Exception handler mode for dependency injection errors.
    /// </summary>
    public enum InjectionExceptionHandlerMode
    {
        /// <summary>
        /// Logs warnings when injection errors occur. Execution continues.
        /// </summary>
        Warning,
        
        /// <summary>
        /// Throws exceptions when injection errors occur. Stops execution.
        /// </summary>
        ThrowException
    }

    /// <summary>
    /// Manages automatic dependency injection for all MonoBehaviour components.
    /// Runs after bootstrappers to ensure services are registered before injection.
    /// Execution order is set to 100 to run after bootstrappers (default 0).
    /// </summary>
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public class DependencyInjectionManager : MonoBehaviour
    {
        private static HashSet<Scene> _injectedScenes = new HashSet<Scene>();
        private static bool _globalInjected = false;

        [Header("Injection Settings")]
        [SerializeField] private bool injectOnAwake = true;
        [SerializeField] private bool injectOnStart = false;
        [SerializeField] private bool injectChildren = true;
        [SerializeField] private bool injectInactiveObjects = false;

        [Header("Exception Handler Settings")]
        [Tooltip("Choose how to handle injection errors: Warning (logs warnings) or ThrowException (throws exceptions). If 'Use Global Config' is enabled, this value is ignored.")]
        [SerializeField] private InjectionExceptionHandlerMode exceptionHandlerMode = InjectionExceptionHandlerMode.Warning;
        
        [Tooltip("Use the global OmniServioConfig for exception handler mode instead of the local setting.")]
        [SerializeField] private bool useGlobalConfig = true;

        private void Awake()
        {
            // Configure the exception handler based on the selected mode
            ConfigureExceptionHandler();

            if (injectOnAwake)
            {
                PerformInjection();
            }
        }

        /// <summary>
        /// Configures the exception handler based on the selected mode.
        /// Uses global config if enabled, otherwise uses local setting.
        /// </summary>
        private void ConfigureExceptionHandler()
        {
            InjectionExceptionHandlerMode mode;
            
            if (useGlobalConfig && OmniServioConfig.Instance != null)
            {
                mode = OmniServioConfig.Instance.DefaultExceptionHandlerMode;
            }
            else
            {
                mode = exceptionHandlerMode;
            }

            IInjectionExceptionHandler handler = mode switch
            {
                InjectionExceptionHandlerMode.ThrowException => new ThrowExceptionHandler(),
                InjectionExceptionHandlerMode.Warning => new WarningExceptionHandler(),
                _ => new WarningExceptionHandler()
            };

            DependencyInjector.SetExceptionHandler(handler);
        }

        private void Start()
        {
            if (injectOnStart)
            {
                PerformInjection();
            }
        }

        /// <summary>
        /// Performs dependency injection for all MonoBehaviour components in the scene.
        /// </summary>
        private void PerformInjection()
        {
            Scene scene = gameObject.scene;
            
            // Check if we've already injected for this scene
            if (_injectedScenes.Contains(scene))
            {
                return;
            }

            // Get the appropriate OmniServio instance for this scene
            OmniServio omniServio = OmniServio.For(this);
            if (omniServio == null)
            {
                Debug.LogWarning("[DependencyInjectionManager] No OmniServio found. Injection skipped.", this);
                return;
            }

            // Inject all MonoBehaviour components in the scene
            // Use FindObjectsByType (Unity 2021.2+) - we'll filter by active/inactive status in the loop below
            MonoBehaviour[] allComponents = Object.FindObjectsByType<MonoBehaviour>(
                injectInactiveObjects ? FindObjectsInactive.Include : FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            
            int injectedCount = 0;
            foreach (MonoBehaviour component in allComponents)
            {
                // Skip this component and destroyed objects
                if (component == null || component == this) continue;

                // Only inject components in the same scene
                if (component.gameObject.scene != scene) continue;

                // Skip if component is inactive and we're not injecting inactive objects
                if (!injectInactiveObjects && !component.gameObject.activeInHierarchy) continue;

                // Perform injection
                DependencyInjector.Inject(component, omniServio);
                injectedCount++;
            }

            // Mark this scene as injected
            _injectedScenes.Add(scene);
            
            Debug.Log($"[DependencyInjectionManager] Injected dependencies into {injectedCount} components in scene '{scene.name}'");
        }

        /// <summary>
        /// Manually trigger injection for the current scene.
        /// Useful for runtime service registration scenarios.
        /// </summary>
        public void InjectScene()
        {
            Scene scene = gameObject.scene;
            _injectedScenes.Remove(scene); // Allow re-injection
            PerformInjection();
        }

        /// <summary>
        /// Manually trigger injection for a specific GameObject and its children.
        /// </summary>
        public void InjectGameObject(GameObject target)
        {
            if (target == null) return;

            OmniServio omniServio = OmniServio.For(this);
            if (omniServio == null) return;

            MonoBehaviour[] components = injectChildren 
                ? target.GetComponentsInChildren<MonoBehaviour>(injectInactiveObjects)
                : target.GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour component in components)
            {
                if (component == null) continue;
                DependencyInjector.Inject(component, omniServio);
            }
        }

        private void OnDestroy()
        {
            // Clean up scene tracking when manager is destroyed
            Scene scene = gameObject.scene;
            if (scene.IsValid())
            {
                _injectedScenes.Remove(scene);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _injectedScenes.Clear();
            _globalInjected = false;
        }
    }
}

