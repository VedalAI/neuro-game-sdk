﻿#nullable enable

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace NeuroSdk.Json;

public static class JTokenWrapperExtensions
{
    public static T? GetValue<T>(this IJTokenWrapper data, string key, T? defaultValue = default)
    {
        JToken? jToken = data.Data?[key];
        return jToken != null ? jToken.Value<T>() : defaultValue;
    }

    public static IEnumerable<T?> GetArray<T>(this IJTokenWrapper data, string key)
    {
        return data.Data?[key]?.Values<T?>() ?? Enumerable.Empty<T?>();
    }
}
