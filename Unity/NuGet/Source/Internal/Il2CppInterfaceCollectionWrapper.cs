#nullable enable

using System;
using System.Collections.Generic;

namespace NeuroSdk.Internal
{
    internal sealed class Il2CppInterfaceCollectionWrapper
    {
        private static readonly Lazy<Type?> _class = new(() => Type.GetType("Il2CppInterop.Runtime.Injection.Il2CppInterfaceCollection, Il2CppInterop.Runtime"));

        public object? Value { get; }

        public Il2CppInterfaceCollectionWrapper(IEnumerable<Type> interfaces)
        {
            Type? classType = _class.Value;
            if (classType == null) return;

            Value = Activator.CreateInstance(classType, interfaces);
        }
    }
}
