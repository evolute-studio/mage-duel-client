using System;

namespace TerritoryWars.DataModels.Events
{
    [Serializable]
    public struct ErrorOccured
    {
        public string ErrorType;
        public string Player;
        // InvalidMove
        public string MoveId;
        // NotYourTurn
        public string board_id;
        
        public ErrorOccured SetData(evolute_duel_InvalidMove invalidMove)
        {
            ErrorType = "InvalidMove";
            MoveId = invalidMove.move_id.Hex();
            Player = invalidMove.player.Hex();
            return this;
        }
        
        public ErrorOccured SetData(evolute_duel_NotYourTurn notYourTurn)
        {
            ErrorType = "NotYourTurn";
            Player = notYourTurn.player_id.Hex();
            board_id = notYourTurn.board_id.Hex();
            return this;
        }
    }
}