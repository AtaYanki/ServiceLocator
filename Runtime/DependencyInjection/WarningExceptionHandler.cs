using System;
using UnityEngine;
using System.Reflection;

namespace AtaYanki.OmniServio
{
    /// <summary>
    /// Exception handler that logs warnings when injection errors occur.
    /// Use this mode for graceful error handling that doesn't stop execution.
    /// </summary>
    public class WarningExceptionHandler : IInjectionExceptionHandler
    {
        public void HandleInjectionError(MonoBehaviour component, MemberInfo memberInfo, Type serviceType, OmniServio injectionLocator)
        {
            string memberName = memberInfo.Name;
            string componentName = component != null ? component.GetType().Name : "Unknown";
            string locatorName = injectionLocator != null ? injectionLocator.name : "Unknown";
            
            Debug.LogWarning(
                $"[DependencyInjector] Could not inject {serviceType.Name} into {componentName}.{memberName}. " +
                $"Service not registered in OmniServio '{locatorName}'.", 
                component);
        }

        public void HandleNoSetterError(MonoBehaviour component, PropertyInfo propertyInfo)
        {
            string componentName = component != null ? component.GetType().Name : "Unknown";
            
            Debug.LogWarning(
                $"[DependencyInjector] Property {componentName}.{propertyInfo.Name} has [Inject] but no setter. Skipping.", 
                component);
        }

        public void HandleOmniServioNotFoundError(MonoBehaviour component)
        {
            string componentName = component != null ? component.GetType().Name : "Unknown";
            
            Debug.LogWarning(
                $"[DependencyInjector] Could not find OmniServio for {componentName}. Dependencies will not be injected.", 
                component);
        }

        public void HandleNullComponentError()
        {
            Debug.LogError("[DependencyInjector] Component is null, cannot inject dependencies.");
        }
    }
}

