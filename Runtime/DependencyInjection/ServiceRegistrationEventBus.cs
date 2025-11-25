using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtaYanki.OmniServio
{
    /// <summary>
    /// Event arguments for service registration events.
    /// </summary>
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

    /// <summary>
    /// Event bus for service registration events.
    /// Allows components to subscribe to notifications when specific services are registered at runtime.
    /// </summary>
    public static class ServiceRegistrationEventBus
    {
        private static readonly Dictionary<Type, List<Action<ServiceRegisteredEventArgs>>> _subscribers = 
            new Dictionary<Type, List<Action<ServiceRegisteredEventArgs>>>();

        /// <summary>
        /// Subscribes to notifications when a service of type T is registered.
        /// </summary>
        /// <typeparam name="T">The type of service to listen for.</typeparam>
        /// <param name="callback">Callback to invoke when the service is registered.</param>
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

        /// <summary>
        /// Unsubscribes from notifications for a service of type T.
        /// </summary>
        /// <typeparam name="T">The type of service to stop listening for.</typeparam>
        /// <param name="callback">Callback to remove.</param>
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

        /// <summary>
        /// Publishes a service registration event.
        /// Called automatically by OmniServio when a service is registered.
        /// </summary>
        /// <param name="serviceType">The type of service that was registered.</param>
        /// <param name="service">The service instance that was registered.</param>
        /// <param name="omniServio">The OmniServio instance where the service was registered.</param>
        internal static void Publish(Type serviceType, object service, OmniServio omniServio)
        {
            if (!_subscribers.TryGetValue(serviceType, out List<Action<ServiceRegisteredEventArgs>> callbacks))
            {
                return;
            }

            var eventArgs = new ServiceRegisteredEventArgs(serviceType, service, omniServio);

            // Create a copy of the list to avoid issues if callbacks modify the subscription list
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

        /// <summary>
        /// Clears all subscriptions. Useful for cleanup.
        /// </summary>
        public static void Clear()
        {
            _subscribers.Clear();
        }

        /// <summary>
        /// Gets the number of subscribers for a specific service type.
        /// </summary>
        public static int GetSubscriberCount<T>() where T : class
        {
            Type serviceType = typeof(T);
            return _subscribers.TryGetValue(serviceType, out List<Action<ServiceRegisteredEventArgs>> callbacks) 
                ? callbacks.Count 
                : 0;
        }
    }
}

