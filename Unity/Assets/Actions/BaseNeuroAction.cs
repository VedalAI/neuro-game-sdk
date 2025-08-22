#nullable enable

using System;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using UnityEngine;

namespace NeuroSdk.Actions
{
    public abstract class BaseNeuroAction : INeuroAction
    {
        /// <summary>
        /// Current action window that this action is a part of.
        /// </summary>
        public ActionWindow? ActionWindow { get; private set; }

        public abstract string Name { get; }
        protected abstract string Description { get; }
        protected abstract JsonSchema? Schema { get; }

        public virtual bool CanAddToActionWindow(ActionWindow actionWindow) => true;

        ExecutionResult INeuroAction.Validate(ActionJData actionData, out object? parsedData)
        {
            ExecutionResult result = Validate(actionData, out parsedData);

            if (ActionWindow != null)
            {
                return new ActionWindowResponse(ActionWindow).Result(result);
            }

            return result;
        }

        void INeuroAction.Execute(object? data) => Execute(data);

        public virtual WsAction GetWsAction()
        {
            return new WsAction(Name, Description, Schema);
        }

        protected abstract ExecutionResult Validate(ActionJData actionData, out object? parsedData);
        protected abstract void Execute(object? data);

        void INeuroAction.SetActionWindow(ActionWindow actionWindow)
        {
            if (ActionWindow != null)
            {
                if (ActionWindow != actionWindow)
                {
                    Debug.LogError("Cannot set the action window for this action, it is already set.");
                }

                return;
            }

            ActionWindow = actionWindow;
        }
    }
}
