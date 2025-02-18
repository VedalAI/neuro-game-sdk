#nullable enable

using Newtonsoft.Json.Linq;

namespace NeuroSdk.Json
{
    public interface IJTokenWrapper
    {
        JToken? Data { get; }
    }
}
