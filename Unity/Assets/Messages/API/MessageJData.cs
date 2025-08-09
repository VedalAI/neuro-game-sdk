#nullable enable

using NeuroSdk.Json;
using Newtonsoft.Json.Linq;

namespace NeuroSdk.Messages.API
{
    public readonly struct MessageJData : IJTokenWrapper
    {
        public JToken? Data { get; }

        public MessageJData(JToken? data)
        {
            Data = data;
        }
    };
}
