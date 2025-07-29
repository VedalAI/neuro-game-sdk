#nullable enable

using NeuroSdk.Actions;
using NeuroSdk.Websocket;
using UnityEngine;

namespace NeuroSdk
{
    // ReSharper disable once PartialTypeWithSinglePart UnusedType.Global
    public static partial class NeuroSdkSetup
    {
        /// <summary>
        /// Use this only if you haven't already added the NeuroSdk prefab in your scenes.
        /// </summary>
        /// <param name="game"></param>
        // ReSharper disable once UnusedMember.Global
        public static void Initialize(string game)
        {
            GameObject obj = new("NeuroSdk") { hideFlags = HideFlags.HideAndDontSave };
            WebsocketConnection connection = obj.AddComponent<WebsocketConnection>();
            connection.game = game;
            connection.messageQueue = obj.AddComponent<MessageQueue>();
            connection.commandHandler = obj.AddComponent<CommandHandler>();
            obj.AddComponent<NeuroActionHandler>();
        }
    }
}
