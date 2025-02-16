#nullable enable

using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using NativeWebSocket;
using NeuroSdk.Il2Cpp;
using NeuroSdk.Internal;
using NeuroSdk.Messages.API;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace NeuroSdk.Websocket
{
#pragma warning disable CS0618 // Type or member is obsolete
    [RegisterInIl2Cpp]
#pragma warning restore CS0618 // Type or member is obsolete
    public sealed class WebsocketConnection : MonoBehaviour
    {
        private const float RECONNECT_INTERVAL = 3;

        private static bool _checkingSelf;

        private static WebsocketConnection? _instance;
        public static WebsocketConnection? Instance
        {
            get
            {
                if (!_instance && !_checkingSelf) Debug.LogWarning("Accessed WebsocketConnection.Instance without an instance being present");
                _checkingSelf = false;
                return _instance;
            }
            private set => _instance = value;
        }

        private static WebSocket? _socket;

        public string game = "";
        public MessageQueue messageQueue = null!;
        public CommandHandler commandHandler = null!;

        public UnityEvent? onConnected;
        public UnityEvent<string>? onError;
        public UnityEvent<WebSocketCloseCode>? onDisconnected;

        private void Awake()
        {
            _checkingSelf = true;
            if (Instance)
            {
                Debug.Log("Destroying duplicate WebsocketConnection instance");
                Destroy(this);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Instance = this;

            Debug.Log("NeuroSdk WebsocketConnection is now awake");
        }

        // ReSharper disable once ArrangeThisQualifier -- Il2Cpp has this as an extension method
        private void Start() => this.StartCoroutine(StartWs());

        [Il2CppHide]
        private IEnumerator Reconnect()
        {
            yield return new WaitForSecondsRealtime(RECONNECT_INTERVAL);
            yield return StartWs();
        }

        [Il2CppHide]
        private IEnumerator StartWs()
        {
            if (MainThreadUtil.Instance == null) MainThreadUtil.Setup();

            try
            {
                if (_socket?.State is WebSocketState.Open or WebSocketState.Connecting) _ = _socket.Close();
            }
            catch
            {
                // ignored
            }

            string? websocketUrl = null;
            yield return WsUrlFinder.FindWsUrl(result => websocketUrl = result);

            if (websocketUrl is null or "")
            {
                string errMessage = "Could not retrieve websocket URL.";
#if UNITY_EDITOR || !UNITY_WEBGL
                errMessage += " You should set the NEURO_SDK_WS_URL environment variable.";
#endif
#if UNITY_WEBGL
                errMessage += " You need to specify a WebSocketURL query parameter in the URL or open a local server that serves the NEURO_SDK_WS_URL environment variable. See the documentation for more information.";
#endif
                Debug.LogError(errMessage);
                yield break;
            }

            // Websocket callbacks get run on separate threads! Watch out
            _socket = new WebSocket(websocketUrl);
            _socket.OnOpen += () =>
            {
                // ReSharper disable once ArrangeThisQualifier -- Il2Cpp has this as an extension method
                this.StartCoroutine(coroutine());
                return;

                IEnumerator coroutine()
                {
                    onConnected?.Invoke();
                    yield break;
                }
            };
            _socket.OnMessage += bytes =>
            {
                string message = Encoding.UTF8.GetString(bytes);
                // ReSharper disable once ArrangeThisQualifier -- Il2Cpp has this as an extension method
                this.StartCoroutine(ReceiveMessage(message));
            };
            _socket.OnError += error =>
            {
                // ReSharper disable once ArrangeThisQualifier -- Il2Cpp has this as an extension method
                this.StartCoroutine(coroutine());
                return;

                IEnumerator coroutine()
                {
                    onError?.Invoke(error);
                    if (error != "Unable to connect to the remote server")
                    {
                        Debug.LogError("Websocket connection has encountered an error!");
                        Debug.LogError(error);
                    }

                    yield break;
                }
            };
            _socket.OnClose += code =>
            {
                // ReSharper disable once ArrangeThisQualifier -- Il2Cpp has this as an extension method
                this.StartCoroutine(coroutine());
                return;

                IEnumerator coroutine()
                {
                    onDisconnected?.Invoke(code);
                    if (code != WebSocketCloseCode.Abnormal) Debug.LogWarning($"Websocket connection has been closed with code {code}!");
                    // ReSharper disable once ArrangeThisQualifier -- Il2Cpp has this as an extension method
                    this.StartCoroutine(Reconnect());
                    yield break;
                }
            };

            _ = _socket.Connect();
        }

        private void Update()
        {
            if (_socket?.State is not WebSocketState.Open) return;

            while (messageQueue.Count > 0)
            {
                OutgoingMessageBuilder builder = messageQueue.Dequeue()!;
                // ReSharper disable once ArrangeThisQualifier -- Il2Cpp has this as an extension method
                this.StartCoroutine(SendTask(builder));
            }

#if !UNITY_WEBGL || UNITY_EDITOR
            _socket.DispatchMessageQueue();
#endif
        }

        [Il2CppHide]
        private IEnumerator SendTask(OutgoingMessageBuilder builder)
        {
            string message = Jason.Serialize(builder.GetWsMessage());

            Debug.Log($"Sending ws message {message}");

            Task task = _socket!.SendText(message);
            // ReSharper disable once RedundantDelegateCreation
            yield return new WaitUntil(new Func<bool>(() => task.IsCompleted));

            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError($"Failed to send ws message {message}");
                messageQueue.Enqueue(builder);
            }
        }

        [Il2CppHide]
        public void Send(OutgoingMessageBuilder messageBuilder) => messageQueue.Enqueue(messageBuilder);

        [Il2CppHide]
        public void SendImmediate(OutgoingMessageBuilder messageBuilder)
        {
            string message = Jason.Serialize(messageBuilder.GetWsMessage());

            if (_socket?.State is not WebSocketState.Open)
            {
                Debug.LogError($"WS not open - failed to send immediate ws message {message}");
                return;
            }

            Debug.Log($"Sending immediate ws message {message}");

            _socket.SendText(message);
        }

        [Il2CppHide]
        private IEnumerator ReceiveMessage(string msgData)
        {
            try
            {
                Debug.Log("Received ws message " + msgData);

                JObject message = JObject.Parse(msgData);
                string? command = message["command"]?.Value<string>();
                MessageJData data = new(message["data"]);

                if (command == null)
                {
                    Debug.LogError("Received command that could not be deserialized. Wtf are you doing?");
                    yield break;
                }

                commandHandler.Handle(command, data);
            }
            catch (Exception e)
            {
                Debug.LogError("Received invalid message");
                Debug.LogError(e.ToString());
            }
        }
    }
}
