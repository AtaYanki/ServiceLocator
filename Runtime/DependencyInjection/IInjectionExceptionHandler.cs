using System;
using UnityEngine;
using System.Reflection;

namespace AtaYanki.OmniServio
{
    public interface IInjectionExceptionHandler
    {
        void HandleInjectionError(MonoBehaviour component, MemberInfo memberInfo, Type serviceType, OmniServio injectionLocator);

        void HandleNoSetterError(MonoBehaviour component, PropertyInfo propertyInfo);

        void HandleOmniServioNotFoundError(MonoBehaviour component);

        void HandleNullComponentError();
    }
}

