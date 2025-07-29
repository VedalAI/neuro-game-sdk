#nullable enable

using System;

namespace NeuroSdk.Internal
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property |
                    AttributeTargets.Event)]
    internal sealed class Il2CppHideAttribute : Attribute
    {
        // This attribute takes the place of HideFromIl2CppAttribute from https://github.com/BepInEx/Il2CppInterop/blob/master/Il2CppInterop.Runtime/Attributes/HideFromIl2CppAttribute.cs
    }
}
