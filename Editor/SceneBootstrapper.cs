using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace AtaYanki.OmniServio.Editor
{
    [InitializeOnLoad]
    public class SceneBootstrapper
    {
        private const string k_PreviousScene = "OmniServio.PreviousScene";

        private static string BootstrapScenePath
        {
            get
            {
                var config = OmniServioConfig.Instance;
                if (config == null || config.GlobalBootstrapScene == null)
                {
                    return null;
                }

                return config.GetBootstrapScenePath();
            }
        }

        private static string PreviousScene
        {
            get => EditorPrefs.GetString(k_PreviousScene);
            set => EditorPrefs.SetString(k_PreviousScene, value);
        }

        private static bool ShouldLoadBootstrapScene
        {
            get
            {
                var config = OmniServioConfig.Instance;
                return config != null && config.AutoLoadBootstrapSceneInEditor;
            }
        }

        private static bool _hasCapturedPreviousScene = false;
        
        private static bool _hasLoadedPreviousScene = false;
        private static int _framesWaited = 0;
        private static float _timeWaited = 0f;
        
        private const float MaxWaitTime = 5f;
        private const int MinFramesToWait = 5;
        private const float AdditionalWaitPerFrame = 0.1f;

        static SceneBootstrapper()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;            
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            if (!ShouldLoadBootstrapScene || _hasCapturedPreviousScene)
                return;

            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
            {
                if (EditorSceneManager.GetActiveScene().IsValid())
                {
                    PreviousScene = EditorSceneManager.GetActiveScene().path;
                    _hasCapturedPreviousScene = true;
                    Debug.Log($"[SceneBootstrapper] Captured previous scene: {PreviousScene}");
                }
            }
            
            if (!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
            {
                _hasCapturedPreviousScene = false;
                _hasLoadedPreviousScene = false;
                _framesWaited = 0;
                _timeWaited = 0f;
            }
            
            if (EditorApplication.isPlaying && ShouldLoadBootstrapScene && !_hasLoadedPreviousScene && !string.IsNullOrEmpty(PreviousScene))
            {
                _framesWaited++;
                _timeWaited += UnityEngine.Time.deltaTime;
                
                bool allBootstrappersComplete = AreAllBootstrappersComplete();
                
                bool minFramesMet = _framesWaited >= MinFramesToWait;
                bool minTimeMet = _timeWaited >= AdditionalWaitPerFrame * MinFramesToWait;
                bool maxTimeExceeded = _timeWaited >= MaxWaitTime;
                
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

        private static void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            if (!ShouldLoadBootstrapScene)
            {
                return;
            }

            switch (playModeStateChange)
            {
                case PlayModeStateChange.ExitingEditMode:
                    if (string.IsNullOrEmpty(PreviousScene))
                    {
                        PreviousScene = EditorSceneManager.GetActiveScene().path;
                        Debug.LogWarning($"[SceneBootstrapper] Previous scene not captured earlier, using fallback: {PreviousScene}");
                    }
                    else
                    {
                        Debug.Log($"[SceneBootstrapper] Using captured previous scene: {PreviousScene}");
                    }

                    string bootstrapPath = BootstrapScenePath;

                    if (string.IsNullOrEmpty(bootstrapPath))
                    {
                        Debug.LogWarning("[SceneBootstrapper] No bootstrap scene configured. Please set it in OmniServio Config (OmniServio > Config > Settings) or create a config asset.");
                        return;
                    }

                    if (!IsSceneInBuildSettings(bootstrapPath))
                    {
                        Debug.LogWarning($"[SceneBootstrapper] Bootstrap scene '{bootstrapPath}' is not in Build Settings. Skipping bootstrap scene load.");
                        return;
                    }

                    if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorApplication.isPlaying = false;
                        return;
                    }

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

                case PlayModeStateChange.EnteredPlayMode:
                    _hasLoadedPreviousScene = false;
                    _framesWaited = 0;
                    _timeWaited = 0f;
                    
                    string currentScenePath = SceneManager.GetActiveScene().path;
                    string expectedBootstrapPath = BootstrapScenePath;

                    if (!string.IsNullOrEmpty(expectedBootstrapPath) &&
                        !currentScenePath.Equals(expectedBootstrapPath, System.StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.Log($"[SceneBootstrapper] Loading bootstrap scene: {expectedBootstrapPath}");
                        SceneManager.LoadScene(System.IO.Path.GetFileNameWithoutExtension(expectedBootstrapPath));
                    }
                    break;

                case PlayModeStateChange.EnteredEditMode:
                    EditorSceneManager.playModeStartScene = null;
                    
                    _hasCapturedPreviousScene = false;
                    _hasLoadedPreviousScene = false;
                    _framesWaited = 0;
                    _timeWaited = 0f;
                    break;
            }
        }


        private static bool IsSceneInBuildSettings(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath))
                return false;

            string normalizedPath = scenePath.Replace('\\', '/');

            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (string.IsNullOrEmpty(scene.path))
                    continue;

                string normalizedScenePath = scene.path.Replace('\\', '/');

                if (normalizedScenePath.Equals(normalizedPath, System.StringComparison.OrdinalIgnoreCase) ||
                    normalizedScenePath.Contains(normalizedPath) ||
                    normalizedPath.Contains(normalizedScenePath))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool AreAllBootstrappersComplete()
        {
            if (!EditorApplication.isPlaying)
                return false;

            System.Type bootstrapperType = System.Type.GetType("AtaYanki.OmniServio.Bootstrapper, Assembly-CSharp");
            if (bootstrapperType == null)
            {
                return true;
            }

            var bootstrappers = Object.FindObjectsByType(bootstrapperType, FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            
            if (bootstrappers.Length == 0)
            {
                return true;
            }

            var completeProperty = bootstrapperType.GetProperty("IsBootstrapComplete", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (var bootstrapper in bootstrappers)
            {
                if (bootstrapper == null) continue;
                
                if (completeProperty != null)
                {
                    bool isComplete = (bool)completeProperty.GetValue(bootstrapper);
                    if (!isComplete)
                    {
                        return false;
                    }
                }
                else
                {
                    continue;
                }
            }

            return true;
        }

        private static void LoadPreviousSceneAfterBootstrap()
        {
            if (!EditorApplication.isPlaying || string.IsNullOrEmpty(PreviousScene))
            {
                return;
            }

            string currentScenePath = SceneManager.GetActiveScene().path;
            string bootstrapPath = BootstrapScenePath;

            if (!string.IsNullOrEmpty(bootstrapPath) && 
                currentScenePath.Equals(bootstrapPath, System.StringComparison.OrdinalIgnoreCase))
            {
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(PreviousScene);
                bool allComplete = AreAllBootstrappersComplete();
                Debug.Log($"[SceneBootstrapper] Bootstrap initialized ({_framesWaited} frames, {_timeWaited:F2}s, All complete: {allComplete}). Loading previous scene: {sceneName}");
                
                _hasLoadedPreviousScene = true;
                
                SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            }
        }
    }
}
