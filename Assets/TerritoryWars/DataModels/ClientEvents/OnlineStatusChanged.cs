using System;

namespace TerritoryWars.DataModels.ClientEvents
{
    [Serializable]
    public struct OnlineStatusChanged
    {
        public string Address;
        public bool IsOnline;
    }
}