using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AtaYanki.OmniServio
{
    /// <summary>
    /// Global configuration for OmniServio service locator.
    /// Create this asset via the menu: Assets > Create > OmniServio > Config
    /// </summary>
    [CreateAssetMenu(fileName = "OmniServioConfig", menuName = "OmniServio/Config", order = 1)]
    public class OmniServioConfig : ScriptableObject
    {
        [Header("Dependency Injection Settings")]
        [Tooltip("Default exception handler mode for dependency injection errors.")]
        [SerializeField] private InjectionExceptionHandlerMode defaultExceptionHandlerMode = InjectionExceptionHandlerMode.Warning;

        [Header("Bootstrap Scene Settings")]
        [Tooltip("Reference to the global bootstrap scene. This scene will be loaded first to initialize services.")]
#if UNITY_EDITOR
        [SerializeField] private SceneAsset globalBootstrapScene;
#else
        [SerializeField] private string globalBootstrapSceneName;
#endif

        [Tooltip("Whether to automatically load the bootstrap scene when entering Play mode in the editor.")]
        [SerializeField] private bool autoLoadBootstrapSceneInEditor = true;

        [Tooltip("Whether to load the bootstrap scene at runtime (for builds).")]
        [SerializeField] private bool loadBootstrapSceneAtRuntime = false;

        /// <summary>
        /// Gets the default exception handler mode.
        /// </summary>
        public InjectionExceptionHandlerMode DefaultExceptionHandlerMode => defaultExceptionHandlerMode;

        /// <summary>
        /// Gets the global bootstrap scene asset (editor only).
        /// </summary>
#if UNITY_EDITOR
        public SceneAsset GlobalBootstrapScene => globalBootstrapScene;
#else
        public string GlobalBootstrapSceneName => globalBootstrapSceneName;
#endif

        /// <summary>
        /// Gets whether to auto-load bootstrap scene in editor.
        /// </summary>
        public bool AutoLoadBootstrapSceneInEditor => autoLoadBootstrapSceneInEditor;

        /// <summary>
        /// Gets whether to load bootstrap scene at runtime.
        /// </summary>
        public bool LoadBootstrapSceneAtRuntime => loadBootstrapSceneAtRuntime;

        /// <summary>
        /// Gets the bootstrap scene path as a string (editor only).
        /// </summary>
        public string GetBootstrapScenePath()
        {
#if UNITY_EDITOR
            if (globalBootstrapScene == null)
            {
                return null;
            }
            return AssetDatabase.GetAssetPath(globalBootstrapScene);
#else
            return null; // Not available at runtime
#endif
        }

        /// <summary>
        /// Gets the bootstrap scene name.
        /// </summary>
        public string GetBootstrapSceneName()
        {
#if UNITY_EDITOR
            if (globalBootstrapScene == null)
            {
                return null;
            }
            return globalBootstrapScene.name;
#else
            return globalBootstrapSceneName;
#endif
        }

        /// <summary>
        /// Sets the exception handler mode.
        /// </summary>
        public void SetExceptionHandlerMode(InjectionExceptionHandlerMode mode)
        {
            defaultExceptionHandlerMode = mode;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>
        /// Sets the global bootstrap scene (editor only).
        /// </summary>
#if UNITY_EDITOR
        public void SetGlobalBootstrapScene(SceneAsset scene)
        {
            globalBootstrapScene = scene;
            EditorUtility.SetDirty(this);
        }
#else
        public void SetGlobalBootstrapSceneName(string sceneName)
        {
            globalBootstrapSceneName = sceneName;
        }
#endif

        /// <summary>
        /// Gets the singleton instance of the config.
        /// Searches for the config asset in Resources folder or via AssetDatabase.
        /// </summary>
        public static OmniServioConfig Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

#if UNITY_EDITOR
                // Try to find in Resources first
                _instance = Resources.Load<OmniServioConfig>("OmniServioConfig");
                
                // If not found, search all assets
                if (_instance == null)
                {
                    string[] guids = AssetDatabase.FindAssets("t:OmniServioConfig");
                    if (guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        _instance = AssetDatabase.LoadAssetAtPath<OmniServioConfig>(path);
                    }
                }
#else
                // At runtime, load from Resources
                _instance = Resources.Load<OmniServioConfig>("OmniServioConfig");
#endif

                return _instance;
            }
        }

        private static OmniServioConfig _instance;

        private void OnEnable()
        {
            // Initialize the singleton when the asset is loaded
            if (_instance == null)
            {
                _instance = this;
            }
        }
    }
}

