using UnityEngine;

namespace TerritoryWars.ConnectorLayers.WebSocketLayer
{
    public class WebSocketConfiguration
    {
        public string SocketUrl { get; private set; } = "wss://socket.evolute.network";
        
        public float PingInterval { get; private set; } = 5f;
        
        public Color OnlineStatusColor { get; private set; } = new Color(0.1411f, 0.8575f, 0.1861f, 1f);
        public Color OfflineStatusColor { get; private set; } = new Color(0.45f, 0.45f, 0.45f, 1f);
    }
    
    public enum WSChannels
    {
        Ping,
    }
}