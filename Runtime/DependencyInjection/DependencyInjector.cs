using System;
using UnityEngine;
using System.Reflection;

namespace AtaYanki.OmniServio
{
    public static class DependencyInjector
    {
        private static IInjectionExceptionHandler _exceptionHandler = new WarningExceptionHandler();

        public static void SetExceptionHandler(IInjectionExceptionHandler handler)
        {
            _exceptionHandler = handler ?? new WarningExceptionHandler();
        }

        public static IInjectionExceptionHandler GetExceptionHandler()
        {
            return _exceptionHandler;
        }

        public static void Inject(MonoBehaviour component, OmniServio omniServio = null)
        {
            if (component == null)
            {
                _exceptionHandler.HandleNullComponentError();
                return;
            }

            OmniServio locator = omniServio ?? OmniServio.For(component);
            if (locator == null)
            {
                _exceptionHandler.HandleOmniServioNotFoundError(component);
                return;
            }

            Type componentType = component.GetType();
            
            InjectFields(component, componentType, locator);
            
            InjectProperties(component, componentType, locator);
        }

        private static void InjectFields(MonoBehaviour component, Type componentType, OmniServio locator)
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

                    object currentValue = field.GetValue(component);
                    if (currentValue != null) continue;

                    OmniServio injectionLocator = injectAttr.UseGlobal ? OmniServio.Global : locator;

                    Type serviceType = field.FieldType;

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

        private static void InjectProperties(MonoBehaviour component, Type componentType, OmniServio locator)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            
            Type currentType = componentType;
            while (currentType != null && currentType != typeof(MonoBehaviour) && currentType != typeof(UnityEngine.Object))
            {
                PropertyInfo[] properties = currentType.GetProperties(flags);
                
                foreach (PropertyInfo property in properties)
                {
                    InjectAttribute injectAttr = property.GetCustomAttribute<InjectAttribute>();
                    if (injectAttr == null) continue;

                    if (!property.CanWrite)
                    {
                        _exceptionHandler.HandleNoSetterError(component, property);
                        continue;
                    }

                    object currentValue = property.GetValue(component);
                    if (currentValue != null) continue;

                    OmniServio injectionLocator = injectAttr.UseGlobal ? OmniServio.Global : locator;

                    Type serviceType = property.PropertyType;

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

        public static bool TryGetService(OmniServio locator, Type serviceType, out object service)
        {
            service = null;

            if (locator == null) return false;

            try
            {
                MethodInfo tryGetMethod = typeof(OmniServio).GetMethod("TryGet", BindingFlags.Public | BindingFlags.Instance);
                
                if (tryGetMethod == null)
                {
                    Debug.LogError("[DependencyInjector] Could not find TryGet method on OmniServio");
                    return false;
                }

                MethodInfo genericMethod = tryGetMethod.MakeGenericMethod(serviceType);
                
                object[] parameters = new object[] { null };
                
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

