#nullable enable

using Newtonsoft.Json;

namespace NeuroSdk.Internal
{
    internal static class Jason
    {
        public static string Serialize(object? value)
        {
            return JsonConvert.SerializeObject(value, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            });
        }
    }
}
