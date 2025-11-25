using UnityEngine;
using UnityEngine.SceneManagement;

namespace AtaYanki.OmniServio
{
    /// <summary>
    /// Runtime loader for bootstrap scenes.
    /// Automatically loads the bootstrap scene at game start if configured.
    /// </summary>
    public static class BootstrapSceneLoader
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadBootstrapScene()
        {
            var config = OmniServioConfig.Instance;
            if (config == null)
            {
                return;
            }

            if (!config.LoadBootstrapSceneAtRuntime)
            {
                return;
            }

            string sceneName = config.GetBootstrapSceneName();
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("[BootstrapSceneLoader] Bootstrap scene is configured but scene name is empty.");
                return;
            }

            // Check if bootstrap scene is already loaded
            Scene bootstrapScene = SceneManager.GetSceneByName(sceneName);
            if (bootstrapScene.IsValid() && bootstrapScene.isLoaded)
            {
                Debug.Log($"[BootstrapSceneLoader] Bootstrap scene '{sceneName}' is already loaded.");
                return;
            }

            // Load the bootstrap scene additively
            Debug.Log($"[BootstrapSceneLoader] Loading bootstrap scene: {sceneName}");
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }
    }
}

