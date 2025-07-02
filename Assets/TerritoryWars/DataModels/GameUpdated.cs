using System;

namespace TerritoryWars.DataModels
{
    [Serializable]
    public struct GameUpdated
    {
        public string PlayerId;
        public GameModelStatus Status;

        public GameUpdated SetData(evolute_duel_GameCreated gameCreated)
        {
            PlayerId = gameCreated.host_player.Hex();
            Status = (GameModelStatus)gameCreated.status.Unwrap();
            return this;
        }

        public GameUpdated SetData(evolute_duel_GameCanceled gameCanceled)
        {
            PlayerId = gameCanceled.host_player.Hex();
            Status = GameModelStatus.Canceled;
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

