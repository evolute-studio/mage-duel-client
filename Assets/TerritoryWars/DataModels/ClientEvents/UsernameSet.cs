using System;

namespace TerritoryWars.DataModels.ClientEvents
{
    [Serializable]
    public struct UsernameSet
    {
        public string Username;
        
        public UsernameSet(string username)
        {
            Username = username;
        }
    }
}