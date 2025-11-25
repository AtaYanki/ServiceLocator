using UnityEngine;

namespace AtaYanki.OmniServio.Samples
{
    public class SampleDependencyInjection : MonoBehaviour
    {
        #region Field Injection Examples

        [Inject] private IAudioService _audioService;
        [Inject(UseGlobal = true)] private ISaveService _globalSaveService;
        [Inject] private IGameplayService _gameplayService;
        [Inject(UseGlobal = true)] private GameTimer _gameTimer;

        #endregion

        #region Property Injection Examples

        [Inject] public IAudioService AudioService { get; private set; }
        [Inject(UseGlobal = true)] public ISaveService SaveService { get; private set; }

        #endregion

        private void Start()
        {
            LogInjectionResults();
            UseInjectedServices();
        }


        private void LogInjectionResults()
        {
            Debug.Log($"IAudioService injected: {_audioService != null}");
            Debug.Log($"ISaveService (global) injected: {_globalSaveService != null}");
            Debug.Log($"IGameplayService injected: {_gameplayService != null}");
            Debug.Log($"GameTimer injected: {_gameTimer != null}");
        }

        private void UseInjectedServices()
        {
            _audioService?.PlaySound("injection_demo");

            AudioService?.SetVolume(0.75f);

            _globalSaveService?.SaveGame("injection_test");

            _gameplayService?.StartGame();

            if (_gameTimer != null)
            {
                _gameTimer.Start();
                Debug.Log($"GameTimer started. Current time: {_gameTimer.GetElapsedTime():F2}s");
            }
        }

        private void Update()
        {
            // Example: Use injected services in Update loop
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _audioService?.PlaySound("space_pressed");
                _gameplayService?.StartGame();
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                _globalSaveService?.SaveGame("manual_save");
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                if (_gameTimer != null)
                {
                    Debug.Log($"Timer elapsed: {_gameTimer.GetElapsedTime():F2}s");
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("Manually re-injecting dependencies...");
                DependencyInjector.Inject(this);
                LogInjectionResults();
            }
        }

        private void OnGUI()
        {
            // Calculate responsive GUI size based on screen dimensions
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            
            // Calculate minimum required height for all content
            // Header: ~25px, Status section: ~120px, Controls section: ~100px, Timer: ~30px, Spacing: ~40px
            float minContentHeight = 315f;
            
            // Use percentage-based sizing for responsiveness
            float panelWidth = Mathf.Min(screenWidth * 0.35f, 450f); // 35% of screen width, max 450px
            
            // Dynamic height: use minimum content height or available screen space (whichever is larger)
            // But don't exceed 85% of screen height to leave some margin
            float availableHeight = screenHeight * 0.85f;
            float panelHeight = Mathf.Max(minContentHeight, Mathf.Min(availableHeight, 600f));
            
            // Always position in top-left corner regardless of screen size
            float panelX = screenWidth * 0.02f; // 2% from left edge
            float panelY = screenHeight * 0.02f; // 2% from top edge
            
            GUILayout.BeginArea(new Rect(panelX, panelY, panelWidth, panelHeight));
            
            GUILayout.Label("Dependency Injection Sample", GUI.skin.box);

            GUILayout.Space(10);
            GUILayout.Label("=== Injection Status ===", GUI.skin.box);
            GUILayout.Label($"Audio Service: {(_audioService != null ? "✓ Injected" : "✗ Not Injected")}");
            GUILayout.Label($"Save Service (Global): {(_globalSaveService != null ? "✓ Injected" : "✗ Not Injected")}");
            GUILayout.Label($"Gameplay Service: {(_gameplayService != null ? "✓ Injected" : "✗ Not Injected")}");
            GUILayout.Label($"Game Timer: {(_gameTimer != null ? "✓ Injected" : "✗ Not Injected")}");

            GUILayout.Space(10);
            GUILayout.Label("=== Controls ===", GUI.skin.box);
            GUILayout.Label("SPACE - Play sound & start game");
            GUILayout.Label("S - Save game");
            GUILayout.Label("T - Show timer");
            GUILayout.Label("R - Re-inject dependencies");

            GUILayout.Space(10);
            if (_gameTimer != null)
            {
                GUILayout.Label($"Timer: {_gameTimer.GetElapsedTime():F2}s", GUI.skin.box);
            }

            GUILayout.EndArea();
        }
    }
}

