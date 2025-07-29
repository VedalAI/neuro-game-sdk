#nullable enable

using NeuroSdk.Websocket;

namespace NeuroSdk.Messages.API
{
    public interface IIncomingMessageHandler
    {
        bool CanHandle(string command);
        ExecutionResult Validate(string command, MessageJData messageData, out object? parsedData);
        void ReportResult(object? parsedData, ExecutionResult result);
        void Execute(object? parsedData);
    }

    public abstract class IncomingMessageHandler : IIncomingMessageHandler
    {
        public abstract bool CanHandle(string command);
        protected abstract ExecutionResult Validate(string command, MessageJData messageData);
        protected abstract void ReportResult(ExecutionResult result);
        protected abstract void Execute();

        ExecutionResult IIncomingMessageHandler.Validate(string command, MessageJData messageData, out object? parsedData)
        {
            ExecutionResult result = Validate(command, messageData);
            parsedData = null;
            return result;
        }

        void IIncomingMessageHandler.ReportResult(object? parsedData, ExecutionResult result) => ReportResult(result);

        void IIncomingMessageHandler.Execute(object? parsedData) => Execute();
    }

    public abstract class IncomingMessageHandler<T> : IIncomingMessageHandler
    {
        public abstract bool CanHandle(string command);
        protected abstract ExecutionResult Validate(string command, MessageJData messageData, out T? parsedData);
        protected abstract void ReportResult(T? parsedData, ExecutionResult result);
        protected abstract void Execute(T? parsedData);

        ExecutionResult IIncomingMessageHandler.Validate(string command, MessageJData messageData, out object? parsedData)
        {
            ExecutionResult result = Validate(command, messageData, out T? tParsedData);
            parsedData = tParsedData;
            return result;
        }

        void IIncomingMessageHandler.ReportResult(object? parsedData, ExecutionResult result) => ReportResult((T?) parsedData, result);

        void IIncomingMessageHandler.Execute(object? parsedData) => Execute((T?) parsedData);
    }
}
