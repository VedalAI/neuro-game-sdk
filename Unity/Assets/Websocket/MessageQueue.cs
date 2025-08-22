#nullable enable

using System.Collections.Generic;
using NeuroSdk.Il2Cpp;
using NeuroSdk.Internal;
using NeuroSdk.Messages.API;
using NeuroSdk.Messages.Outgoing;
using UnityEngine;

namespace NeuroSdk.Websocket
{
#pragma warning disable CS0618 // Type or member is obsolete
    [RegisterInIl2Cpp]
#pragma warning restore CS0618 // Type or member is obsolete
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class MessageQueue : MonoBehaviour
    {
        // ReSharper disable once MemberCanBePrivate.Global
        protected readonly List<OutgoingMessageBuilder> Messages = new() { new Startup() };

        public virtual int Count
        {
            get
            {
                lock (Messages)
                {
                    return Messages.Count;
                }
            }
        }

        [Il2CppHide]
        public virtual void Enqueue(OutgoingMessageBuilder message)
        {
            lock (Messages)
            {
                foreach (OutgoingMessageBuilder existingMessage in Messages)
                {
                    if (existingMessage.Merge(message)) return;
                }

                Messages.Add(message);
            }
        }

        [Il2CppHide]
        public virtual OutgoingMessageBuilder? Dequeue()
        {
            lock (Messages)
            {
                if (Messages.Count == 0) return null;

                OutgoingMessageBuilder message = Messages[0];
                Messages.RemoveAt(0);

                return message;
            }
        }
    }
}
