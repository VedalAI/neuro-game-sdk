#nullable enable

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using NeuroSdk.Il2Cpp;
using NeuroSdk.Internal;

namespace NeuroSdk
{
    partial class NeuroSdkSetup
    {
#pragma warning disable CS0436 // Type conflicts with imported type
#pragma warning disable CA2255 // The 'ModuleInitializer' attribute is only intended to be used in application code or advanced source generator scenarios
        [ModuleInitializer]
#pragma warning restore CA2255 // The 'ModuleInitializer' attribute is only intended to be used in application code or advanced source generator scenarios
#pragma warning restore CS0436 // Type conflicts with imported type
        [Obsolete("This method is only for compiler use and should not be called directly.", true)]
        internal static void ModuleInitializer()
        {
            ResourceManager.InjectAssemblies();
            ClassInjectorPatch.Patch();
            RegisterInIl2CppAttribute.Register(Assembly.GetExecutingAssembly());
        }
    }
}
