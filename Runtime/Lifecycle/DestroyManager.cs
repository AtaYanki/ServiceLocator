using System;
using System.Collections.Generic;

namespace AtaYanki.OmniServio
{
    public interface IDestroyable
    {
        void Destroy();
    }

    public class DestroyManager
    {
        private readonly List<IDestroyable> _destroyableObjects = new();

        public void RegisterDestroyable(Type type)
        {
            if (type is not IDestroyable destroyable)
            {
                throw new ArgumentException($"Invalid argument in DestroyManager.RegisterDestroyable: Type '{type.FullName}' does not implement the required interface 'IDestroyable'. Please ensure that the provided type is compatible with the IDestroyable interface.", nameof(type));
            }

            if (!_destroyableObjects.Contains(destroyable))
                _destroyableObjects.Add(destroyable);
        }

        public void RegisterDestroyable<T>(T service)
        {
            Type type = typeof(T);

            if (service is not IDestroyable destroyable)
            {
                throw new ArgumentException($"Invalid argument in DestroyManager.RegisterDestroyable: Type '{type.FullName}' does not implement the required interface 'IDestroyable'. Please ensure that the provided type is compatible with the IDestroyable interface.", nameof(type));
            }

            if (!_destroyableObjects.Contains(destroyable))
                _destroyableObjects.Add(destroyable);
        }

        public void DestroyAll()
        {
            foreach (var destroyable in _destroyableObjects)
                destroyable.Destroy();
        }
    }
}