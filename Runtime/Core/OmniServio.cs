using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace AtaYanki.OmniServio
{
    public class OmniServio : MonoBehaviour
    {
        private static OmniServio _globalOmniServio;
        private static Dictionary<Scene, OmniServio> _sceneOmniServios;
        private static List<GameObject> _tempSceneGameObjects;

        private const string GlobalOmniServioName = "OmniServio [Global]";
        private const string SceneOmniServioName = "OmniServio [Scene]";

        private readonly ServiceManager _serviceManager = new ServiceManager();
        private readonly UpdateManager _updateManager = new UpdateManager();
        private readonly DestroyManager _destroyManager = new DestroyManager();

        public static OmniServio Global
        {
            get 
            {
                if (_globalOmniServio != null) return _globalOmniServio;

                // Bootstrap or initialize the new instance of global as necessary.
                if (FindFirstObjectByType<OmniServioGlobalBootstrapper>() is {} omniServioGlobalBootstrapper)
                {
                    omniServioGlobalBootstrapper.BootstrapOnDemand();
                    return _globalOmniServio;
                }

                GameObject container = new GameObject(GlobalOmniServioName, typeof(OmniServio));
                container.AddComponent<OmniServioGlobalBootstrapper>().BootstrapOnDemand();

                return _globalOmniServio;
            }
        }

        internal void ConfigureAsGlobal(bool dontDestroyOnLoad)
        {
            if (_globalOmniServio == this)
            {
                Debug.LogWarning($"[OmniServio].ConfigureAsGlobal - Already configured as global", this);
            }
            else if (_globalOmniServio != null)
            {
                Debug.LogError($"[OmniServio].ConfigureAsGlobal - Another OmniServio is already configured as global", this);
            }
            else
            {
                _globalOmniServio = this;
                if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
            }
        }

        internal void ConfigureForScene()
        {
            Scene scene = gameObject.scene;

            if (_sceneOmniServios.ContainsKey(scene))
            {
                Debug.LogError($"[OmniServio].ConfigureForScene - Another OmniServio is already configured for this scene", this);
                return;
            }

            _sceneOmniServios.Add(scene, this);
        }

        public static OmniServio ForSceneOf(MonoBehaviour monoBehaviour)
        {
            Scene scene = monoBehaviour.gameObject.scene;

            if (_sceneOmniServios.TryGetValue(scene, out OmniServio omniServio) && omniServio != monoBehaviour)
            {
                return omniServio;
            }

            _tempSceneGameObjects.Clear();
            scene.GetRootGameObjects(_tempSceneGameObjects);

            foreach (var tempSceneGameObject in _tempSceneGameObjects.Where(tempSceneGameObject => tempSceneGameObject.GetComponent<OmniServioSceneBootstrapper>() != null))
            {
                if (tempSceneGameObject.TryGetComponent(out OmniServioSceneBootstrapper omniServioSceneBootstrapper) && omniServioSceneBootstrapper.OmniServio != monoBehaviour)
                {
                    omniServioSceneBootstrapper.BootstrapOnDemand();
                    return omniServioSceneBootstrapper.OmniServio;
                }
            }

            return Global;
        }

        public static OmniServio For(MonoBehaviour monoBehaviour)
        {
            return monoBehaviour.GetComponentInParent<OmniServio>().OrNull() ?? ForSceneOf(monoBehaviour) ?? Global;
        }

        public OmniServio Register<T>(T service)
        {
            _serviceManager.Register(service);
            return this;
        }

        public OmniServio Register(Type type, object service)
        {
            _serviceManager.Register(type, service);
            return this;
        }

        #region Register Updatable

        public OmniServio RegisterUpdatable<T>(T service)
        {
            _serviceManager.Register(typeof(T), service);
            _updateManager.RegisterUpdatable(service);
            return this;
        }

        public OmniServio RegisterUpdatable(Type type, object service)
        {
            _serviceManager.Register(type, service);
            _updateManager.RegisterUpdatable(type, service);
            return this;
        }

        #endregion

        #region Register FixedUpdatable

        public OmniServio RegisterFixedUpdatable<T>(T service)
        {
            _serviceManager.Register(typeof(T), service);
            _updateManager.RegisterFixedUpdatable(service);
            return this;
        }

        public OmniServio RegisterFixedUpdatable(Type type, object service)
        {
            _serviceManager.Register(type, service);
            _updateManager.RegisterFixedUpdatable(type, service);
            return this;
        }

        #endregion

        #region Register LateUpdatable

        public OmniServio RegisterLateUpdatable<T>(T service)
        {
            _serviceManager.Register(typeof(T), service);
            _updateManager.RegisterLateUpdatable(service);
            return this;
        }

        public OmniServio RegisterLateUpdatable(Type type, object service)
        {
            _serviceManager.Register(type, service);
            _updateManager.RegisterLateUpdatable(type, service);
            return this;
        }

        #endregion

        #region Register Destroyable

        public OmniServio RegisterDestroyable<T>(T service)
        {
            _serviceManager.Register(typeof(T), service);
            _destroyManager.RegisterDestroyable(service);
            return this;
        }

        public OmniServio RegisterDestroyable(Type type, object service)
        {
            _serviceManager.Register(type, service);
            _destroyManager.RegisterDestroyable(service);
            return this;
        }

        #endregion

        public OmniServio Get<T>(out T service) where T : class
        {
            if (TryGetService(out service)) return this;

            if (TryGetNextInHierarchy(out OmniServio omniServio))
            {
                omniServio.Get(out service);
                return this;
            }

            throw new ArgumentException($"[OmniServio].Get - Service of type {typeof(T).FullName} not registered.");
        }

        /// <summary>
        /// Tries to get a service without throwing an exception.
        /// Returns true if the service was found, false otherwise.
        /// </summary>
        public bool TryGet<T>(out T service) where T : class
        {
            if (TryGetService(out service)) return true;

            if (TryGetNextInHierarchy(out OmniServio omniServio))
            {
                return omniServio.TryGet(out service);
            }

            service = null;
            return false;
        }

        private bool TryGetService<T>(out T service) where T : class
        {
            return _serviceManager.TryGet(out service);
        }

        private bool TryGetNextInHierarchy(out OmniServio omniServio)
        {
            if (this == _globalOmniServio)
            {
                omniServio = null;
                return false;
            }

            omniServio = transform.parent.OrNull()?.GetComponentInParent<OmniServio>().OrNull() ?? ForSceneOf(this);
            return omniServio != null;
        }

        private void Update()
        {
            _updateManager.UpdateAll();
        }

        private void FixedUpdate()
        {
            _updateManager.FixedUpdateAll();
        }

        private void LateUpdate()
        {
            _updateManager.LateUpdateAll();
        }

        private void OnDestroy()
        {
            _destroyManager.DestroyAll();

            if (this == _globalOmniServio)
            {
                _globalOmniServio = null;
            }
            else if (_sceneOmniServios.ContainsValue(this))
            {
                _sceneOmniServios.Remove(gameObject.scene);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _globalOmniServio = null;
            _sceneOmniServios = new Dictionary<Scene, OmniServio>();
            _tempSceneGameObjects = new List<GameObject>();
        }
    }
}

