#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using AtaYanki.OmniServio;

namespace AtaYanki.OmniServio.Editor
{
    public static class OmniServioMenuItems
    {
        private const string GlobalOmniServioName = "OmniServio [Global]";
        private const string SceneOmniServioName = "OmniServio [Scene]";

        [MenuItem("GameObject/OmniServio/Add Global")]
        private static void AddGlobal()
        {
            GameObject globalOmniServio = new GameObject(GlobalOmniServioName, typeof(OmniServioGlobalBootstrapper));
            Selection.activeGameObject = globalOmniServio;
        }

        [MenuItem("GameObject/OmniServio/Add Scene")]
        private static void AddScene()
        {
            GameObject sceneOmniServio = new GameObject(SceneOmniServioName, typeof(OmniServioSceneBootstrapper));
            Selection.activeGameObject = sceneOmniServio;
        }
    }
}
#endif

