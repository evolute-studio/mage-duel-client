using System;

namespace TerritoryWars.DataModels
{
    [Serializable]
    public struct GameModel
    {
        public bool IsNull => string.IsNullOrEmpty(Player);
        
        public string Player;
        public GameStatus Status;
        public string BoardId;
        public string Snapshot_id;

        public GameModel SetData(evolute_duel_Game game)
        {
            Player = game.player.Hex();
            Status = (GameStatus)game.status.Unwrap();
            BoardId = game.board_id.Unwrap()?.Hex();
            Snapshot_id = game.snapshot_id.Unwrap()?.Hex();
            return this;
        }
        
    }
    
    public enum GameStatus
    {
        Finished = 0,
        Created = 1,
        Canceled = 2,
        InProgress = 3,
    }
}