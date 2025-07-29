using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using NeonLadder.Debug;

namespace NeonLadder.WebRequests
{
    public static class WebRequestHandler
    {
        private static readonly string apiEndpoint = "https://yourapi.com/events";
        public static IEnumerator SendEventDataAsync(string eventName, float tick)
        {
            var eventData = new
            {
                eventName,
                tick,
                // Add other relevant data here
            };

            string jsonData = JsonUtility.ToJson(eventData);
            UnityWebRequest request = new UnityWebRequest(apiEndpoint, "PUT");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                NLDebug.LogError($"Error sending event data: {request.error}");
            }
            else
            {
                NLDebug.Log("Event data sent successfully!");
            }
        }
    }
}
