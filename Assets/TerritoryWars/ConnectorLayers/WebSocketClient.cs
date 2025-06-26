
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NativeWebSocket;
using UnityEngine;

namespace TerritoryWars.ConnectorLayers
{


public static class WebSocketClient
{
    private static WebSocket websocket;

    [Serializable]
    private class Message
    {
        public string action;
        public string channel;
        public string payload;
    }

    [Serializable]
    private class IncomingMessage
    {
        public string channel;
        public string payload;
    }

    public static async void Initialize()
    {
        websocket = new WebSocket("ws://165.227.140.139:8080");

        websocket.OnOpen += () =>
        {
            Debug.Log("WebSocket opened");
            Subscribe("chat");
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError("WebSocket error: " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("WebSocket closed with: " + e);
        };

        websocket.OnMessage += (bytes) =>
        {
            var json = Encoding.UTF8.GetString(bytes);
            var msg = JsonUtility.FromJson<IncomingMessage>(json);
            Debug.Log($"Channel: '{msg.channel}': {msg.payload}");
        };

        await websocket.Connect();
    }

    public static async void Subscribe(string channelName)
    {
        var msg = new Message { action = "subscribe", channel = channelName };
        await SendJson(msg);
    }

    public static async void Unsubscribe(string channelName)
    {
        var msg = new Message { action = "unsubscribe", channel = channelName };
        await SendJson(msg);
    }

    public static async void Publish(string channelName, string payload)
    {
        var msg = new Message { action = "publish", channel = channelName, payload = payload };
        await SendJson(msg);
    }

    private static async Task SendJson(object obj)
    {
        if (websocket.State == WebSocketState.Open)
        {
            string json = JsonUtility.ToJson(obj);
            await websocket.SendText(json);
        }
    }

    private static async void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Publish("chat", "{\"text\":\"Test!\"}");
        }
    }

    public static async void Dispose()
    {
        await websocket.Close();
    }
}

}