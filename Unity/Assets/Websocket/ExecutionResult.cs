#nullable enable

using NeuroSdk.Internal;

namespace NeuroSdk.Websocket
{
    public sealed class ExecutionResult
    {
        public bool Successful { get; }
        public string? Message { get; }

        private ExecutionResult(bool success, string? message)
        {
            Successful = success;
            Message = message;
        }

        public static ExecutionResult Success(string? message = null) => new(true, message);
        public static ExecutionResult Failure(string reason) => new(false, reason);
        public static ExecutionResult VedalFailure(string reason) => Failure(reason + NeuroSdkStrings.VedalFaultSuffix);
        public static ExecutionResult ModFailure(string reason) => Failure(reason + NeuroSdkStrings.ModFaultSuffix);
    }
}
