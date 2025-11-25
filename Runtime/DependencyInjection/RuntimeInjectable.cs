using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AtaYanki.OmniServio
{
    /// <summary>
    /// Abstract base class for MonoBehaviour components that need runtime dependency injection.
    /// Automatically subscribes to service registration events and re-injects dependencies when services become available.
    /// </summary>
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

        /// <summary>
        /// Initializes runtime injection by subscribing to services that are needed but not yet available.
        /// </summary>
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

            // Find all fields and properties marked with [Inject] that are not yet injected
            Type componentType = GetType();
            FindAndSubscribeToMissingServices(componentType);
        }

        /// <summary>
        /// Finds all [Inject] marked members and subscribes to services that are not yet available.
        /// </summary>
        private void FindAndSubscribeToMissingServices(Type componentType)
        {
            // Check fields
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            
            Type currentType = componentType;
            while (currentType != null && currentType != typeof(MonoBehaviour) && currentType != typeof(UnityEngine.Object))
            {
                // Check fields
                FieldInfo[] fields = currentType.GetFields(flags);
                foreach (FieldInfo field in fields)
                {
                    InjectAttribute injectAttr = field.GetCustomAttribute<InjectAttribute>();
                    if (injectAttr == null) continue;

                    // Skip if already injected
                    object currentValue = field.GetValue(this);
                    if (currentValue != null) continue;

                    Type serviceType = field.FieldType;
                    SubscribeToService(serviceType, injectAttr.UseGlobal, field);
                }

                // Check properties
                PropertyInfo[] properties = currentType.GetProperties(flags);
                foreach (PropertyInfo property in properties)
                {
                    InjectAttribute injectAttr = property.GetCustomAttribute<InjectAttribute>();
                    if (injectAttr == null) continue;

                    if (!property.CanWrite) continue;

                    // Skip if already injected
                    object currentValue = property.GetValue(this);
                    if (currentValue != null) continue;

                    Type serviceType = property.PropertyType;
                    SubscribeToService(serviceType, injectAttr.UseGlobal, property);
                }

                currentType = currentType.BaseType;
            }
        }

        /// <summary>
        /// Subscribes to a service type if it's not yet available.
        /// </summary>
        private void SubscribeToService(Type serviceType, bool useGlobal, MemberInfo memberInfo)
        {
            // Check if service is already available
            OmniServio locator = useGlobal ? OmniServio.Global : _omniServio;
            if (locator != null && DependencyInjector.TryGetService(locator, serviceType, out _))
            {
                // Service is already available, try to inject it now
                TryInjectMember(memberInfo, serviceType, useGlobal);
                return;
            }

            // Service not available, subscribe to event
            if (_subscriptions.ContainsKey(serviceType))
            {
                return; // Already subscribed
            }

            Action<ServiceRegisteredEventArgs> callback = (args) =>
            {
                // Check if this is the right OmniServio instance
                OmniServio targetLocator = useGlobal ? OmniServio.Global : _omniServio;
                if (args.OmniServio != targetLocator)
                {
                    return; // Not the right locator
                }

                // Try to inject the service
                TryInjectMember(memberInfo, serviceType, useGlobal);
                
                // Unsubscribe after successful injection
                UnsubscribeFromService(serviceType);
            };

            // Subscribe using reflection to call the generic Subscribe method
            MethodInfo subscribeMethod = typeof(ServiceRegistrationEventBus).GetMethod("Subscribe", 
                BindingFlags.Public | BindingFlags.Static);
            
            if (subscribeMethod != null)
            {
                MethodInfo genericSubscribe = subscribeMethod.MakeGenericMethod(serviceType);
                genericSubscribe.Invoke(null, new object[] { callback });
                
                _subscriptions[serviceType] = callback;
            }
        }

        /// <summary>
        /// Attempts to inject a member with a service.
        /// </summary>
        private void TryInjectMember(MemberInfo memberInfo, Type serviceType, bool useGlobal)
        {
            OmniServio locator = useGlobal ? OmniServio.Global : _omniServio;
            if (locator == null) return;

            if (!DependencyInjector.TryGetService(locator, serviceType, out object service))
            {
                return;
            }

            // Inject the service
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

        /// <summary>
        /// Unsubscribes from a specific service type.
        /// </summary>
        private void UnsubscribeFromService(Type serviceType)
        {
            if (!_subscriptions.TryGetValue(serviceType, out Action<ServiceRegisteredEventArgs> callback))
            {
                return;
            }

            // Unsubscribe using reflection
            MethodInfo unsubscribeMethod = typeof(ServiceRegistrationEventBus).GetMethod("Unsubscribe", 
                BindingFlags.Public | BindingFlags.Static);
            
            if (unsubscribeMethod != null)
            {
                MethodInfo genericUnsubscribe = unsubscribeMethod.MakeGenericMethod(serviceType);
                genericUnsubscribe.Invoke(null, new object[] { callback });
            }

            _subscriptions.Remove(serviceType);
        }

        /// <summary>
        /// Unsubscribes from all services.
        /// </summary>
        private void UnsubscribeAll()
        {
            var serviceTypes = new List<Type>(_subscriptions.Keys);
            foreach (Type serviceType in serviceTypes)
            {
                UnsubscribeFromService(serviceType);
            }
            _subscriptions.Clear();
        }

        /// <summary>
        /// Called when a service is successfully injected at runtime.
        /// Override this method to perform additional setup when a service becomes available.
        /// </summary>
        /// <param name="memberInfo">The field or property that was injected.</param>
        /// <param name="serviceType">The type of service that was injected.</param>
        /// <param name="service">The service instance that was injected.</param>
        protected virtual void OnServiceInjected(MemberInfo memberInfo, Type serviceType, object service)
        {
            Debug.Log($"[RuntimeInjectable] Runtime injected {serviceType.Name} into {GetType().Name}.{memberInfo.Name}", this);
        }

        /// <summary>
        /// Manually triggers runtime injection check.
        /// Useful if you want to check for newly available services without waiting for events.
        /// </summary>
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

