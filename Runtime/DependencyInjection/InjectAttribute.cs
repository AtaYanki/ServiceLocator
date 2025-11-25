using System;

namespace AtaYanki.OmniServio
{
    /// <summary>
    /// Attribute to mark fields and properties for automatic dependency injection.
    /// Fields and properties marked with [Inject] will be automatically populated
    /// with services from the OmniServio container.
    /// 
    /// Example:
    /// [Inject] private IAudioService _audioService;
    /// [Inject] public ISaveService SaveService { get; private set; }
    /// [Inject(UseGlobal = true)] private IConfigService _configService; // Always use global
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class InjectAttribute : Attribute
    {
        /// <summary>
        /// Optional: Specify a specific service locator to use for injection.
        /// If null, uses the default resolution hierarchy (parent → scene → global).
        /// </summary>
        public bool UseGlobal { get; set; } = false;
    }
}

