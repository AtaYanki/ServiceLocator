using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtaYanki.OmniServio
{
    public interface IUpdatable
    {
        void Update(float deltaTime);
    }
    
    public interface IFixedUpdatable
    {
        void FixedUpdate(float deltaTime);
    }

    public interface ILateUpdatable
    {
        void LateUpdate(float deltaTime);
    }

    public class UpdateManager
    {
        private readonly List<IUpdatable> _updatableObjects = new List<IUpdatable>();
        private readonly List<IFixedUpdatable> _fixedUpdatableObjects = new List<IFixedUpdatable>();
        private readonly List<ILateUpdatable> _lateUpdatableObjects = new List<ILateUpdatable>();

        public void RegisterUpdatable(Type type, object service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service), "Service instance cannot be null.");
            }

            if (!typeof(IUpdatable).IsAssignableFrom(type))
            {
                throw new ArgumentException($"Invalid argument in UpdateManager.RegisterUpdatable: Type '{type.FullName}' does not implement the required interface 'IUpdatable'. Please ensure that the provided type is compatible with the IUpdatable interface.", nameof(type));
            }

            if (service is not IUpdatable updatable)
            {
                throw new ArgumentException($"Service instance of type '{service.GetType().FullName}' does not implement IUpdatable.", nameof(service));
            }

            if (!_updatableObjects.Contains(updatable))
                _updatableObjects.Add(updatable);
        }

        public void RegisterUpdatable<T>(T service)
        {
            if (service is not IUpdatable updatable)
            {
                Type type = typeof(T);
                throw new ArgumentException($"Invalid argument in UpdateManager.RegisterUpdatable: Type '{type.FullName}' does not implement the required interface 'IUpdatable'. Please ensure that the provided type is compatible with the IUpdatable interface.", nameof(service));
            }

            if (!_updatableObjects.Contains(updatable))
                _updatableObjects.Add(updatable);
        }

        public void UnregisterUpdatable(IUpdatable updatable)
        {
            if (_updatableObjects.Contains(updatable))
                _updatableObjects.Remove(updatable);
        }

        public void RegisterFixedUpdatable(Type type, object service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service), "Service instance cannot be null.");
            }

            if (!typeof(IFixedUpdatable).IsAssignableFrom(type))
            {
                throw new ArgumentException($"Invalid argument in UpdateManager.RegisterFixedUpdatable: Type '{type.FullName}' does not implement the required interface 'IFixedUpdatable'. Please ensure that the provided type is compatible with the IFixedUpdatable interface.", nameof(type));
            }

            if (service is not IFixedUpdatable fixedUpdatable)
            {
                throw new ArgumentException($"Service instance of type '{service.GetType().FullName}' does not implement IFixedUpdatable.", nameof(service));
            }

            if (!_fixedUpdatableObjects.Contains(fixedUpdatable))
                _fixedUpdatableObjects.Add(fixedUpdatable);
        }

        public void RegisterFixedUpdatable<T>(T service)
        {
            if (service is not IFixedUpdatable fixedUpdatable)
            {
                Type type = typeof(T);
                throw new ArgumentException($"Invalid argument in UpdateManager.RegisterFixedUpdatable: Type '{type.FullName}' does not implement the required interface 'IFixedUpdatable'. Please ensure that the provided type is compatible with the IFixedUpdatable interface.", nameof(service));
            }

            if (!_fixedUpdatableObjects.Contains(fixedUpdatable))
                _fixedUpdatableObjects.Add(fixedUpdatable);
        }

        public void UnregisterFixedUpdatable(IFixedUpdatable fixedUpdatable)
        {
            if (_fixedUpdatableObjects.Contains(fixedUpdatable))
                _fixedUpdatableObjects.Remove(fixedUpdatable);
        }

        public void RegisterLateUpdatable(Type type, object service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service), "Service instance cannot be null.");
            }

            if (!typeof(ILateUpdatable).IsAssignableFrom(type))
            {
                throw new ArgumentException($"Invalid argument in UpdateManager.RegisterLateUpdatable: Type '{type.FullName}' does not implement the required interface 'ILateUpdatable'. Please ensure that the provided type is compatible with the ILateUpdatable interface.", nameof(type));
            }

            if (service is not ILateUpdatable lateUpdatable)
            {
                throw new ArgumentException($"Service instance of type '{service.GetType().FullName}' does not implement ILateUpdatable.", nameof(service));
            }

            if (!_lateUpdatableObjects.Contains(lateUpdatable))
                _lateUpdatableObjects.Add(lateUpdatable);
        }

        public void RegisterLateUpdatable<T>(T service)
        {
            if (service is not ILateUpdatable lateUpdatable)
            {
                Type type = typeof(T);
                throw new ArgumentException($"Invalid argument in UpdateManager.RegisterLateUpdatable: Type '{type.FullName}' does not implement the required interface 'ILateUpdatable'. Please ensure that the provided type is compatible with the ILateUpdatable interface.", nameof(service));
            }

            if (!_lateUpdatableObjects.Contains(lateUpdatable))
                _lateUpdatableObjects.Add(lateUpdatable);
        }

        public void UnregisterLateUpdatable(ILateUpdatable lateUpdatable)
        {
            if (_lateUpdatableObjects.Contains(lateUpdatable))
                _lateUpdatableObjects.Remove(lateUpdatable);
        }

        public void UpdateAll()
        {
            foreach (var updatable in _updatableObjects)
                updatable.Update(Time.deltaTime);
        }

        public void FixedUpdateAll()
        {
            foreach (var fixedUpdatable in _fixedUpdatableObjects)
                fixedUpdatable.FixedUpdate(Time.fixedDeltaTime);
        }

        public void LateUpdateAll()
        {
            foreach (var lateUpdatable in _lateUpdatableObjects)
                lateUpdatable.LateUpdate(Time.deltaTime);
        }
    }
}