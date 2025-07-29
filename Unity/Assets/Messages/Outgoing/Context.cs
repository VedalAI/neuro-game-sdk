#nullable enable

using NeuroSdk.Messages.API;
using NeuroSdk.Websocket;
using Newtonsoft.Json;

namespace NeuroSdk.Messages.Outgoing
{
    public sealed class Context : OutgoingMessageBuilder
    {
        public Context(string message, bool silent = false)
        {
            Message = message;
            _silent = silent;
        }

        protected override string Command => "context";

        [JsonProperty("message", Order = 0)]
        public readonly string Message;

        [JsonProperty("silent", Order = 10)]
        private readonly bool _silent;

        public static void Send(string message, bool silent = false) => WebsocketConnection.Instance!.Send(new Context(message, silent));
    }
}
