using System;

namespace TerritoryWars.DataModels
{
    [Serializable]
    public struct GameCreated
    {
        public string PlayerId;
        public GameModelStatus Status;

        public GameCreated SetData(evolute_duel_GameCreated gameCreated)
        {
            PlayerId = gameCreated.host_player.Hex();
            Status = (GameModelStatus)gameCreated.status.Unwrap();
            return this;
        }
    }

    public enum GameModelStatus
    {
        Finished = 0,
        Created = 1,
        Canceled = 2,
        InProgress = 3,
        None = 4
    }
}

