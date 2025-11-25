using System;
using UnityEngine;
using System.Reflection;

namespace AtaYanki.OmniServio
{
    /// <summary>
    /// Handles automatic dependency injection using reflection.
    /// Injects services marked with [Inject] attribute into MonoBehaviour components.
    /// </summary>
    public static class DependencyInjector
    {
        private static IInjectionExceptionHandler _exceptionHandler = new WarningExceptionHandler();

        /// <summary>
        /// Sets the exception handler to use for injection errors.
        /// Default is WarningExceptionHandler.
        /// </summary>
        /// <param name="handler">The exception handler to use. If null, uses WarningExceptionHandler.</param>
        public static void SetExceptionHandler(IInjectionExceptionHandler handler)
        {
            _exceptionHandler = handler ?? new WarningExceptionHandler();
        }

        /// <summary>
        /// Gets the current exception handler.
        /// </summary>
        public static IInjectionExceptionHandler GetExceptionHandler()
        {
            return _exceptionHandler;
        }

        /// <summary>
        /// Injects dependencies into a MonoBehaviour component.
        /// Looks for fields and properties marked with [Inject] attribute and populates them.
        /// </summary>
        /// <param name="component">The MonoBehaviour component to inject dependencies into.</param>
        /// <param name="omniServio">Optional OmniServio instance. If null, uses OmniServio.For(component).</param>
        public static void Inject(MonoBehaviour component, OmniServio omniServio = null)
        {
            if (component == null)
            {
                _exceptionHandler.HandleNullComponentError();
                return;
            }

            // Get the OmniServio to use for injection
            OmniServio locator = omniServio ?? OmniServio.For(component);
            if (locator == null)
            {
                _exceptionHandler.HandleOmniServioNotFoundError(component);
                return;
            }

            Type componentType = component.GetType();
            
            // Inject fields
            InjectFields(component, componentType, locator);
            
            // Inject properties
            InjectProperties(component, componentType, locator);
        }

        /// <summary>
        /// Injects dependencies into fields marked with [Inject] attribute.
        /// </summary>
        private static void InjectFields(MonoBehaviour component, Type componentType, OmniServio locator)
        {
            // Get all fields including private ones from base classes
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            
            Type currentType = componentType;
            while (currentType != null && currentType != typeof(MonoBehaviour) && currentType != typeof(UnityEngine.Object))
            {
                FieldInfo[] fields = currentType.GetFields(flags);
                
                foreach (FieldInfo field in fields)
                {
                    InjectAttribute injectAttr = field.GetCustomAttribute<InjectAttribute>();
                    if (injectAttr == null) continue;

                    // Skip if field is already set (unless it's null)
                    object currentValue = field.GetValue(component);
                    if (currentValue != null) continue;

                    // Determine which OmniServio to use
                    OmniServio injectionLocator = injectAttr.UseGlobal ? OmniServio.Global : locator;

                    // Get the service type
                    Type serviceType = field.FieldType;

                    // Try to get the service
                    if (TryGetService(injectionLocator, serviceType, out object service))
                    {
                        field.SetValue(component, service);
                        Debug.Log($"[DependencyInjector] Injected {serviceType.Name} into {componentType.Name}.{field.Name}", component);
                    }
                    else
                    {
                        _exceptionHandler.HandleInjectionError(component, field, serviceType, injectionLocator);
                    }
                }

                currentType = currentType.BaseType;
            }
        }

        /// <summary>
        /// Injects dependencies into properties marked with [Inject] attribute.
        /// </summary>
        private static void InjectProperties(MonoBehaviour component, Type componentType, OmniServio locator)
        {
            // Get all properties including private ones from base classes
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            
            Type currentType = componentType;
            while (currentType != null && currentType != typeof(MonoBehaviour) && currentType != typeof(UnityEngine.Object))
            {
                PropertyInfo[] properties = currentType.GetProperties(flags);
                
                foreach (PropertyInfo property in properties)
                {
                    InjectAttribute injectAttr = property.GetCustomAttribute<InjectAttribute>();
                    if (injectAttr == null) continue;

                    // Skip if property doesn't have a setter
                    if (!property.CanWrite)
                    {
                        _exceptionHandler.HandleNoSetterError(component, property);
                        continue;
                    }

                    // Skip if property is already set (unless it's null)
                    object currentValue = property.GetValue(component);
                    if (currentValue != null) continue;

                    // Determine which OmniServio to use
                    OmniServio injectionLocator = injectAttr.UseGlobal ? OmniServio.Global : locator;

                    // Get the service type
                    Type serviceType = property.PropertyType;

                    // Try to get the service
                    if (TryGetService(injectionLocator, serviceType, out object service))
                    {
                        property.SetValue(component, service);
                        Debug.Log($"[DependencyInjector] Injected {serviceType.Name} into {componentType.Name}.{property.Name}", component);
                    }
                    else
                    {
                        _exceptionHandler.HandleInjectionError(component, property, serviceType, injectionLocator);
                    }
                }

                currentType = currentType.BaseType;
            }
        }

        /// <summary>
        /// Tries to get a service from the OmniServio using reflection.
        /// Uses the TryGet method which doesn't throw exceptions.
        /// </summary>
        /// <param name="locator">The OmniServio instance to get the service from.</param>
        /// <param name="serviceType">The type of service to retrieve.</param>
        /// <param name="service">The service instance if found, null otherwise.</param>
        /// <returns>True if the service was found, false otherwise.</returns>
        public static bool TryGetService(OmniServio locator, Type serviceType, out object service)
        {
            service = null;

            if (locator == null) return false;

            try
            {
                // Use reflection to call the generic TryGet<T>(out T) method
                MethodInfo tryGetMethod = typeof(OmniServio).GetMethod("TryGet", BindingFlags.Public | BindingFlags.Instance);
                
                if (tryGetMethod == null)
                {
                    Debug.LogError("[DependencyInjector] Could not find TryGet method on OmniServio");
                    return false;
                }

                // Create a generic method for the service type
                MethodInfo genericMethod = tryGetMethod.MakeGenericMethod(serviceType);
                
                // Create parameters array with null for the out parameter
                object[] parameters = new object[] { null };
                
                // Invoke the method - returns bool indicating success
                bool success = (bool)genericMethod.Invoke(locator, parameters);
                
                if (success)
                {
                    service = parameters[0];
                    return service != null;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DependencyInjector] Error getting service {serviceType.Name}: {ex.Message}");
                return false;
            }
        }
    }
}

