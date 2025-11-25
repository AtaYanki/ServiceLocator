using System;
using UnityEngine;
using System.Collections.Generic;

namespace AtaYanki.OmniServio
{
    public class ServiceManager
    {
        public IEnumerable<object> RegisteredServices => _services.Values;

        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public T Get<T>() where T : class
        {
            Type type = typeof(T);

            if (_services.TryGetValue(type, out object service))
            {
                return service as T;
            }

            throw new ArgumentException($"[Service Manager].Get - Service of type {type.FullName} not registered.");
        }

        public bool TryGet<T>(out T service) where T : class
        {
            Type type = typeof(T);

            if (_services.TryGetValue(type, out object obj))
            {
                service = obj as T;
                return true;
            }

            service = null;
            return false;
        }

        public ServiceManager Register<T>(T service)
        {
            Type type = typeof(T);

            if (!_services.TryAdd(type, service))
            {
                Debug.LogError($"[Service Manager].Register - Service of type {type.FullName} already registered");
            }

            return this;
        }

        public ServiceManager Register(Type type, object service)
        {
            if (!type.IsInstanceOfType(service))
            {
                throw new ArgumentException("Service Manager].Register - Type of service does not match type of service interface", nameof(service));
            }

            if (!_services.TryAdd(type, service))
            {
                Debug.LogError($"[Service Manager].Register - Service of type {type.FullName} already registered");
            }

            return this;
        }
    }
}