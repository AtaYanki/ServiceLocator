using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AtaYanki.OmniServio.Editor
{
    /// <summary>
    /// Editor script that automatically loads a bootstrap scene when entering Play mode.
    /// Useful for ensuring services are registered before gameplay starts.
    /// </summary>
    [InitializeOnLoad]
    public class SceneBootstrapper
    {
        // Use these keys for Editor preferences
        private const string k_PreviousScene = "OmniServio.PreviousScene";
        private const string k_ShouldLoadBootstrap = "OmniServio.LoadBootstrapScene";
        private const string k_BootstrapScenePath = "OmniServio.BootstrapScenePath";

        // These appear as menu names
        private const string k_LoadBootstrapMenu = "OmniServio/Load Bootstrap Scene On Play";
        private const string k_DontLoadBootstrapMenu = "OmniServio/Don't Load Bootstrap Scene On Play";

        // This gets the bootstrap scene path from EditorPrefs or uses default
        private static string BootstrapScenePath
        {
            get
            {
                string path = EditorPrefs.GetString(k_BootstrapScenePath, "");
                if (string.IsNullOrEmpty(path))
                {
                    // Try to find a scene with "Bootstrap" in the name
                    foreach (var scene in EditorBuildSettings.scenes)
                    {
                        if (scene.path.Contains("Bootstrap") || scene.path.Contains("bootstrap"))
                        {
                            return scene.path;
                        }
                    }
                    // Fallback to first scene in build settings
                    if (EditorBuildSettings.scenes.Length > 0)
                    {
                        return EditorBuildSettings.scenes[0].path;
                    }
                }
                return path;
            }
            set => EditorPrefs.SetString(k_BootstrapScenePath, value);
        }

        // This string is the scene path where we entered Play mode 
        private static string PreviousScene
        {
            get => EditorPrefs.GetString(k_PreviousScene);
            set => EditorPrefs.SetString(k_PreviousScene, value);
        }

        // Is the bootstrap behavior enabled?
        private static bool ShouldLoadBootstrapScene
        {
            get => EditorPrefs.GetBool(k_ShouldLoadBootstrap, false);
            set => EditorPrefs.SetBool(k_ShouldLoadBootstrap, value);
        }

        // Track if we've captured the previous scene for this play session
        private static bool _hasCapturedPreviousScene = false;
        
        // Track if we've loaded the previous scene after bootstrap
        private static bool _hasLoadedPreviousScene = false;
        private static int _framesWaited = 0;
        private static float _timeWaited = 0f;
        
        // Configuration for waiting for bootstrappers
        private const float MaxWaitTime = 5f; // Maximum time to wait in seconds
        private const int MinFramesToWait = 5; // Minimum frames to wait
        private const float AdditionalWaitPerFrame = 0.1f; // Additional time per frame

        // A static constructor runs with InitializeOnLoad attribute
        static SceneBootstrapper()
        {
            // Listen for the Editor changing play states
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            
            // Also listen to update to capture scene before play mode starts
            EditorApplication.update += OnEditorUpdate;
        }

        // Capture the current scene before play mode starts
        private static void OnEditorUpdate()
        {
            // Only capture if feature is enabled and we haven't captured yet
            if (!ShouldLoadBootstrapScene || _hasCapturedPreviousScene)
                return;

            // Capture scene when play mode is about to start (but we're still in edit mode)
            // This happens before ExitingEditMode is called
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
            {
                // Capture the scene path immediately, before any scene switching happens
                if (EditorSceneManager.GetActiveScene().IsValid())
                {
                    PreviousScene = EditorSceneManager.GetActiveScene().path;
                    _hasCapturedPreviousScene = true;
                    Debug.Log($"[SceneBootstrapper] Captured previous scene: {PreviousScene}");
                }
            }
            
            // Reset flag when we're back in edit mode
            if (!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
            {
                _hasCapturedPreviousScene = false;
                _hasLoadedPreviousScene = false;
                _framesWaited = 0;
                _timeWaited = 0f;
            }
            
            // Handle loading previous scene after bootstrap in play mode
            if (EditorApplication.isPlaying && ShouldLoadBootstrapScene && !_hasLoadedPreviousScene && !string.IsNullOrEmpty(PreviousScene))
            {
                // Wait for bootstrappers to initialize (they run at execution order -100)
                _framesWaited++;
                _timeWaited += UnityEngine.Time.deltaTime;
                
                // Check if all bootstrappers have completed
                bool allBootstrappersComplete = AreAllBootstrappersComplete();
                
                // Wait for minimum frames AND minimum time to handle long-running tasks
                // This ensures bootstrappers have time to complete async operations
                bool minFramesMet = _framesWaited >= MinFramesToWait;
                bool minTimeMet = _timeWaited >= AdditionalWaitPerFrame * MinFramesToWait;
                bool maxTimeExceeded = _timeWaited >= MaxWaitTime;
                
                // Load previous scene if:
                // 1. All bootstrappers are complete AND minimum wait conditions met, OR
                // 2. Max time exceeded (to prevent infinite wait)
                if ((allBootstrappersComplete && minFramesMet && minTimeMet) || maxTimeExceeded)
                {
                    if (maxTimeExceeded && !allBootstrappersComplete)
                    {
                        Debug.LogWarning($"[SceneBootstrapper] Max wait time ({MaxWaitTime}s) exceeded. Some bootstrappers may not have completed. Loading previous scene anyway. If bootstrappers have long-running tasks, call MarkBootstrapComplete() when done or increase MaxWaitTime.");
                    }
                    LoadPreviousSceneAfterBootstrap();
                }
            }
        }

        // This runs when the Editor changes play state (e.g. entering Play mode, exiting Play mode)
        private static void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            // Do nothing if disabled from the menus
            if (!ShouldLoadBootstrapScene)
            {
                return;
            }

            switch (playModeStateChange)
            {
                // This sets up the bootstrap scene when entering Play mode
                case PlayModeStateChange.ExitingEditMode:
                    // Previous scene should already be captured in OnEditorUpdate
                    // But if for some reason it wasn't, capture it now as fallback
                    if (string.IsNullOrEmpty(PreviousScene))
                    {
                        PreviousScene = EditorSceneManager.GetActiveScene().path;
                        Debug.LogWarning($"[SceneBootstrapper] Previous scene not captured earlier, using fallback: {PreviousScene}");
                    }
                    else
                    {
                        Debug.Log($"[SceneBootstrapper] Using captured previous scene: {PreviousScene}");
                    }

                    // Get the bootstrap scene path
                    string bootstrapPath = BootstrapScenePath;

                    if (string.IsNullOrEmpty(bootstrapPath))
                    {
                        Debug.LogWarning("[SceneBootstrapper] No bootstrap scene configured. Please set it in OmniServio preferences or ensure a scene with 'Bootstrap' in the name exists in Build Settings.");
                        return;
                    }

                    // Check if scene exists in build settings
                    if (!IsSceneInBuildSettings(bootstrapPath))
                    {
                        Debug.LogWarning($"[SceneBootstrapper] Bootstrap scene '{bootstrapPath}' is not in Build Settings. Skipping bootstrap scene load.");
                        return;
                    }

                    // Save current scene if modified
                    if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        // User cancelled save, abort play mode entry
                        EditorApplication.isPlaying = false;
                        return;
                    }

                    // Set the play mode start scene - this will load automatically when entering play mode
                    SceneAsset bootstrapScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(bootstrapPath);
                    if (bootstrapScene != null)
                    {
                        EditorSceneManager.playModeStartScene = bootstrapScene;
                        Debug.Log($"[SceneBootstrapper] Set bootstrap scene: {bootstrapPath}");
                    }
                    else
                    {
                        Debug.LogError($"[SceneBootstrapper] Could not load scene asset at: {bootstrapPath}");
                    }
                    break;

                // This loads the bootstrap scene when play mode actually starts
                case PlayModeStateChange.EnteredPlayMode:
                    // Reset flags for new play session
                    _hasLoadedPreviousScene = false;
                    _framesWaited = 0;
                    _timeWaited = 0f;
                    
                    // The scene should already be loaded by playModeStartScene, but verify
                    string currentScenePath = SceneManager.GetActiveScene().path;
                    string expectedBootstrapPath = BootstrapScenePath;

                    if (!string.IsNullOrEmpty(expectedBootstrapPath) &&
                        !currentScenePath.Equals(expectedBootstrapPath, System.StringComparison.OrdinalIgnoreCase))
                    {
                        // If for some reason the scene didn't load, load it now
                        Debug.Log($"[SceneBootstrapper] Loading bootstrap scene: {expectedBootstrapPath}");
                        SceneManager.LoadScene(System.IO.Path.GetFileNameWithoutExtension(expectedBootstrapPath));
                    }
                    break;

                // This restores the PreviousScene when exiting Play mode
                case PlayModeStateChange.EnteredEditMode:
                    // Clear the play mode start scene
                    EditorSceneManager.playModeStartScene = null;
                    
                    // Reset flags
                    _hasCapturedPreviousScene = false;
                    _hasLoadedPreviousScene = false;
                    _framesWaited = 0;
                    _timeWaited = 0f;
                    break;
            }
        }

        // These menu items toggle behavior.

        // This adds a menu item called "Load Bootstrap Scene On Play" to the GameSystems menu and
        // enables the behavior if selected.
        [MenuItem(k_LoadBootstrapMenu)]
        private static void EnableBootstrapper()
        {
            ShouldLoadBootstrapScene = true;
        }

        // Validates the above function and menu item, which grays out if ShouldLoadBootstrapScene is true.
        [MenuItem(k_LoadBootstrapMenu, true)]
        private static bool ValidateEnableBootstrapper()
        {
            return !ShouldLoadBootstrapScene;
        }

        // Adds a menu item called "Don't Load Bootstrap Scene On Play" to the GameSystems menu and
        // disables the behavior if selected.
        [MenuItem(k_DontLoadBootstrapMenu)]
        private static void DisableBootstrapper()
        {
            ShouldLoadBootstrapScene = false;
        }

        // Validates the above function and menu item, which grays out if ShouldLoadBootstrapScene is false.
        [MenuItem(k_DontLoadBootstrapMenu, true)]
        private static bool ValidateDisableBootstrapper()
        {
            return ShouldLoadBootstrapScene;
        }

        // Is a scenePath a valid scene in the File > Build Settings?
        private static bool IsSceneInBuildSettings(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath))
                return false;

            // Normalize paths for comparison
            string normalizedPath = scenePath.Replace('\\', '/');

            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (string.IsNullOrEmpty(scene.path))
                    continue;

                string normalizedScenePath = scene.path.Replace('\\', '/');

                // Check for exact match or if the scene path contains the bootstrap path
                if (normalizedScenePath.Equals(normalizedPath, System.StringComparison.OrdinalIgnoreCase) ||
                    normalizedScenePath.Contains(normalizedPath) ||
                    normalizedPath.Contains(normalizedScenePath))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if all bootstrappers in the current scene have completed initialization.
        /// Uses reflection to check the IsBootstrapComplete property.
        /// </summary>
        private static bool AreAllBootstrappersComplete()
        {
            if (!EditorApplication.isPlaying)
                return false;

            // Get the Bootstrapper type using reflection (since editor can't directly reference runtime types)
            System.Type bootstrapperType = System.Type.GetType("AtaYanki.OmniServio.Bootstrapper, Assembly-CSharp");
            if (bootstrapperType == null)
            {
                // If type not found, assume complete (fallback behavior)
                return true;
            }

            // Find all bootstrappers in the current scene
            var bootstrappers = Object.FindObjectsByType(bootstrapperType, FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            
            if (bootstrappers.Length == 0)
            {
                // No bootstrappers found, assume complete
                return true;
            }

            // Get the IsBootstrapComplete property
            var completeProperty = bootstrapperType.GetProperty("IsBootstrapComplete", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            // Check if all bootstrappers are complete
            foreach (var bootstrapper in bootstrappers)
            {
                if (bootstrapper == null) continue;
                
                if (completeProperty != null)
                {
                    bool isComplete = (bool)completeProperty.GetValue(bootstrapper);
                    if (!isComplete)
                    {
                        return false; // At least one bootstrapper is not complete
                    }
                }
                else
                {
                    // Property not found, assume complete (fallback)
                    continue;
                }
            }

            return true; // All bootstrappers are complete
        }

        /// <summary>
        /// Loads the previous scene after bootstrap scene has initialized.
        /// Called from OnEditorUpdate after waiting for bootstrappers to complete.
        /// </summary>
        private static void LoadPreviousSceneAfterBootstrap()
        {
            // Only load if we're still in play mode and have a previous scene
            if (!EditorApplication.isPlaying || string.IsNullOrEmpty(PreviousScene))
            {
                return;
            }

            // Check if we're currently in the bootstrap scene
            string currentScenePath = SceneManager.GetActiveScene().path;
            string bootstrapPath = BootstrapScenePath;

            // Only load previous scene if we're in the bootstrap scene
            if (!string.IsNullOrEmpty(bootstrapPath) && 
                currentScenePath.Equals(bootstrapPath, System.StringComparison.OrdinalIgnoreCase))
            {
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(PreviousScene);
                bool allComplete = AreAllBootstrappersComplete();
                Debug.Log($"[SceneBootstrapper] Bootstrap initialized ({_framesWaited} frames, {_timeWaited:F2}s, All complete: {allComplete}). Loading previous scene: {sceneName}");
                
                // Mark as loaded to prevent multiple loads
                _hasLoadedPreviousScene = true;
                
                // Load the previous scene (replaces bootstrap scene)
                SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            }
        }
    }
}
