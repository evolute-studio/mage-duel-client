using System;

namespace TerritoryWars.DataModels.Events
{
    [Serializable]
    public struct GameFinished
    {
        public string PlayerId;
        public string BoardId;
        
        public GameFinished SetData(evolute_duel_GameFinished gameFinished)
        {
            PlayerId = gameFinished.host_player.Hex();
            BoardId = gameFinished.board_id.Hex();
            return this;
        }
        
        public GameFinished SetData(evolute_duel_GameIsAlreadyFinished gameFinished)
        {
            PlayerId = gameFinished.player_id.Hex();
            BoardId = gameFinished.board_id.Hex();
            return this;
        }
    }
}