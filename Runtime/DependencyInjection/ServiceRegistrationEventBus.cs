using System;
using UnityEngine;
using System.Collections.Generic;

namespace AtaYanki.OmniServio
{
    public class ServiceRegisteredEventArgs : EventArgs
    {
        public Type ServiceType { get; }
        public object Service { get; }
        public OmniServio OmniServio { get; }

        public ServiceRegisteredEventArgs(Type serviceType, object service, OmniServio omniServio)
        {
            ServiceType = serviceType;
            Service = service;
            OmniServio = omniServio;
        }
    }

    public static class ServiceRegistrationEventBus
    {
        private static readonly Dictionary<Type, List<Action<ServiceRegisteredEventArgs>>> _subscribers = 
            new Dictionary<Type, List<Action<ServiceRegisteredEventArgs>>>();

        public static void Subscribe<T>(Action<ServiceRegisteredEventArgs> callback) where T : class
        {
            Type serviceType = typeof(T);
            
            if (!_subscribers.TryGetValue(serviceType, out List<Action<ServiceRegisteredEventArgs>> callbacks))
            {
                callbacks = new List<Action<ServiceRegisteredEventArgs>>();
                _subscribers[serviceType] = callbacks;
            }

            if (!callbacks.Contains(callback))
            {
                callbacks.Add(callback);
            }
        }

        public static void Unsubscribe<T>(Action<ServiceRegisteredEventArgs> callback) where T : class
        {
            Type serviceType = typeof(T);
            
            if (_subscribers.TryGetValue(serviceType, out List<Action<ServiceRegisteredEventArgs>> callbacks))
            {
                callbacks.Remove(callback);
                
                if (callbacks.Count == 0)
                {
                    _subscribers.Remove(serviceType);
                }
            }
        }

        internal static void Publish(Type serviceType, object service, OmniServio omniServio)
        {
            if (!_subscribers.TryGetValue(serviceType, out List<Action<ServiceRegisteredEventArgs>> callbacks))
            {
                return;
            }

            var eventArgs = new ServiceRegisteredEventArgs(serviceType, service, omniServio);

            var callbacksCopy = new List<Action<ServiceRegisteredEventArgs>>(callbacks);
            
            foreach (var callback in callbacksCopy)
            {
                try
                {
                    callback?.Invoke(eventArgs);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ServiceRegistrationEventBus] Error invoking callback for {serviceType.Name}: {ex.Message}");
                }
            }
        }

        public static void Clear()
        {
            _subscribers.Clear();
        }

        public static int GetSubscriberCount<T>() where T : class
        {
            Type serviceType = typeof(T);
            return _subscribers.TryGetValue(serviceType, out List<Action<ServiceRegisteredEventArgs>> callbacks) 
                ? callbacks.Count 
                : 0;
        }
    }
}

