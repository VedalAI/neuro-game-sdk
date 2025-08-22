#nullable enable

using NeuroSdk.Messages.API;

namespace NeuroSdk.Messages.Outgoing
{
    public sealed class Startup : OutgoingMessageBuilder
    {
        protected override string Command => "startup";
        protected override object? Data => null;
    }
}
