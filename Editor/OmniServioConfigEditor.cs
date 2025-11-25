using UnityEditor;
using UnityEngine;

namespace AtaYanki.OmniServio.Editor
{
    /// <summary>
    /// Custom editor for OmniServioConfig to provide helpful UI and validation.
    /// </summary>
    [CustomEditor(typeof(OmniServioConfig))]
    public class OmniServioConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty _defaultExceptionHandlerModeProp;
        private SerializedProperty _globalBootstrapSceneProp;
        private SerializedProperty _autoLoadBootstrapSceneInEditorProp;
        private SerializedProperty _loadBootstrapSceneAtRuntimeProp;

        private void OnEnable()
        {
            _defaultExceptionHandlerModeProp = serializedObject.FindProperty("defaultExceptionHandlerMode");
            _globalBootstrapSceneProp = serializedObject.FindProperty("globalBootstrapScene");
            _autoLoadBootstrapSceneInEditorProp = serializedObject.FindProperty("autoLoadBootstrapSceneInEditor");
            _loadBootstrapSceneAtRuntimeProp = serializedObject.FindProperty("loadBootstrapSceneAtRuntime");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("OmniServio Configuration", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Configure global settings for the OmniServio service locator system.",
                MessageType.Info);

            EditorGUILayout.Space();

            // Dependency Injection Settings
            EditorGUILayout.LabelField("Dependency Injection Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_defaultExceptionHandlerModeProp, new GUIContent("Exception Handler Mode"));
            EditorGUILayout.HelpBox(
                "Warning: Logs warnings when injection fails. Execution continues.\n" +
                "ThrowException: Throws exceptions when injection fails. Stops execution.",
                MessageType.None);

            EditorGUILayout.Space();

            // Bootstrap Scene Settings
            EditorGUILayout.LabelField("Bootstrap Scene Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_globalBootstrapSceneProp, new GUIContent("Global Bootstrap Scene"));
            
            if (_globalBootstrapSceneProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox(
                    "No bootstrap scene assigned. Create a scene with OmniServioGlobalBootstrapper component and assign it here.",
                    MessageType.Warning);
            }
            else
            {
                // Validate that the scene is in build settings
                string scenePath = AssetDatabase.GetAssetPath(_globalBootstrapSceneProp.objectReferenceValue);
                bool inBuildSettings = IsSceneInBuildSettings(scenePath);
                
                if (!inBuildSettings)
                {
                    EditorGUILayout.HelpBox(
                        "The bootstrap scene is not in Build Settings. Add it to ensure it loads correctly.",
                        MessageType.Warning);
                    
                    if (GUILayout.Button("Add to Build Settings"))
                    {
                        AddSceneToBuildSettings(scenePath);
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_autoLoadBootstrapSceneInEditorProp, 
                new GUIContent("Auto Load in Editor", 
                "Automatically load the bootstrap scene when entering Play mode in the editor."));
            
            EditorGUILayout.PropertyField(_loadBootstrapSceneAtRuntimeProp, 
                new GUIContent("Load at Runtime", 
                "Load the bootstrap scene when the game starts (for builds)."));

            EditorGUILayout.Space();

            // Apply button
            if (GUILayout.Button("Apply Configuration", GUILayout.Height(30)))
            {
                ApplyConfiguration();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ApplyConfiguration()
        {
            var config = (OmniServioConfig)target;
            
            // Apply exception handler mode immediately
            IInjectionExceptionHandler handler = config.DefaultExceptionHandlerMode switch
            {
                InjectionExceptionHandlerMode.ThrowException => new ThrowExceptionHandler(),
                InjectionExceptionHandlerMode.Warning => new WarningExceptionHandler(),
                _ => new WarningExceptionHandler()
            };

            DependencyInjector.SetExceptionHandler(handler);
            
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"[OmniServioConfig] Configuration applied. Exception Handler Mode: {config.DefaultExceptionHandlerMode}");
        }

        private bool IsSceneInBuildSettings(string scenePath)
        {
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.path == scenePath)
                {
                    return true;
                }
            }
            return false;
        }

        private void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($"[OmniServioConfig] Added scene to Build Settings: {scenePath}");
        }
    }

    /// <summary>
    /// Menu item to create a new OmniServioConfig asset.
    /// </summary>
    public static class OmniServioConfigCreator
    {
        [MenuItem("OmniServio/Config/Create Config Asset", priority = 2)]
        public static void CreateConfig()
        {
            // Check if config already exists
            string[] guids = AssetDatabase.FindAssets("t:OmniServioConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                EditorUtility.DisplayDialog(
                    "Config Already Exists",
                    $"A config already exists at:\n{path}\n\nOpening Settings window instead.",
                    "OK");
                OmniServioSettingsWindow.ShowWindow();
                return;
            }

            var config = ScriptableObject.CreateInstance<OmniServioConfig>();
            
            string pathToCreate = "Assets/OmniServioConfig.asset";
            
            // If Resources folder exists, prefer it
            string resourcesPath = "Assets/Resources";
            if (AssetDatabase.IsValidFolder(resourcesPath))
            {
                pathToCreate = "Assets/Resources/OmniServioConfig.asset";
            }
            else
            {
                // Create Resources folder
                string[] folders = resourcesPath.Split('/');
                string parentFolder = folders[0];
                for (int i = 1; i < folders.Length; i++)
                {
                    string currentFolder = $"{parentFolder}/{folders[i]}";
                    if (!AssetDatabase.IsValidFolder(currentFolder))
                    {
                        AssetDatabase.CreateFolder(parentFolder, folders[i]);
                    }
                    parentFolder = currentFolder;
                }
                pathToCreate = "Assets/Resources/OmniServioConfig.asset";
            }
            
            // Make sure the path doesn't already exist
            pathToCreate = AssetDatabase.GenerateUniqueAssetPath(pathToCreate);
            
            AssetDatabase.CreateAsset(config, pathToCreate);
            AssetDatabase.SaveAssets();
            
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = config;
            
            Debug.Log($"[OmniServioConfig] Created config asset at: {pathToCreate}");
            EditorUtility.DisplayDialog(
                "Config Created",
                $"Config created at:\n{pathToCreate}\n\nOpening Settings window.",
                "OK");
            
            OmniServioSettingsWindow.ShowWindow();
        }
    }
}

