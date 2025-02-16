#nullable enable

using NeuroSdk.Actions;
using NeuroSdk.Messages.API;
using NeuroSdk.Websocket;

namespace NeuroSdk.Messages.Incoming
{
    // ReSharper disable once UnusedType.Global
    public sealed class ActionsReregisterAll : IncomingMessageHandler
    {
        public override bool CanHandle(string command) => command == "actions/reregister_all";

        protected override ExecutionResult Validate(string command, MessageJData messageData) => ExecutionResult.Success();

        protected override void ReportResult(ExecutionResult result)
        {
        }

        protected override void Execute()
        {
            NeuroActionHandler.ResendRegisteredActions();
        }
    }
}
