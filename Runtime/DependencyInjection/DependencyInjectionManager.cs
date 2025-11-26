using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace AtaYanki.OmniServio
{
    public enum InjectionExceptionHandlerMode
    {
        Warning = 1,
        ThrowException = 2
    }

    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public class DependencyInjectionManager : MonoBehaviour
    {
        private static HashSet<Scene> _injectedScenes = new HashSet<Scene>();

        [Header("Injection Settings")]
        [SerializeField] private bool injectOnAwake = true;
        [SerializeField] private bool injectOnStart = false;
        [SerializeField] private bool injectChildren = true;
        [SerializeField] private bool injectInactiveObjects = false;

        private void Awake()
        {
            ConfigureExceptionHandler();

            if (injectOnAwake)
            {
                PerformInjection();
            }
        }

        private void ConfigureExceptionHandler()
        {
            IInjectionExceptionHandler handler = OmniServioConfig.Instance.DefaultExceptionHandlerMode switch
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

        private void PerformInjection()
        {
            Scene scene = gameObject.scene;

            if (_injectedScenes.Contains(scene))
            {
                return;
            }

            OmniServio omniServio = OmniServio.For(this);
            if (omniServio == null)
            {
                Debug.LogWarning("[DependencyInjectionManager] No OmniServio found. Injection skipped.", this);
                return;
            }

            MonoBehaviour[] allComponents = Object.FindObjectsByType<MonoBehaviour>(
                injectInactiveObjects ? FindObjectsInactive.Include : FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);

            int injectedCount = 0;
            foreach (MonoBehaviour component in allComponents)
            {
                if (component == null || component == this) continue;

                if (component.gameObject.scene != scene) continue;

                if (!injectInactiveObjects && !component.gameObject.activeInHierarchy) continue;

                DependencyInjector.Inject(component, omniServio);
                injectedCount++;
            }

            _injectedScenes.Add(scene);

            Debug.Log($"[DependencyInjectionManager] Injected dependencies into {injectedCount} components in scene '{scene.name}'");
        }

        public void InjectScene()
        {
            Scene scene = gameObject.scene;
            _injectedScenes.Remove(scene);
            PerformInjection();
        }

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
        }
    }
}

