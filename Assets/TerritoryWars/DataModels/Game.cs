using System;

namespace TerritoryWars.DataModels
{
    [Serializable]
    public struct Game
    {
        public string Player;
        public GameStatus Status;
        public string BoardId;
        public string Snapshot_id;
    }
    
    public enum GameStatus
    {
        WaitingForPlayers = 0,
        InProgress = 1,
        Finished = 2,
        Aborted = 3
    }
}