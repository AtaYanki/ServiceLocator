using UnityEngine;
using AtaYanki.OmniServio;

namespace AtaYanki.OmniServio.Samples
{
    #region Service Interfaces

    /// <summary>
    /// Example audio service interface for playing sounds.
    /// </summary>
    public interface IAudioService
    {
        void PlaySound(string soundName);
        void PlayMusic(string musicName);
        void SetVolume(float volume);
    }

    /// <summary>
    /// Example save service interface for managing game saves.
    /// </summary>
    public interface ISaveService
    {
        void SaveGame(string saveName);
        void LoadGame(string saveName);
        bool HasSave(string saveName);
    }

    /// <summary>
    /// Example gameplay service interface for scene-specific game logic.
    /// </summary>
    public interface IGameplayService
    {
        void StartGame();
        void EndGame();
        int GetScore();
    }

    #endregion

    #region Service Implementations

    /// <summary>
    /// Example implementation of IAudioService.
    /// </summary>
    public class AudioService : IAudioService
    {
        private float _volume = 1.0f;

        public void PlaySound(string soundName)
        {
            Debug.Log($"[AudioService] Playing sound: {soundName} at volume {_volume}");
        }

        public void PlayMusic(string musicName)
        {
            Debug.Log($"[AudioService] Playing music: {musicName} at volume {_volume}");
        }

        public void SetVolume(float volume)
        {
            _volume = Mathf.Clamp01(volume);
            Debug.Log($"[AudioService] Volume set to {_volume}");
        }
    }

    /// <summary>
    /// Example implementation of ISaveService.
    /// </summary>
    public class SaveService : ISaveService
    {
        public void SaveGame(string saveName)
        {
            Debug.Log($"[SaveService] Saving game: {saveName}");
            PlayerPrefs.SetString($"Save_{saveName}", "saved_data");
            PlayerPrefs.Save();
        }

        public void LoadGame(string saveName)
        {
            if (HasSave(saveName))
            {
                Debug.Log($"[SaveService] Loading game: {saveName}");
                string data = PlayerPrefs.GetString($"Save_{saveName}");
            }
            else
            {
                Debug.LogWarning($"[SaveService] Save file not found: {saveName}");
            }
        }

        public bool HasSave(string saveName)
        {
            return PlayerPrefs.HasKey($"Save_{saveName}");
        }
    }

    /// <summary>
    /// Example implementation of IGameplayService.
    /// </summary>
    public class GameplayService : IGameplayService
    {
        private int _score = 0;
        private bool _isGameActive = false;

        public void StartGame()
        {
            _isGameActive = true;
            _score = 0;
            Debug.Log("[GameplayService] Game started!");
        }

        public void EndGame()
        {
            _isGameActive = false;
            Debug.Log($"[GameplayService] Game ended! Final score: {_score}");
        }

        public int GetScore()
        {
            return _score;
        }

        public void AddScore(int points)
        {
            if (_isGameActive)
            {
                _score += points;
                Debug.Log($"[GameplayService] Score updated: {_score}");
            }
        }
    }

    #endregion

    #region Updatable Services

    /// <summary>
    /// Example service that implements IUpdatable to receive Update() callbacks.
    /// </summary>
    public class GameTimer : IUpdatable
    {
        private float _elapsedTime = 0f;
        private bool _isRunning = false;

        public void Start()
        {
            _isRunning = true;
            _elapsedTime = 0f;
        }

        public void Stop()
        {
            _isRunning = false;
        }

        public void Update(float deltaTime)
        {
            if (_isRunning)
            {
                _elapsedTime += deltaTime;
                
                // Log each second
                int currentSecond = Mathf.FloorToInt(_elapsedTime);
                int previousSecond = Mathf.FloorToInt(_elapsedTime - deltaTime);
                if (currentSecond > previousSecond)
                {
                    Debug.Log($"[GameTimer] Elapsed time: {currentSecond} seconds");
                }
            }
        }

        public float GetElapsedTime() => _elapsedTime;
        public bool IsRunning() => _isRunning;
    }

    /// <summary>
    /// Example service that implements IFixedUpdatable to receive FixedUpdate() callbacks.
    /// </summary>
    public class PhysicsMonitor : IFixedUpdatable
    {
        private int _fixedUpdateCount = 0;

        public void FixedUpdate(float deltaTime)
        {
            _fixedUpdateCount++;
            // Log every 50 fixed updates to avoid spam
            if (_fixedUpdateCount % 50 == 0)
            {
                Debug.Log($"[PhysicsMonitor] FixedUpdate called {_fixedUpdateCount} times (deltaTime: {deltaTime})");
            }
        }

        public int GetFixedUpdateCount() => _fixedUpdateCount;
    }

    /// <summary>
    /// Example service that implements ILateUpdatable to receive LateUpdate() callbacks.
    /// </summary>
    public class CameraFollowService : ILateUpdatable
    {
        private Transform _target;
        private Vector3 _offset = new Vector3(0, 5, -10);

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public void LateUpdate(float deltaTime)
        {
            if (_target != null)
            {
                Vector3 desiredPosition = _target.position + _offset;
                Debug.Log($"[CameraFollowService] Following target at {desiredPosition}");
            }
        }
    }

    #endregion

    #region Destroyable Services

    /// <summary>
    /// Example service that implements IDestroyable to receive cleanup callbacks.
    /// </summary>
    public class NetworkService : IDestroyable
    {
        private bool _isConnected = false;

        public void Connect()
        {
            _isConnected = true;
            Debug.Log("[NetworkService] Connected to network");
        }

        public void Disconnect()
        {
            _isConnected = false;
            Debug.Log("[NetworkService] Disconnected from network");
        }

        public void Destroy()
        {
            if (_isConnected)
            {
                Disconnect();
            }
            Debug.Log("[NetworkService] Network service destroyed and cleaned up");
        }
    }

    #endregion
}

