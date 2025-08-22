#nullable enable

using System;

namespace NeuroSdk.Il2Cpp
{
    /// <summary>
    /// Automatically registers this class as an il2cpp type at runtime.
    ///
    /// This is only used for MODDED IL2CPP.
    /// </summary>
    #if UNITY_EDITOR
    [Obsolete("This attribute should only be used in modded Il2Cpp.")]
    #endif
    [AttributeUsage(AttributeTargets.Class)]
    public sealed partial class RegisterInIl2CppAttribute : Attribute
    {
        /// <summary>
        /// Gets il2cpp interfaces to be injected with this type.
        /// </summary>
        private Type[] Interfaces { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterInIl2CppAttribute"/> class without any interfaces.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public RegisterInIl2CppAttribute()
        {
            Interfaces = Type.EmptyTypes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterInIl2CppAttribute"/> class with interfaces.
        /// </summary>
        /// <param name="interfaces">Il2Cpp interfaces to be injected with this type.</param>
        // ReSharper disable once UnusedMember.Global
        public RegisterInIl2CppAttribute(params Type[] interfaces)
        {
            Interfaces = interfaces;
        }
    }
}
