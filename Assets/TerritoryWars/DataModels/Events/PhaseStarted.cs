using System;
using UnityEngine;

namespace TerritoryWars.DataModels.Events
{
    [Serializable]
    public struct PhaseStarted
    {
        public bool IsNull => string.IsNullOrEmpty(BoardId);
        
        public string BoardId;
        public SessionPhase Phase;
        public byte? TopTileIndex;
        public byte? CommitedTile;
        public ulong StartedAt;

        public PhaseStarted SetData(evolute_duel_PhaseStarted phaseStarted)
        {
            BoardId = phaseStarted.board_id.Hex();
            Phase = (SessionPhase)phaseStarted.phase;
            TopTileIndex = phaseStarted.top_tile.UnwrapByte();
            CommitedTile = phaseStarted.commited_tile.UnwrapByte();
            StartedAt = phaseStarted.started_at;
            return this;
        }
        
        public PhaseStarted SetData(Board board)
        {
            BoardId = board.Id;
            Phase = board.Phase;
            TopTileIndex = board.TopTileIndex;
            CommitedTile = board.CommitedTileIndex;
            StartedAt = board.LastUpdateTimestamp;
            return this;
        }
    }
    
    

    public enum SessionPhase
    {
        Creating = 0,
        Reveal = 1,
        Request = 2,
        Move = 3,
        Finished = 4
    }
}