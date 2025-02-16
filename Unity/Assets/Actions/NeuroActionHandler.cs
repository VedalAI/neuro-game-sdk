#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NeuroSdk.Il2Cpp;
using NeuroSdk.Messages.Outgoing;
using NeuroSdk.Websocket;
using UnityEngine;

namespace NeuroSdk.Actions
{
#pragma warning disable CS0618 // Type or member is obsolete
    [RegisterInIl2Cpp]
#pragma warning restore CS0618 // Type or member is obsolete
    public sealed class NeuroActionHandler : MonoBehaviour
    {
        private static List<INeuroAction> _currentlyRegisteredActions = new();
        private static readonly List<INeuroAction> _dyingActions = new();

        public static INeuroAction? GetRegistered(string name) => _currentlyRegisteredActions.FirstOrDefault(a => a.Name == name);
        public static bool IsRecentlyUnregistered(string name) => _dyingActions.Any(a => a.Name == name);

        private void OnApplicationQuit()
        {
            WebsocketConnection.Instance!.SendImmediate(new ActionsUnregister(_currentlyRegisteredActions));
            _currentlyRegisteredActions = null!;
        }

        public static void RegisterActions(IReadOnlyCollection<INeuroAction> newActions)
        {
            _currentlyRegisteredActions.RemoveAll(oldAction => newActions.Any(newAction => oldAction.Name == newAction.Name));
            _dyingActions.RemoveAll(oldAction => newActions.Any(newAction => oldAction.Name == newAction.Name));
            _currentlyRegisteredActions.AddRange(newActions);
            WebsocketConnection.Instance!.Send(new ActionsRegister(newActions));
        }

        public static void RegisterActions(params INeuroAction[] newActions)
            => RegisterActions((IReadOnlyCollection<INeuroAction>) newActions);

        public static void UnregisterActions(IEnumerable<string> removeActionsList)
        {
            INeuroAction[] actionsToRemove = _currentlyRegisteredActions.Where(oldAction => removeActionsList.Any(removeAction => oldAction.Name == removeAction)).ToArray();

            _currentlyRegisteredActions.RemoveAll(actionsToRemove.Contains);
            _dyingActions.AddRange(actionsToRemove);

            WebsocketConnection connection = WebsocketConnection.Instance!;
            connection.StartCoroutine(removeActions());
            connection.Send(new ActionsUnregister(removeActionsList));

            return;

            IEnumerator removeActions()
            {
                yield return new WaitForSeconds(10);
                _dyingActions.RemoveAll(actionsToRemove.Contains);
            }
        }

        public static void UnregisterActions(IEnumerable<INeuroAction> removeActionsList)
            => UnregisterActions(removeActionsList.Select(a => a.Name));

        public static void UnregisterActions(params INeuroAction[] removeActionsList)
            => UnregisterActions((IReadOnlyCollection<INeuroAction>) removeActionsList);

        public static void UnregisterActions(params string[] removeActionNamesList)
            => UnregisterActions((IReadOnlyCollection<string>) removeActionNamesList);

        public static void ResendRegisteredActions()
        {
            WebsocketConnection.Instance!.Send(new ActionsRegister(_currentlyRegisteredActions));
        }
    }
}
