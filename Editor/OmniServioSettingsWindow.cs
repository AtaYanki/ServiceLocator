using UnityEditor;
using UnityEngine;

namespace AtaYanki.OmniServio.Editor
{
    public class OmniServioSettingsWindow : EditorWindow
    {
        private OmniServioConfig _config;
        private Vector2 _scrollPosition;
        private bool _configNeedsSave = false;

        private const string WindowTitle = "OmniServio Settings";
        private const float MinWindowWidth = 400f;
        private const float MinWindowHeight = 500f;

        [MenuItem("OmniServio/Config/Settings", priority = 1)]
        public static void ShowWindow()
        {
            OmniServioSettingsWindow window = GetWindow<OmniServioSettingsWindow>(WindowTitle);
            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
            window.Show();
        }

        private void OnEnable()
        {
            LoadOrCreateConfig();
        }

        private void OnDisable()
        {
            SaveConfigIfNeeded();
        }

        private void OnGUI()
        {
            if (_config == null)
            {
                EditorGUILayout.HelpBox("Config not found. Click the button below to create one.", MessageType.Warning);
                if (GUILayout.Button("Create Config", GUILayout.Height(30)))
                {
                    CreateConfig();
                }
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("OmniServio Configuration", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Configure global settings for the OmniServio service locator system.",
                MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.LabelField("Dependency Injection Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            InjectionExceptionHandlerMode currentMode = _config.DefaultExceptionHandlerMode;
            InjectionExceptionHandlerMode newMode = (InjectionExceptionHandlerMode)EditorGUILayout.EnumPopup(
                new GUIContent("Exception Handler Mode", 
                "Choose how to handle injection errors: Warning (logs warnings) or ThrowException (throws exceptions)"),
                currentMode);

            if (newMode != currentMode)
            {
                _config.SetExceptionHandlerMode(newMode);
                _configNeedsSave = true;
            }

            EditorGUILayout.HelpBox(
                "Warning: Logs warnings when injection fails. Execution continues.\n" +
                "ThrowException: Throws exceptions when injection fails. Stops execution.",
                MessageType.None);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.LabelField("Bootstrap Scene Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            SceneAsset currentScene = _config.GlobalBootstrapScene;
            SceneAsset newScene = (SceneAsset)EditorGUILayout.ObjectField(
                new GUIContent("Global Bootstrap Scene", 
                "Reference to the global bootstrap scene. This scene will be loaded first to initialize services."),
                currentScene,
                typeof(SceneAsset),
                false);

            if (EditorGUI.EndChangeCheck())
            {
                _config.SetGlobalBootstrapScene(newScene);
                _configNeedsSave = true;
            }

            if (newScene != null)
            {
                string scenePath = _config.GetBootstrapScenePath();
                bool inBuildSettings = IsSceneInBuildSettings(scenePath);

                if (!inBuildSettings)
                {
                    EditorGUILayout.HelpBox(
                        "The bootstrap scene is not in Build Settings. Add it to ensure it loads correctly.",
                        MessageType.Warning);

                    if (GUILayout.Button("Add to Build Settings", GUILayout.Height(25)))
                    {
                        AddSceneToBuildSettings(scenePath);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        $"Bootstrap scene is configured: {newScene.name}\nPath: {scenePath}",
                        MessageType.Info);
                }
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "No bootstrap scene assigned. Create a scene with OmniServioGlobalBootstrapper component and assign it here.",
                    MessageType.Warning);
            }

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            bool autoLoadInEditor = _config.AutoLoadBootstrapSceneInEditor;
            bool newAutoLoadInEditor = EditorGUILayout.Toggle(
                new GUIContent("Auto Load in Editor", 
                "Automatically load the bootstrap scene when entering Play mode in the editor."),
                autoLoadInEditor);

            if (EditorGUI.EndChangeCheck())
            {
                SetAutoLoadInEditor(newAutoLoadInEditor);
                _configNeedsSave = true;
            }

            EditorGUI.BeginChangeCheck();
            bool loadAtRuntime = _config.LoadBootstrapSceneAtRuntime;
            bool newLoadAtRuntime = EditorGUILayout.Toggle(
                new GUIContent("Load at Runtime", 
                "Load the bootstrap scene when the game starts (for builds)."),
                loadAtRuntime);

            if (EditorGUI.EndChangeCheck())
            {
                SetLoadAtRuntime(newLoadAtRuntime);
                _configNeedsSave = true;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Apply Configuration", GUILayout.Height(30)))
            {
                ApplyConfiguration();
            }

            if (GUILayout.Button("Refresh Config", GUILayout.Height(30)))
            {
                LoadOrCreateConfig();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            string configPath = AssetDatabase.GetAssetPath(_config);
            EditorGUILayout.LabelField("Config Location:", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(configPath, EditorStyles.textField, GUILayout.Height(20));

            if (GUILayout.Button("Select Config Asset", GUILayout.Height(25)))
            {
                Selection.activeObject = _config;
                EditorGUIUtility.PingObject(_config);
            }

            EditorGUILayout.EndScrollView();

            if (_configNeedsSave)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Changes detected. Config will be saved automatically when window closes.", MessageType.Info);
            }
        }

        private void LoadOrCreateConfig()
        {
            _config = OmniServioConfig.Instance;

            if (_config == null)
            {
                CreateConfig();
            }
            else
            {
                _configNeedsSave = false;
            }
        }

        private void CreateConfig()
        {
            string[] guids = AssetDatabase.FindAssets("t:OmniServioConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _config = AssetDatabase.LoadAssetAtPath<OmniServioConfig>(path);
                Debug.Log($"[OmniServioSettingsWindow] Found existing config at: {path}");
                return;
            }

            _config = ScriptableObject.CreateInstance<OmniServioConfig>();

            string resourcesPath = "Assets/Resources";
            string configPath;

            if (AssetDatabase.IsValidFolder(resourcesPath))
            {
                configPath = "Assets/Resources/OmniServioConfig.asset";
            }
            else
            {
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
                configPath = "Assets/Resources/OmniServioConfig.asset";
            }

            configPath = AssetDatabase.GenerateUniqueAssetPath(configPath);

            AssetDatabase.CreateAsset(_config, configPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _configNeedsSave = false;

            Debug.Log($"[OmniServioSettingsWindow] Created config at: {configPath}");
            EditorUtility.DisplayDialog(
                "Config Created",
                $"OmniServio config has been created at:\n{configPath}\n\nYou can now configure your settings.",
                "OK");
        }

        private void SaveConfigIfNeeded()
        {
            if (_configNeedsSave && _config != null)
            {
                EditorUtility.SetDirty(_config);
                AssetDatabase.SaveAssets();
                _configNeedsSave = false;
                Debug.Log("[OmniServioSettingsWindow] Config saved.");
            }
        }

        private void ApplyConfiguration()
        {
            if (_config == null)
            {
                EditorUtility.DisplayDialog("Error", "Config not found. Please create one first.", "OK");
                return;
            }

            IInjectionExceptionHandler handler = _config.DefaultExceptionHandlerMode switch
            {
                InjectionExceptionHandlerMode.ThrowException => new ThrowExceptionHandler(),
                InjectionExceptionHandlerMode.Warning => new WarningExceptionHandler(),
                _ => new WarningExceptionHandler()
            };

            DependencyInjector.SetExceptionHandler(handler);

            SaveConfigIfNeeded();

            Debug.Log($"[OmniServioSettingsWindow] Configuration applied. Exception Handler Mode: {_config.DefaultExceptionHandlerMode}");
            EditorUtility.DisplayDialog(
                "Configuration Applied",
                $"Exception Handler Mode: {_config.DefaultExceptionHandlerMode}\n\nSettings have been applied successfully.",
                "OK");
        }

        private void SetAutoLoadInEditor(bool value)
        {
            if (_config == null) return;

            SerializedObject serializedObject = new SerializedObject(_config);
            SerializedProperty property = serializedObject.FindProperty("autoLoadBootstrapSceneInEditor");
            if (property != null)
            {
                property.boolValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void SetLoadAtRuntime(bool value)
        {
            if (_config == null) return;

            SerializedObject serializedObject = new SerializedObject(_config);
            SerializedProperty property = serializedObject.FindProperty("loadBootstrapSceneAtRuntime");
            if (property != null)
            {
                property.boolValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }

        private bool IsSceneInBuildSettings(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                return false;
            }

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
            Debug.Log($"[OmniServioSettingsWindow] Added scene to Build Settings: {scenePath}");
            EditorUtility.DisplayDialog(
                "Scene Added",
                $"Scene has been added to Build Settings:\n{scenePath}",
                "OK");
        }
    }
}

