using System;

namespace TerritoryWars.DataModels
{
    [Serializable]
    public struct SessionPlayer
    {
        public string PlayerId;
        public string Username;
        public byte PlayerSide;
        public byte JokerCount;
        public SessionPlayerScore Score;
        public byte ActiveSkin;

        public void Update(SessionPlayer player)
        {
            JokerCount = player.JokerCount;
            Score = player.Score;
        }

        public SessionPlayer SetData(PlayerProfile player)
        {
            Username = player.Username;
            ActiveSkin = player.ActiveSkin;
            return this;
        }
    }
}