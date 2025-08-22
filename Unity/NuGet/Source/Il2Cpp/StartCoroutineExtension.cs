#nullable enable

using System;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace UnityEngine
{
    // ReSharper disable once UnusedType.Global
    public static class StartCoroutineExtension
    {
        private static readonly Lazy<MethodInfo?> _wrapToIl2Cpp = new(() => Type.GetType("BepInEx.Unity.IL2CPP.Utils.Collections.CollectionExtensions, BepInEx.Unity.IL2CPP")?.GetMethods().FirstOrDefault(m => m.Name == "WrapToIl2Cpp" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(System.Collections.IEnumerator)));

        private static readonly Lazy<MethodInfo?> _startIl2CppCoroutine = new (() => typeof(MonoBehaviour).GetMethods().FirstOrDefault(m => m.Name == nameof(MonoBehaviour.StartCoroutine) && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType.FullName == "Il2CppSystem.Collections.IEnumerator"));

        // Class methods get prioritized over extension methods, so this extension method will only be called when the corresponding class method is missing.
        // ReSharper disable once UnusedMember.Global
        public static Coroutine StartCoroutine(this MonoBehaviour mainThreadUtil, System.Collections.IEnumerator enumerator)
        {
            MethodInfo? startCoroutineMethod = _startIl2CppCoroutine.Value;
            if (startCoroutineMethod == null)
            {
                throw new MissingMethodException("Could not find method 'UnityEngine.MonoBehaviour.StartCoroutine(Il2CppSystem.Collections.IEnumerator)'. How did you even get here? This method should only be called in an Il2Cpp environment.");
            }

            return (Coroutine) startCoroutineMethod.Invoke(mainThreadUtil, new[] { WrapToIl2Cpp(enumerator) })!;
        }

        private static object WrapToIl2Cpp(System.Collections.IEnumerator enumerator)
        {
            MethodInfo? wrapToIl2CppMethod = _wrapToIl2Cpp.Value;
            if (wrapToIl2CppMethod == null)
            {
                throw new InvalidOperationException("Could not find method 'BepInEx.Unity.IL2CPP.Utils.Collections.CollectionExtensions:WrapToIl2Cpp(System.Collections.IEnumerator)'");
            }

            return wrapToIl2CppMethod.Invoke(null, new object[] { enumerator })!;
        }
    }
}
