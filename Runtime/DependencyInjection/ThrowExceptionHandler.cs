using System;
using UnityEngine;
using System.Reflection;

namespace AtaYanki.OmniServio
{
    /// <summary>
    /// Exception handler that throws exceptions when injection errors occur.
    /// Use this mode for strict error handling during development.
    /// </summary>
    public class ThrowExceptionHandler : IInjectionExceptionHandler
    {
        public void HandleInjectionError(MonoBehaviour component, MemberInfo memberInfo, Type serviceType, OmniServio injectionLocator)
        {
            string memberName = memberInfo.Name;
            string componentName = component != null ? component.GetType().Name : "Unknown";
            string locatorName = injectionLocator != null ? injectionLocator.name : "Unknown";
            
            throw new InvalidOperationException(
                $"[DependencyInjector] Failed to inject service of type {serviceType.FullName} into {componentName}.{memberName}. " +
                $"Service not registered in OmniServio '{locatorName}'. " +
                $"Make sure the service is registered before injection occurs.");
        }

        public void HandleNoSetterError(MonoBehaviour component, PropertyInfo propertyInfo)
        {
            string componentName = component != null ? component.GetType().Name : "Unknown";
            
            throw new InvalidOperationException(
                $"[DependencyInjector] Property {componentName}.{propertyInfo.Name} has [Inject] attribute but no setter. " +
                $"Add a setter to enable dependency injection.");
        }

        public void HandleOmniServioNotFoundError(MonoBehaviour component)
        {
            string componentName = component != null ? component.GetType().Name : "Unknown";
            
            throw new InvalidOperationException(
                $"[DependencyInjector] Could not find OmniServio for {componentName}. " +
                $"Dependencies cannot be injected. Ensure an OmniServio instance exists in the scene hierarchy.");
        }

        public void HandleNullComponentError()
        {
            throw new ArgumentNullException(nameof(MonoBehaviour),
                "[DependencyInjector] Component is null, cannot inject dependencies.");
        }
    }
}

