using System;
using UnityEngine;
using System.Reflection;

namespace AtaYanki.OmniServio
{
    /// <summary>
    /// Interface for handling injection errors using the Adapter pattern.
    /// Allows different error handling strategies (throw exception vs warning).
    /// </summary>
    public interface IInjectionExceptionHandler
    {
        /// <summary>
        /// Handles an injection error when a service cannot be found.
        /// </summary>
        /// <param name="component">The MonoBehaviour component where injection failed.</param>
        /// <param name="memberInfo">The field or property that failed to inject.</param>
        /// <param name="serviceType">The type of service that was not found.</param>
        /// <param name="injectionLocator">The OmniServio instance used for injection.</param>
        void HandleInjectionError(MonoBehaviour component, MemberInfo memberInfo, Type serviceType, OmniServio injectionLocator);

        /// <summary>
        /// Handles an injection error when a property has no setter.
        /// </summary>
        /// <param name="component">The MonoBehaviour component where injection failed.</param>
        /// <param name="propertyInfo">The property that has no setter.</param>
        void HandleNoSetterError(MonoBehaviour component, PropertyInfo propertyInfo);

        /// <summary>
        /// Handles an error when OmniServio cannot be found for a component.
        /// </summary>
        /// <param name="component">The MonoBehaviour component that needs injection.</param>
        void HandleOmniServioNotFoundError(MonoBehaviour component);

        /// <summary>
        /// Handles an error when the component is null.
        /// </summary>
        void HandleNullComponentError();
    }
}

