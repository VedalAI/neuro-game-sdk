#nullable enable

using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace NeuroSdk.Internal
{
    internal static class ClassInjectorPatch
    {
        private static readonly Lazy<MethodInfo?> _isMethodEligible = new(() => Type.GetType("Il2CppInterop.Runtime.Injection.ClassInjector, Il2CppInterop.Runtime")?.GetMethod("IsMethodEligible", BindingFlags.Static | BindingFlags.NonPublic));

        private static readonly Lazy<Type?> _harmony = new(() => Type.GetType("HarmonyLib.Harmony, 0Harmony"));
        private static readonly Lazy<MethodInfo?> _patch = new(() => _harmony.Value?.GetMethods().Where(m => m.Name == "Patch").OrderByDescending(m => m.GetParameters().Length).FirstOrDefault());
        private static readonly Lazy<Type?> _harmonyMethod = new (() => Type.GetType("HarmonyLib.HarmonyMethod, 0Harmony"));

        private static readonly MethodInfo _prefix = typeof(ClassInjectorPatch).GetMethod(nameof(Prefix), BindingFlags.Static | BindingFlags.NonPublic)!;

        public static void Patch()
        {
            MethodInfo? isMethodEligibleMethod = _isMethodEligible.Value;
            if (isMethodEligibleMethod == null) return;

            Type? harmonyType = _harmony.Value;
            if (harmonyType == null)
            {
                Debug.LogWarning("Type 'HarmonyLib.Harmony' not found. Expect Il2CppInterop warnings.");
                return;
            }

            MethodInfo? patchMethod = _patch.Value;
            if (patchMethod == null)
            {
                Debug.LogWarning("Method 'HarmonyLib.Harmony:Patch' not found. Expect Il2CppInterop warnings.");
                return;
            }

            Type? harmonyMethodType = _harmonyMethod.Value;
            if (harmonyMethodType == null)
            {
                Debug.LogWarning("Type 'HarmonyLib.HarmonyMethod' not found. Expect Il2CppInterop warnings.");
                return;
            }

            object harmonyInstance = Activator.CreateInstance(harmonyType, "NeuroSdkClassInjectorPatch")!;
            object harmonyMethodInstance = Activator.CreateInstance(harmonyMethodType, _prefix)!;
            patchMethod.Invoke(harmonyInstance, new[] { isMethodEligibleMethod, harmonyMethodInstance, null, null, null, null });
        }

        private static bool Prefix(ref bool __result, MethodInfo method)
        {
            if (method.CustomAttributes.Any(a => typeof(Il2CppHideAttribute).IsAssignableFrom(a.AttributeType)))
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}
