#nullable enable

using NeuroSdk.Websocket;

namespace NeuroSdk.Actions
{
    public interface INeuroAction
    {
        string Name { get; }

        ActionWindow? ActionWindow { get; }

        /// <summary>
        /// If this returns false, the action won't be added to the action window.
        /// </summary>
        bool CanAddToActionWindow(ActionWindow actionWindow);

        ExecutionResult Validate(ActionJData actionData, out object? data);
        void Execute(object? data);

        WsAction GetWsAction();

        /// <summary>
        /// Sets the active action window for this action. This will be called by the SDK and should not be used manually!
        /// </summary>
        void SetActionWindow(ActionWindow actionWindow);
    }
}
