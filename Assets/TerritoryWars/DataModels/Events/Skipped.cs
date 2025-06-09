using System;
using UnityEngine.Serialization;

namespace TerritoryWars.DataModels.Events
{
    [Serializable]
    public struct Skipped
    {
        public bool IsNull => String.IsNullOrEmpty(Id);
        
        public string Id;
        public string PlayerId;
        public string PrevMoveId;
        public string BoardId;
        public ulong Timestamp;

        public Skipped(string id, string playerId, string prevMoveId, string boardId, ulong timestamp)
        {
            Id = id;
            PlayerId = playerId;
            PrevMoveId = prevMoveId;
            BoardId = boardId;
            Timestamp = timestamp;
        }
        
        public Skipped SetData(evolute_duel_Skiped skiped)
        {
            Id = skiped.move_id.Hex();
            PlayerId = skiped.player.Hex();
            PrevMoveId = skiped.prev_move_id.Unwrap()?.Hex();
            BoardId = skiped.board_id.Hex();
            Timestamp = skiped.timestamp;
            return this;
        }
    }
}