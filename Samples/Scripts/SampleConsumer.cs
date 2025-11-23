using UnityEngine;
using AtaYanki.ServiceLocator;
using AtaYanki.ServiceLocator.Samples;

namespace AtaYanki.ServiceLocator.Samples
{
    /// <summary>
    /// Example MonoBehaviour that consumes services from the ServiceLocator.
    /// Demonstrates different ways to retrieve and use services.
    /// </summary>
    public class SampleConsumer : MonoBehaviour
    {
        [Header("Service References")]
        [SerializeField] private bool useGlobalServices = true;
        [SerializeField] private bool useSceneServices = true;

        // Cached service references
        private IAudioService _audioService;
        private ISaveService _saveService;
        private IGameplayService _gameplayService;
        private GameTimer _gameTimer;

        private void Start()
        {
            // Method 1: Using ServiceLocator.For(this) - automatically resolves hierarchy
            // This is the recommended approach as it checks:
            // 1. Parent hierarchy
            // 2. Scene ServiceLocator
            // 3. Global ServiceLocator
            ServiceLocator locator = ServiceLocator.For(this);

            if (useGlobalServices)
            {
                // Retrieve global services
                try
                {
                    locator.Get(out _audioService);
                    Debug.Log("[SampleConsumer] Retrieved IAudioService using ServiceLocator.For(this)");

                    locator.Get(out _saveService);
                    Debug.Log("[SampleConsumer] Retrieved ISaveService using ServiceLocator.For(this)");

                    locator.Get(out _gameTimer);
                    Debug.Log("[SampleConsumer] Retrieved GameTimer using ServiceLocator.For(this)");
                }
                catch (System.ArgumentException ex)
                {
                    Debug.LogWarning($"[SampleConsumer] Service not found: {ex.Message}");
                }
            }

            if (useSceneServices)
            {
                // Retrieve scene-specific services
                try
                {
                    locator.Get(out _gameplayService);
                    Debug.Log("[SampleConsumer] Retrieved IGameplayService using ServiceLocator.For(this)");
                }
                catch (System.ArgumentException ex)
                {
                    Debug.LogWarning($"[SampleConsumer] Service not found: {ex.Message}");
                }
            }

            // Demonstrate service usage
            UseServices();
        }

        private void UseServices()
        {
            // Use audio service
            _audioService?.PlaySound("startup");
            _audioService?.SetVolume(0.8f);

            // Use save service
            _saveService?.SaveGame("autosave");

            // Use gameplay service
            _gameplayService?.StartGame();
        }

        private void Update()
        {
            // Example: Use services in Update loop
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _audioService?.PlaySound("jump");
                _gameplayService?.StartGame();
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                _saveService?.SaveGame("manual_save");
            }

            // Display timer if available
            if (_gameTimer != null && _gameTimer.IsRunning())
            {
                // Timer is automatically updated by UpdateManager
                // You can access it here if needed
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Service Locator Sample", GUI.skin.box);

            if (GUILayout.Button("Play Sound"))
            {
                _audioService?.PlaySound("button_click");
            }

            if (GUILayout.Button("Save Game"))
            {
                _saveService?.SaveGame("gui_save");
            }

            if (GUILayout.Button("Start Game"))
            {
                _gameplayService?.StartGame();
            }

            if (GUILayout.Button("End Game"))
            {
                _gameplayService?.EndGame();
            }

            if (_gameTimer != null)
            {
                GUILayout.Label($"Timer: {_gameTimer.GetElapsedTime():F2}s");
            }

            GUILayout.EndArea();
        }
    }

    /// <summary>
    /// Example of directly accessing the global ServiceLocator.
    /// Use this when you know the service is always global.
    /// </summary>
    public class SampleGlobalConsumer : MonoBehaviour
    {
        private IAudioService _audioService;

        private void Start()
        {
            // Method 2: Direct global access
            // Use this when you're certain the service is registered globally
            try
            {
                ServiceLocator.Global.Get(out _audioService);
                Debug.Log("[SampleGlobalConsumer] Retrieved IAudioService from Global ServiceLocator");
            }
            catch (System.ArgumentException ex)
            {
                Debug.LogWarning($"[SampleGlobalConsumer] Service not found: {ex.Message}");
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                _audioService?.PlaySound("global_sound");
            }
        }
    }

    /// <summary>
    /// Example of accessing scene-specific ServiceLocator.
    /// Use this when you need services specific to the current scene.
    /// </summary>
    public class SampleSceneConsumer : MonoBehaviour
    {
        private IGameplayService _gameplayService;

        private void Start()
        {
            // Method 3: Scene-specific access
            // Use this when you need services from the scene ServiceLocator
            try
            {
                ServiceLocator sceneLocator = ServiceLocator.ForSceneOf(this);
                sceneLocator.Get(out _gameplayService);
                Debug.Log("[SampleSceneConsumer] Retrieved IGameplayService from Scene ServiceLocator");
            }
            catch (System.ArgumentException ex)
            {
                Debug.LogWarning($"[SampleSceneConsumer] Service not found: {ex.Message}");
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                _gameplayService?.StartGame();
            }
        }
    }
}

