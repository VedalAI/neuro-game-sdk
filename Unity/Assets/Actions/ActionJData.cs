#nullable enable

using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NeuroSdk.Actions
{
    /// <summary>
    /// A wrapper class for the data of an <see cref="NeuroSdk.Messages.Incoming.Action"/> message.
    /// </summary>
    public sealed class ActionJData
    {
        public JToken? Data { get; private set; }

        private ActionJData()
        {
        }

        private void DeserializeFromJson(string? stringifiedData)
        {
            if (stringifiedData is null or "")
            {
                Data = null;
                return;
            }

            Data = JToken.Parse(stringifiedData);
        }

        internal static bool TryParse(string? stringifiedData, out ActionJData? actionJData)
        {
            try
            {
                actionJData = new ActionJData();
                actionJData.DeserializeFromJson(stringifiedData);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to deserialize ActionJData from string.");
                Debug.LogError(e.ToString());
                actionJData = null;
                return false;
            }
        }
    }
}
