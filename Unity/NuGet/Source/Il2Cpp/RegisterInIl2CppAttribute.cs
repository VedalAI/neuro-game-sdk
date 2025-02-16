// Taken from https://github.com/NuclearPowered/Reactor/blob/master/Reactor/Utilities/Attributes/RegisterInIl2CppAttribute.cs

#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using NeuroSdk.Internal;
using UnityEngine;

namespace NeuroSdk.Il2Cpp
{
    partial class RegisterInIl2CppAttribute
    {
        private static readonly HashSet<Assembly> _registeredAssemblies = new();

        private static void RegisterType(Type type, Type[] interfaces)
        {
            RegisterInIl2CppAttribute? baseTypeAttribute = type.BaseType?.GetCustomAttribute<RegisterInIl2CppAttribute>();
            if (baseTypeAttribute != null)
            {
                RegisterType(type.BaseType!, baseTypeAttribute.Interfaces);
            }

            if (ClassInjectorWrapper.IsTypeRegisteredInIl2Cpp(type))
            {
                return;
            }

            try
            {
                ClassInjectorWrapper.RegisterTypeInIl2Cpp(type, interfaces);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to register {type.FullName}: {e}");
            }
        }

        /// <summary>
        /// Registers all Il2Cpp types annotated with <see cref="RegisterInIl2CppAttribute"/> in the specified <paramref name="assembly"/>.
        /// </summary>
        /// <remarks>This is called automatically on plugin assemblies so you probably don't need to call this.</remarks>
        /// <param name="assembly">The assembly to search.</param>
        public static void Register(Assembly assembly)
        {
            if (!_registeredAssemblies.Add(assembly)) return;

            foreach (Type type in assembly.GetTypes())
            {
                RegisterInIl2CppAttribute? attribute = type.GetCustomAttribute<RegisterInIl2CppAttribute>();
                if (attribute != null)
                {
                    RegisterType(type, attribute.Interfaces);
                }
            }
        }
    }
}
