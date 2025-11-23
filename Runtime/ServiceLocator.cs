using System;
using UnityEngine;
using System.Linq;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace AtaYanki.ServiceLocator
{
    public class ServiceLocator : MonoBehaviour
    {
        private static ServiceLocator _globalServiceLocator;
        private static Dictionary<Scene, ServiceLocator> _sceneServiceLocators;
        private static List<GameObject> _tempSceneGameObjects;

        private const string GlobalServiceLocatorName = "ServiceLocator [Global]";
        private const string SceneServiceLocatorName = "ServiceLocator [Scene]";

        private readonly ServiceManager _serviceManager = new ServiceManager();
        private readonly UpdateManager _updateManager = new UpdateManager();
        private readonly DestroyManager _destroyManager = new DestroyManager();

        public static ServiceLocator Global
        {
            get 
            {
                if (_globalServiceLocator != null) return _globalServiceLocator;

                // Bootstrap or initialize the new instance of global as necessary.
                if (FindFirstObjectByType<ServiceLocatorGlobalBootstrapper>() is {} serviceLocatorGlobalBootstrapper)
                {
                    serviceLocatorGlobalBootstrapper.BootstrapOnDemand();
                    return _globalServiceLocator;
                }

                GameObject container = new GameObject(GlobalServiceLocatorName, typeof(ServiceLocator));
                container.AddComponent<ServiceLocatorGlobalBootstrapper>().BootstrapOnDemand();

                return _globalServiceLocator;
            }
        }

        internal void ConfigureAsGlobal(bool dontDestroyOnLoad)
        {
            if (_globalServiceLocator == this)
            {
                Debug.LogWarning($"[ServiceLocator].ConfigureAsGlobal - Already configured as global", this);
            }
            else if (_globalServiceLocator != null)
            {
                Debug.LogError($"[ServiceLocator].ConfigureAsGlobal - Another ServiceLocator is already configured as global", this);
            }
            else
            {
                _globalServiceLocator = this;
                if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
            }
        }

        internal void ConfigureForScene()
        {
            Scene scene = gameObject.scene;

            if (_sceneServiceLocators.ContainsKey(scene))
            {
                Debug.LogError($"[ServiceLocator].ConfigureForScene - Another ServiceLocator is already configured for this scene", this);
                return;
            }

            _sceneServiceLocators.Add(scene, this);
        }

        public static ServiceLocator ForSceneOf(MonoBehaviour monoBehaviour)
        {
            Scene scene = monoBehaviour.gameObject.scene;

            if (_sceneServiceLocators.TryGetValue(scene, out ServiceLocator serviceLocator) && serviceLocator != monoBehaviour)
            {
                return serviceLocator;
            }

            _tempSceneGameObjects.Clear();
            scene.GetRootGameObjects(_tempSceneGameObjects);

            foreach (var tempSceneGameObject in _tempSceneGameObjects.Where(tempSceneGameObject => tempSceneGameObject.GetComponent<ServiceLocatorSceneBootstrapper>() != null))
            {
                if (tempSceneGameObject.TryGetComponent(out ServiceLocatorSceneBootstrapper serviceLocatorSceneBootstrapper) && serviceLocatorSceneBootstrapper.ServiceLocator != monoBehaviour)
                {
                    serviceLocatorSceneBootstrapper.BootstrapOnDemand();
                    return serviceLocatorSceneBootstrapper.ServiceLocator;
                }
            }

            return Global;
        }

        public static ServiceLocator For(MonoBehaviour monoBehaviour)
        {
            return monoBehaviour.GetComponentInParent<ServiceLocator>().OrNull() ?? ForSceneOf(monoBehaviour) ?? Global;
        }

        public ServiceLocator Register<T>(T service)
        {
            _serviceManager.Register(service);
            return this;
        }

        public ServiceLocator Register(Type type, object service)
        {
            _serviceManager.Register(type, service);
            return this;
        }

        #region Register Updatable

        public ServiceLocator RegisterUpdatable<T>(T service)
        {
            _updateManager.RegisterUpdatable(service);
            return this;
        }

        public ServiceLocator RegisterUpdatable(Type type, object service)
        {
            _updateManager.RegisterUpdatable(type);
            return this;
        }

        #endregion

        #region Register FixedUpdatable

        public ServiceLocator RegisterFixedUpdatable<T>(T service)
        {
            _updateManager.RegisterFixedUpdatable(service);
            return this;
        }

        public ServiceLocator RegisterFixedUpdatable(Type type, object service)
        {
            _updateManager.RegisterFixedUpdatable(type);
            return this;
        }

        #endregion

        #region Register LateUpdatable

        public ServiceLocator RegisterLateUpdatable<T>(T service)
        {
            _updateManager.RegisterLateUpdatable(service);
            return this;
        }

        public ServiceLocator RegisterLateUpdatable(Type type, object service)
        {
            _updateManager.RegisterLateUpdatable(type);
            return this;
        }

        #endregion

        #region Register Destroyable

        public ServiceLocator RegisterDestroyable<T>(T service)
        {
            _destroyManager.RegisterDestroyable(service);
            return this;
        }

        public ServiceLocator RegisterDestroyable(Type type, object service)
        {
            _destroyManager.RegisterDestroyable(service);
            return this;
        }

        #endregion

        public ServiceLocator Get<T>(out T service) where T : class
        {
            if (TryGetService(out service)) return this;

            if (TryGetNextInHierarchy(out ServiceLocator serviceLocator))
            {
                serviceLocator.Get(out service);
                return this;
            }

            throw new ArgumentException($"[ServiceLocator].Get - Service of type {typeof(T).FullName} not registered.");
        }

        private bool TryGetService<T>(out T service) where T : class
        {
            return _serviceManager.TryGet(out service);
        }

        private bool TryGetNextInHierarchy(out ServiceLocator serviceLocator)
        {
            if (this == _globalServiceLocator)
            {
                serviceLocator = null;
                return false;
            }

            serviceLocator = transform.parent.OrNull()?.GetComponentInParent<ServiceLocator>().OrNull() ?? ForSceneOf(this);
            return serviceLocator != null;
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

            if (this == _globalServiceLocator)
            {
                _globalServiceLocator = null;
            }
            else if (_sceneServiceLocators.ContainsValue(this))
            {
                _sceneServiceLocators.Remove(gameObject.scene);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _globalServiceLocator = null;
            _sceneServiceLocators = new Dictionary<Scene, ServiceLocator>();
            _tempSceneGameObjects = new List<GameObject>();
        }
    }
}