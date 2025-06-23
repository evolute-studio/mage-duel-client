using System;

namespace TerritoryWars.DataModels.Events
{
    [Serializable]
    public struct ErrorOccured
    {
        public ServerErrorType ErrorType;
        public string Player;
        // InvalidMove
        public string MoveId;
        // NotYourTurn
        public string board_id;
        
        public ErrorOccured SetData(evolute_duel_InvalidMove invalidMove)
        {
            ErrorType = ServerErrorType.InvalidMove;
            MoveId = invalidMove.move_id.Hex();
            Player = invalidMove.player.Hex();
            return this;
        }
        
        public ErrorOccured SetData(evolute_duel_NotYourTurn notYourTurn)
        {
            ErrorType = ServerErrorType.NotYourTurn;
            Player = notYourTurn.player_id.Hex();
            board_id = notYourTurn.board_id.Hex();
            return this;
        }

        public ErrorOccured SetData(evolute_duel_GameJoinFailed joinFailed)
        {
            Player = joinFailed.host_player.Hex();
            ErrorType = ServerErrorType.GameJoinFailed;
            return this;
        }
    }
    
    [Serializable]
    public enum ServerErrorType
    {
        InvalidMove,
        NotYourTurn,
        GameJoinFailed,
    }
}