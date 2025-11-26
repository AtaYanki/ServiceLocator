using System;

namespace AtaYanki.OmniServio
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class InjectAttribute : Attribute
    {
        public bool UseGlobal { get; set; } = false;
    }
}

