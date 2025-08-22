#nullable enable

using System;
using System.Linq;
using System.Reflection;

namespace NeuroSdk.Internal
{
    internal static class ClassInjectorWrapper
    {
        private static readonly Lazy<Type?> _class = new(() => Type.GetType("Il2CppInterop.Runtime.Injection.ClassInjector, Il2CppInterop.Runtime"));
        private static readonly Lazy<MethodInfo?> _isTypeRegisteredInIl2Cpp = new(() => _class.Value?.GetMethod("IsTypeRegisteredInIl2Cpp", new[] { typeof(Type) }));
        private static readonly Lazy<MethodInfo?> _registerTypeInIl2Cpp = new(() => _class.Value?.GetMethods().First(m => m.Name == "RegisterTypeInIl2Cpp" && m.GetParameters().Length == 2));

        public static bool IsTypeRegisteredInIl2Cpp(Type type)
        {
            MethodInfo? isTypeRegisteredInIl2Cpp = _isTypeRegisteredInIl2Cpp.Value;
            if (isTypeRegisteredInIl2Cpp == null) return true;

            return (bool) isTypeRegisteredInIl2Cpp.Invoke(null, new object[] { type })!;
        }

        public static void RegisterTypeInIl2Cpp(Type type, Type[] interfaces)
        {
            MethodInfo? registerTypeInIl2Cpp = _registerTypeInIl2Cpp.Value;
            if (registerTypeInIl2Cpp == null) return;

            registerTypeInIl2Cpp.Invoke(null, new[] { type, new RegisterTypeOptionsWrapper { Interfaces = interfaces }.Value });
        }
    }
}
