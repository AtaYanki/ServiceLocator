#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using AtaYanki.ServiceLocator;

namespace AtaYanki.ServiceLocator.Editor
{
    public static class ServiceLocatorMenuItems
    {
        private const string GlobalServiceLocatorName = "ServiceLocator [Global]";
        private const string SceneServiceLocatorName = "ServiceLocator [Scene]";

        [MenuItem("GameObject/ServiceLocator/Add Global")]
        private static void AddGlobal()
        {
            GameObject globalServiceLocator = new GameObject(GlobalServiceLocatorName, typeof(ServiceLocatorGlobalBootstrapper));
            Selection.activeGameObject = globalServiceLocator;
        }

        [MenuItem("GameObject/ServiceLocator/Add Scene")]
        private static void AddScene()
        {
            GameObject sceneServiceLocator = new GameObject(SceneServiceLocatorName, typeof(ServiceLocatorSceneBootstrapper));
            Selection.activeGameObject = sceneServiceLocator;
        }
    }
}
#endif

