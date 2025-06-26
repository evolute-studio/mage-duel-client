using System;
using System.Text;
using System.Threading.Tasks;
using NativeWebSocket;
using TerritoryWars.Dojo;
using TerritoryWars.General;
using UnityEngine;

namespace TerritoryWars.ConnectorLayers.WebSocketLayer
{


public static class WebSocketClient
{
    private static WebSocket websocket;
    public static WebSocketConfiguration Configuration { get; private set; } = new WebSocketConfiguration();
    
    private static float _lastPingTime = 0f;

    [Serializable]
    private class Message
    {
        public string action;
        public string channel;
        public string payload;
    }

    [Serializable]
    public class IncomingMessage
    {
        public string channel;
        public string payload;
    }

    public static async void Initialize()
    {
        websocket = new WebSocket(Configuration.SocketUrl);

        websocket.OnOpen += () =>
        {
            Debug.Log("WebSocket opened");
            Subscribe(nameof(WSChannels.Ping));
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
            EventBus.Publish(msg);
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

    public static async void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Publish("chat", "{\"text\":\"Test!\"}");
        }
        
        
    }
    
    private static void PingEvent()
    {
        string accountAddress = DojoGameManager.Instance?.LocalAccount?.Address?.Hex();
        if (string.IsNullOrEmpty(accountAddress))
        {
            return;
        }
        Publish("ping", "{\"address\":\"Ping!\"}");
    }

    public static async void Dispose()
    {
        await websocket.Close();
    }
}

}