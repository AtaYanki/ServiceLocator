using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AtaYanki.OmniServio
{
    public abstract class RuntimeInjectable : MonoBehaviour
    {
        private readonly Dictionary<Type, Action<ServiceRegisteredEventArgs>> _subscriptions = 
            new Dictionary<Type, Action<ServiceRegisteredEventArgs>>();
        
        private OmniServio _omniServio;
        private bool _isInitialized = false;

        protected virtual void Awake()
        {
            InitializeRuntimeInjection();
        }

        protected virtual void OnDestroy()
        {
            UnsubscribeAll();
        }

        private void InitializeRuntimeInjection()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            _omniServio = OmniServio.For(this);
            if (_omniServio == null)
            {
                Debug.LogWarning($"[RuntimeInjectable] Could not find OmniServio for {GetType().Name}. Runtime injection may not work.", this);
                return;
            }

            Type componentType = GetType();
            FindAndSubscribeToMissingServices(componentType);
        }

        private void FindAndSubscribeToMissingServices(Type componentType)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            
            Type currentType = componentType;
            while (currentType != null && currentType != typeof(MonoBehaviour) && currentType != typeof(UnityEngine.Object))
            {
                FieldInfo[] fields = currentType.GetFields(flags);
                foreach (FieldInfo field in fields)
                {
                    InjectAttribute injectAttr = field.GetCustomAttribute<InjectAttribute>();
                    if (injectAttr == null) continue;

                    object currentValue = field.GetValue(this);
                    if (currentValue != null) continue;

                    Type serviceType = field.FieldType;
                    SubscribeToService(serviceType, injectAttr.UseGlobal, field);
                }

                PropertyInfo[] properties = currentType.GetProperties(flags);
                foreach (PropertyInfo property in properties)
                {
                    InjectAttribute injectAttr = property.GetCustomAttribute<InjectAttribute>();
                    if (injectAttr == null) continue;

                    if (!property.CanWrite) continue;

                    object currentValue = property.GetValue(this);
                    if (currentValue != null) continue;

                    Type serviceType = property.PropertyType;
                    SubscribeToService(serviceType, injectAttr.UseGlobal, property);
                }

                currentType = currentType.BaseType;
            }
        }

        private void SubscribeToService(Type serviceType, bool useGlobal, MemberInfo memberInfo)
        {
            OmniServio locator = useGlobal ? OmniServio.Global : _omniServio;
            if (locator != null && DependencyInjector.TryGetService(locator, serviceType, out _))
            {
                TryInjectMember(memberInfo, serviceType, useGlobal);
                return;
            }
    
            if (_subscriptions.ContainsKey(serviceType))
            {
                return;
            }

            Action<ServiceRegisteredEventArgs> callback = (args) =>
            {
                OmniServio targetLocator = useGlobal ? OmniServio.Global : _omniServio;
                if (args.OmniServio != targetLocator)
                {
                    return;
                }

                TryInjectMember(memberInfo, serviceType, useGlobal);
                
                UnsubscribeFromService(serviceType);
            };

            MethodInfo subscribeMethod = typeof(ServiceRegistrationEventBus).GetMethod("Subscribe", 
                BindingFlags.Public | BindingFlags.Static);
            
            if (subscribeMethod != null)
            {
                MethodInfo genericSubscribe = subscribeMethod.MakeGenericMethod(serviceType);
                genericSubscribe.Invoke(null, new object[] { callback });
                
                _subscriptions[serviceType] = callback;
            }
        }

        private void TryInjectMember(MemberInfo memberInfo, Type serviceType, bool useGlobal)
        {
            OmniServio locator = useGlobal ? OmniServio.Global : _omniServio;
            if (locator == null) return;

            if (!DependencyInjector.TryGetService(locator, serviceType, out object service))
            {
                return;
            }

            if (memberInfo is FieldInfo field)
            {
                object currentValue = field.GetValue(this);
                if (currentValue == null)
                {
                    field.SetValue(this, service);
                    OnServiceInjected(field, serviceType, service);
                }
            }
            else if (memberInfo is PropertyInfo property && property.CanWrite)
            {
                object currentValue = property.GetValue(this);
                if (currentValue == null)
                {
                    property.SetValue(this, service);
                    OnServiceInjected(property, serviceType, service);
                }
            }
        }

        private void UnsubscribeFromService(Type serviceType)
        {
            if (!_subscriptions.TryGetValue(serviceType, out Action<ServiceRegisteredEventArgs> callback))
            {
                return;
            }

            MethodInfo unsubscribeMethod = typeof(ServiceRegistrationEventBus).GetMethod("Unsubscribe", 
                BindingFlags.Public | BindingFlags.Static);
            
            if (unsubscribeMethod != null)
            {
                MethodInfo genericUnsubscribe = unsubscribeMethod.MakeGenericMethod(serviceType);
                genericUnsubscribe.Invoke(null, new object[] { callback });
            }

            _subscriptions.Remove(serviceType);
        }

        private void UnsubscribeAll()
        {
            var serviceTypes = new List<Type>(_subscriptions.Keys);
            foreach (Type serviceType in serviceTypes)
            {
                UnsubscribeFromService(serviceType);
            }
            _subscriptions.Clear();
        }

        protected virtual void OnServiceInjected(MemberInfo memberInfo, Type serviceType, object service)
        {
            Debug.Log($"[RuntimeInjectable] Runtime injected {serviceType.Name} into {GetType().Name}.{memberInfo.Name}", this);
        }

        public void CheckForRuntimeInjection()
        {
            if (!_isInitialized)
            {
                InitializeRuntimeInjection();
                return;
            }

            Type componentType = GetType();
            FindAndSubscribeToMissingServices(componentType);
        }
    }
}

