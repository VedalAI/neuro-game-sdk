#nullable enable

using System;
using NeuroSdk.Websocket;

namespace NeuroSdk.Actions
{
    /// <summary>
    /// Represents a NeuroAction with no parsed state
    /// </summary>
    public abstract class NeuroAction : BaseNeuroAction
    {
        protected abstract ExecutionResult Validate(ActionJData actionData);
        protected abstract void Execute();

        protected sealed override ExecutionResult Validate(ActionJData actionData, out object? parsedData)
        {
            ExecutionResult result = Validate(actionData);
            parsedData = null;
            return result;
        }

        protected sealed override void Execute(object? data) => Execute();
    }

    /// <summary>
    /// Represents a NeuroAction with a parsed state
    /// </summary>
    /// <typeparam name="TData">The type of the state parameter passed between <see cref="Validate(NeuroSdk.Actions.ActionJData,out TData?)"/> and <see cref="ExecuteAsync(TData?)"/></typeparam>
    public abstract class NeuroAction<TData> : BaseNeuroAction
    {
        protected abstract ExecutionResult Validate(ActionJData actionData, out TData? parsedData);
        protected abstract void Execute(TData? parsedData);

        protected sealed override ExecutionResult Validate(ActionJData actionData, out object? parsedData)
        {
            ExecutionResult result = Validate(actionData, out TData? tParsedData);
            parsedData = tParsedData;
            return result;
        }

        protected sealed override void Execute(object? parsedData) => Execute((TData?) parsedData);
    }

    /// <summary>
    /// Represents a NeuroAction with a parsed state that is a value type.
    /// Use this instead of <see cref="NeuroAction{TData}"/> when using primite types or structs to ensure proper nullability.
    /// </summary>
    /// <typeparam name="TData">The type of the state parameter passed between <see cref="NeuroAction{TData}.Validate(NeuroSdk.Actions.ActionJData,out TData?)"/> and <see cref="NeuroAction{TData}.ExecuteAsync(TData?)"/></typeparam>
    public abstract class NeuroActionS<TData> : NeuroAction<TData?> where TData : struct
    {
    }
}
