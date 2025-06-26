using System;
using TerritoryWars.ConnectorLayers.WebSocketLayer;

namespace TerritoryWars.DataModels.WebSocketEvents
{
    [Serializable]
    public struct PingEvent
    {
        public string Address;
        public ulong Timestamp;
    }
}