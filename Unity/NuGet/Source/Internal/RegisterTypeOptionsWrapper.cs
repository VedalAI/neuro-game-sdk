#nullable enable

using System;
using System.Reflection;

namespace NeuroSdk.Internal
{
    internal sealed class RegisterTypeOptionsWrapper
    {
        private static readonly Lazy<Type?> _class = new(() => Type.GetType("Il2CppInterop.Runtime.Injection.RegisterTypeOptions, Il2CppInterop.Runtime"));
        private static readonly Lazy<PropertyInfo?> _interfaces = new(() => _class.Value?.GetProperty("Interfaces"));

        public object? Value { get; }

        public Type[] Interfaces
        {
            set
            {
                PropertyInfo? interfaces = _interfaces.Value;
                if (interfaces == null) return;

                interfaces.SetValue(Value, new Il2CppInterfaceCollectionWrapper(value).Value);
            }
        }

        public RegisterTypeOptionsWrapper()
        {
            Type? classType = _class.Value;
            if (classType == null) return;

            Value = Activator.CreateInstance(classType);
        }
    }
}
