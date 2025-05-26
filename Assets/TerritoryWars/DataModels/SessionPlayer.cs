using System;

namespace TerritoryWars.DataModels
{
    [Serializable]
    public struct SessionPlayer
    {
        public string PlayerId;
        public byte PlayerSide;
        public byte JokerCount;
        public SessionPlayerScore Score;
        public byte ActiveSkin;

        public void Update(byte jokerCount, SessionPlayerScore score)
        {
            JokerCount = jokerCount;
            Score = score;
        }
    }
}