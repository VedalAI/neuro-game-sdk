#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using NeuroSdk.Il2Cpp;
using NeuroSdk.Internal;
using NeuroSdk.Messages.Outgoing;
using NeuroSdk.Websocket;
using UnityEngine;

namespace NeuroSdk.Actions
{
    /// <summary>
    /// A wrapper class around the concept of an action window, which handles sending context, registering actions, forcing actions and unregistering the actions afterwards.
    /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
    [RegisterInIl2Cpp]
#pragma warning restore CS0618 // Type or member is obsolete
    public sealed class ActionWindow : MonoBehaviour
    {
        #region Creation

        private static bool _isCreatedCorrectly = false;

        /// <summary>
        /// Creates a new ActionWindow. If the parent is destroyed, this ActionWindow will be automatically ended.
        /// </summary>
        public static ActionWindow Create(GameObject parent)
        {
            try
            {
                _isCreatedCorrectly = true;
                return parent.AddComponent<ActionWindow>();
            }
            finally
            {
                _isCreatedCorrectly = false;
            }
        }

        private void Awake()
        {
            if (!_isCreatedCorrectly)
            {
                Debug.LogError("ActionWindow should be created using Create method. This ActionWindow was either created with AddComponent or with Instantiate.");
                Destroy(this);
            }
        }

        #endregion

        #region State

        public enum State
        {
            Building,
            Registered,
            Forced,
            Ended
        }

        public State CurrentState { get; private set; } = State.Building;

        private bool ValidateFrozen()
        {
            if (CurrentState != State.Building)
            {
                Debug.LogError("Cannot mutate ActionWindow after it has been registered.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Register this ActionWindow, sending an actions register to the websocket and making this window immutable.
        /// </summary>
        public void Register()
        {
            if (CurrentState != State.Building)
            {
                Debug.LogError("Cannot register an ActionWindow multiple times.");
                return;
            }

            if (_actions.Count == 0)
            {
                Debug.LogError("Cannot register an ActionWindow with no actions.");
                return;
            }

            if (_contextMessage is not (null or ""))
                Context.Send(_contextMessage, _contextSilent!.Value);
            NeuroActionHandler.RegisterActions(_actions);

            CurrentState = State.Registered;
        }

        #endregion

        #region Context

        private string? _contextMessage;
        private bool? _contextSilent;

        /// <summary>
        /// Set a context message to be sent alongside the action register.
        /// </summary>
        /// <returns>The <see cref="ActionWindow"/> itself for chaining.</returns>
        public ActionWindow SetContext(string message, bool silent = false)
        {
            if (!ValidateFrozen()) return this;

            _contextMessage = message;
            _contextSilent = silent;

            return this;
        }

        #endregion

        #region Actions

        private readonly List<INeuroAction> _actions = new();

        /// <summary>
        /// Add a new action to the list of possible actions that Neuro can pick from
        /// </summary>
        /// <returns>The <see cref="ActionWindow"/> itself for chaining.</returns>
        [Il2CppHide]
        public ActionWindow AddAction(INeuroAction action)
        {
            if (!ValidateFrozen()) return this;

            if (action.ActionWindow != null)
            {
                if (action.ActionWindow != this)
                {
                    Debug.LogError($"Cannot add action {action.Name} to this ActionWindow because it is already included in another ActionWindow.");
                }

                return this;
            }

            if (!action.CanAddToActionWindow(this)) return this;

            if (_actions.Any(a => a.Name == action.Name))
            {
                Debug.LogError($"Cannot add two actions with the same name to the same ActionWindow. Triggered by {action.Name}");
                return this;
            }

            action.SetActionWindow(this);
            _actions.Add(action);

            return this;
        }

        #endregion

        #region Forcing

        private Func<bool>? _shouldForceFunc;
        private Func<string>? _forceQueryGetter;
        private Func<string?>? _forceStateGetter;
        private bool? _forceEphemeralContext;

        /// <summary>
        /// Specify a condition under which the actions should be forced.
        /// </summary>
        /// <param name="shouldForce">When this returns true, the actions will be forced.</param>
        /// <param name="queryGetter">A getter for the query of the action force, invoked at force-time.</param>
        /// <param name="stateGetter">A getter for the state of the action force, invoked at force-time.</param>
        /// <param name="ephemeralContext">If true, the query and state won't be remembered after the action force is finished.</param>
        /// <returns>The <see cref="ActionWindow"/> itself for chaining.</returns>
        [Il2CppHide]
        public ActionWindow SetForce(Func<bool> shouldForce, Func<string> queryGetter, Func<string?> stateGetter, bool ephemeralContext = false)
        {
            if (!ValidateFrozen()) return this;

            _shouldForceFunc = shouldForce;
            _forceQueryGetter = queryGetter;
            _forceStateGetter = stateGetter;
            _forceEphemeralContext = ephemeralContext;

            return this;
        }

        /// <summary>
        /// Specify a condition under which the actions should be forced.
        /// </summary>
        /// <param name="shouldForce">When this returns true, the actions will be forced.</param>
        /// <param name="ephemeralContext">If true, the query and state won't be remembered after the action force is finished.</param>
        /// <returns>The <see cref="ActionWindow"/> itself for chaining.</returns>
        [Il2CppHide]
        public ActionWindow SetForce(Func<bool> shouldForce, string query, string? state, bool ephemeralContext = false)
            => SetForce(shouldForce, () => query, () => state, ephemeralContext);

        /// <summary>
        /// Specify a time in seconds after which the actions should be forced.
        /// </summary>
        /// <param name="queryGetter">A getter for the query of the action force, invoked at force-time.</param>
        /// <param name="stateGetter">A getter for the state of the action force, invoked at force-time.</param>
        /// <param name="ephemeralContext">If true, the query and state won't be remembered after the action force is finished.</param>
        /// <returns>The <see cref="ActionWindow"/> itself for chaining.</returns>
        [Il2CppHide]
        public ActionWindow SetForce(float afterSeconds, Func<string> queryGetter, Func<string?> stateGetter, bool ephemeralContext = false)
        {
            float time = afterSeconds;

            return SetForce(shouldForce, queryGetter, stateGetter, ephemeralContext);

            bool shouldForce()
            {
                // called every update
                time -= Time.deltaTime;
                return time <= 0;
            }
        }

        /// <summary>
        /// Specify a time in seconds after which the actions should be forced.
        /// </summary>
        /// <param name="ephemeralContext">If true, the query and state won't be remembered after the action force is finished.</param>
        /// <returns>The <see cref="ActionWindow"/> itself for chaining.</returns>
        public ActionWindow SetForce(float afterSeconds, string query, string? state, bool ephemeralContext = false)
            => SetForce(afterSeconds, () => query, () => state, ephemeralContext);

        public void Force()
        {
            if (CurrentState != State.Registered) return;

            CurrentState = State.Forced;
            _shouldForceFunc = null;
            WebsocketConnection.Instance!.Send(new ActionsForce(_forceQueryGetter!(), _forceStateGetter!(), _forceEphemeralContext, _actions));
        }

        #endregion

        #region Ending

        private Func<bool>? _shouldEndFunc;

        /// <summary>
        /// Specify a condition under which the actions should be unregistered and this window closed.
        /// </summary>
        /// <param name="shouldEnd">When this returns true, the actions will be unregistered.</param>
        /// <returns>The <see cref="ActionWindow"/> itself for chaining.</returns>
        [Il2CppHide]
        public ActionWindow SetEnd(Func<bool> shouldEnd)
        {
            if (!ValidateFrozen()) return this;

            _shouldEndFunc = shouldEnd;

            return this;
        }

        /// <summary>
        /// Specify a time in seconds after which the actions should be unregistered and this window closed.
        /// </summary>
        /// <returns>The <see cref="ActionWindow"/> itself for chaining.</returns>
        public ActionWindow SetEnd(float afterSeconds)
        {
            float time = afterSeconds;

            return SetEnd(shouldEnd);

            bool shouldEnd()
            {
                // called every update
                time -= Time.deltaTime;
                return time <= 0;
            }
        }

        private void OnDestroy()
        {
            if (CurrentState == State.Ended) return;
            End();
        }

        public void End()
        {
            if (CurrentState >= State.Ended) return;

            NeuroActionHandler.UnregisterActions(_actions);
            _shouldForceFunc = null;
            _shouldEndFunc = null;
            CurrentState = State.Ended;
            Destroy(this);
        }

        #endregion

        #region Handling

        private void Update()
        {
            if (CurrentState != State.Registered) return;

            if (_shouldForceFunc != null && _shouldForceFunc())
            {
                Force();
            }

            if (_shouldEndFunc != null && _shouldEndFunc())
            {
                End();
            }
        }

        #endregion
    }
}
