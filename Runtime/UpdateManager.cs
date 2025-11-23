using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtaYanki.ServiceLocator
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

        public void RegisterUpdatable(Type type)
        {
            if (type is not IUpdatable)
            {
                throw new ArgumentException($"Invalid argument in UpdateManager.RegisterUpdatable: Type '{type.FullName}' does not implement the required interface 'IUpdatable'. Please ensure that the provided type is compatible with the IUpdatable interface.", nameof(type));
            }

            IUpdatable updatable = type as IUpdatable;

            if (!_updatableObjects.Contains(updatable))
                _updatableObjects.Add(updatable);
        }

        public void RegisterUpdatable<T>(T service)
        {
            Type type = typeof(T);

            if (service is not IUpdatable)
            {
                throw new ArgumentException($"Invalid argument in UpdateManager.RegisterUpdatable: Type '{type.FullName}' does not implement the required interface 'IUpdatable'. Please ensure that the provided type is compatible with the IUpdatable interface.", nameof(type));
            }

            IUpdatable updatable = service as IUpdatable;

            if (!_updatableObjects.Contains(updatable))
                _updatableObjects.Add(updatable);
        }

        public void UnregisterUpdatable(IUpdatable updatable)
        {
            if (_updatableObjects.Contains(updatable))
                _updatableObjects.Remove(updatable);
        }

        public void RegisterFixedUpdatable(Type type)
        {
            if (type is not IFixedUpdatable)
            {
                throw new ArgumentException($"Invalid argument in UpdateManager.RegisterFixedUpdatable: Type '{type.FullName}' does not implement the required interface 'IFixedUpdatable'. Please ensure that the provided type is compatible with the IFixedUpdatable interface.", nameof(type));
            }

            IFixedUpdatable fixedUpdatable = type as IFixedUpdatable;

            if (!_fixedUpdatableObjects.Contains(fixedUpdatable))
                _fixedUpdatableObjects.Add(fixedUpdatable);
        }

        public void RegisterFixedUpdatable<T>(T service)
        {
            Type type = typeof(T);

            if (type is not IFixedUpdatable)
            {
                throw new ArgumentException($"Invalid argument in UpdateManager.RegisterFixedUpdatable: Type '{type.FullName}' does not implement the required interface 'IFixedUpdatable'. Please ensure that the provided type is compatible with the IFixedUpdatable interface.", nameof(type));
            }

            IFixedUpdatable fixedUpdatable = service as IFixedUpdatable;

            if (!_fixedUpdatableObjects.Contains(fixedUpdatable))
                _fixedUpdatableObjects.Add(fixedUpdatable);
        }

        public void UnregisterFixedUpdatable(IFixedUpdatable fixedUpdatable)
        {
            if (_fixedUpdatableObjects.Contains(fixedUpdatable))
                _fixedUpdatableObjects.Remove(fixedUpdatable);
        }

        public void RegisterLateUpdatable(Type type)
        {
            if (type is not ILateUpdatable)
            {
                throw new ArgumentException($"Invalid argument in UpdateManager.RegisterLateUpdatable: Type '{type.FullName}' does not implement the required interface 'ILateUpdatable'. Please ensure that the provided type is compatible with the ILateUpdatable interface.", nameof(type));
            }

            ILateUpdatable lateUpdatable = type as ILateUpdatable;

            if (!_lateUpdatableObjects.Contains(lateUpdatable))
                _lateUpdatableObjects.Add(lateUpdatable);
        }

        public void RegisterLateUpdatable<T>(T service)
        {
            Type type = typeof(T);

            if (type is not ILateUpdatable)
            {
                throw new ArgumentException($"Invalid argument in UpdateManager.RegisterLateUpdatable: Type '{type.FullName}' does not implement the required interface 'ILateUpdatable'. Please ensure that the provided type is compatible with the ILateUpdatable interface.", nameof(type));
            }

            ILateUpdatable lateUpdatable = service as ILateUpdatable;

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