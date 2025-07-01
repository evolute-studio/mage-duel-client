using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NativeWebSocket;
using TerritoryWars.DataModels.WebSocketEvents;
using TerritoryWars.Dojo;
using TerritoryWars.General;
using TerritoryWars.Tools;
using UnityEngine;

namespace TerritoryWars.ConnectorLayers.WebSocketLayer
{


public static class WebSocketClient
{
    private static WebSocket websocket;
    public static WebSocketConfiguration Configuration { get; private set; } = new WebSocketConfiguration();
    
    private static float _lastPingTime = 0f;
    private static float _connectionCheckInterval = 10f; // Check connection every 10 seconds

    private static List<string> subscribedChannels = new List<string>()
    {
        nameof(WSChannels.Ping)
    };

    public static OnlinePlayers OnlinePlayersData { get; private set; } = new OnlinePlayers();

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
        public string action;
        public string channel;
        public string payload;
    }

    [Serializable]
    public class OnlinePlayers
    {
        public PlayersListWrapper players;
        public bool[] onlineStatus;
        
        public Dictionary<string, bool> ToDictionary()
        {
            var dict = new Dictionary<string, bool>();
            for (int i = 0; i < players.players.Count; i++)
            {
                dict[players.players[i]] = onlineStatus[i];
            }
            return dict;
        }
    }
    [Serializable]
    public class PlayersListWrapper
    {
        public List<string> players;
    }

    public static async void Initialize()
    {
        websocket = new WebSocket(Configuration.SocketUrl);

        websocket.OnOpen += () =>
        {
            Debug.Log("WebSocket opened");
            // Subscribe to default channels
            foreach (var channel in subscribedChannels)
            {
                Subscribe(channel);
            }
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
            CustomLogger.LogImportant($"[WebSocketClient] Received message: {json}");
            var msg = JsonUtility.FromJson<IncomingMessage>(json);
            CustomLogger.LogImportant($"[WebSocketClient] Action: {msg.action}, Channel: {msg.channel}, Payload: {msg.payload}");
            EventBus.Publish(msg);
        };

        await websocket.Connect();
    }

    public static async void Subscribe(string channelName)
    {
        var msg = new Message { action = "subscribe", channel = channelName };
        if(!subscribedChannels.Contains(channelName)) subscribedChannels.Add(channelName);
        await SendJson(msg);
    }

    public static async void Unsubscribe(string channelName)
    {
        var msg = new Message { action = "unsubscribe", channel = channelName };
        if(subscribedChannels.Contains(channelName)) subscribedChannels.Remove(channelName);
        await SendJson(msg);
    }

    public static async void Publish(string channelName, string payload)
    {
        var msg = new Message { action = "publish", channel = channelName, payload = payload };
        await SendJson(msg);
    }

    public static async void Ping(PingEvent ping)
    {
        string payload = JsonUtility.ToJson(ping);
        var msg = new Message { action = "ping", payload = payload };
        await SendJson(msg);
    }

    public static async void CheckOnline(List<string> players)
    {
        CustomLogger.LogImportant($"[WebSocketClient] Checking online status for players: {string.Join(", ", players)}");
        OnlinePlayersData.players = new PlayersListWrapper { players = players };
    
        var wrapper = new PlayersListWrapper { players = players };
        string payload = JsonUtility.ToJson(wrapper);
        var msg = new Message { action = "check_online", payload = payload };
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
    }

    private static void CheckConnection()
    {
        if (websocket.State == WebSocketState.Closed || websocket.State == WebSocketState.Closing)
        {
            Debug.LogWarning("WebSocket is not connected, attempting to reconnect...");
            Initialize();
        }
    }

    public static async void Dispose()
    {
        await websocket.Close();
    }
}

}