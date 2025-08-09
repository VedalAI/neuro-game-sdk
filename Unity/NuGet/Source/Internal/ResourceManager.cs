#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace NeuroSdk.Internal
{
    internal static class ResourceManager
    {
        internal static void InjectAssemblies()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (_, args) => GetAssembly(new AssemblyName(args.Name).Name!);
        }

        private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();
        private static readonly Dictionary<string, object> _cache = [];

        private static Assembly? GetAssembly(string name)
        {
            string targetedName = name + ".dll";

            if (TryGetCached(targetedName, out Assembly? cached)) return cached;

            byte[]? buffer = GetEmbeddedBytes(targetedName, false);
            if (buffer == null) return null;

            Assembly assembly = Assembly.Load(buffer);

            Debug.Log("Loading embedded assembly " + targetedName);

            return Cache(targetedName, assembly);
        }

        private static byte[]? GetEmbeddedBytes(string name, bool throwIfNotFound)
        {
            string? path = _assembly.GetManifestResourceNames().FirstOrDefault(n => n.Contains(name));
            if (path == null) return !throwIfNotFound ? null : throw new FileNotFoundException($"Embedded resource {name} not found");

            Stream manifestResourceStream = _assembly.GetManifestResourceStream(path)!;
            return manifestResourceStream.ReadFully();
        }

        private static bool TryGetCached<T>(string name, out T? value) where T : class
        {
            if (!_cache.TryGetValue(name, out object? cachedObject))
            {
                value = null;
                return false;
            }

            value = cachedObject as T ?? throw new InvalidCastException($"Cached object is not of type {typeof(T)}");
            return true;
        }

        private static T Cache<T>(string name, T obj) where T : notnull
        {
            _cache.Add(name, obj);
            return obj;
        }

        private static byte[] ReadFully(this Stream stream)
        {
            using MemoryStream ms = new();
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
