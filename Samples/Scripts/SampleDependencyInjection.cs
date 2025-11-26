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
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            float minContentHeight = 315f;
            float panelWidth = Mathf.Min(screenWidth * 0.35f, 450f);

            float availableHeight = screenHeight * 0.85f;
            float panelHeight = Mathf.Max(minContentHeight, Mathf.Min(availableHeight, 600f));

            float panelX = screenWidth * 0.02f;
            float panelY = screenHeight * 0.02f;

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

