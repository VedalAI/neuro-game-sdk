using System;
using NeuroSdk.Websocket;

namespace NeuroSdk.Actions;

public sealed class ActionWindowResponse
{
    public ActionWindow ActionWindow { get; }

    public ActionWindowResponse(ActionWindow window) => ActionWindow = window;

    /// <summary>
    /// Run an <see cref="ExecutionResult"/> through this ActionWindow. This is invoked automatically in <see cref="NeuroAction"/>, but if you are not using that class you will need to invoke this manually.
    /// </summary>
    public ExecutionResult Result(ExecutionResult result)
    {
        if (ActionWindow.CurrentState <= ActionWindow.State.Building) throw new InvalidOperationException("Cannot handle a result before registering the ActionWindow.");
        if (ActionWindow.CurrentState >= ActionWindow.State.Ended) throw new InvalidOperationException("Cannot handle a result after the ActionWindow has ended.");

        if (result.Successful) ActionWindow.End();
        // else if (CurrentState == State.Forced) Force(); // Vedal is now responsible for retrying forces

        return result;
    }
}
