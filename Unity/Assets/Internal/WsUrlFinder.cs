#nullable enable

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace NeuroSdk.Internal
{
    internal static class WsUrlFinder
    {
        public static IEnumerator FindWsUrl(Action<string> callback)
        {
            string? url = null;
            TryGetWsUrlFromQuery(ref url);
            yield return TryGetWsUrlFromServer(url, result => url = result);
            TryGetWsUrlFromEnvironment(ref url);

            callback(url ?? "");
        }

        private static void TryGetWsUrlFromQuery(ref string? url)
        {
            try
            {
                if (Application.absoluteURL.IndexOf("?", StringComparison.Ordinal) == -1) return;

                string[] urlSplits = Application.absoluteURL.Split('?');
                if (urlSplits.Length <= 1) return;

                string[] urlParamSplits = urlSplits[1].Split(new[] { "WebSocketURL=" }, StringSplitOptions.None);
                if (urlParamSplits.Length <= 1) return;

                string param = urlParamSplits[1].Split('&')[0];
                if (string.IsNullOrEmpty(param)) return;

                url = param;
            }
            catch
            {
                // ignore
            }
        }

        private static IEnumerator TryGetWsUrlFromServer(string? url, Action<string> callback)
        {
            if (url is not null && url is not "") yield break;

            UnityWebRequest request;
            try
            {
                Uri uri = new(Application.absoluteURL);
                string requestUrl = $"{uri.Scheme}://{uri.Host}:{uri.Port}/$env/NEURO_SDK_WS_URL";
                request = UnityWebRequest.Get(requestUrl);
            }
            catch
            {
                yield break;
            }

            yield return request.SendWebRequest();

#pragma warning disable CS0618 // Type or member is obsolete
            if (request is { isDone: true, isHttpError: false, isNetworkError: false })
#pragma warning restore CS0618 // Type or member is obsolete
            {
                callback(request.downloadHandler.text);
            }
        }

        private static void TryGetWsUrlFromEnvironment(ref string? url)
        {
            if (url is not null && url is not "") return;

            url = Environment.GetEnvironmentVariable("NEURO_SDK_WS_URL", EnvironmentVariableTarget.Process) ??
                  Environment.GetEnvironmentVariable("NEURO_SDK_WS_URL", EnvironmentVariableTarget.User) ??
                  Environment.GetEnvironmentVariable("NEURO_SDK_WS_URL", EnvironmentVariableTarget.Machine);
        }
    }
}
